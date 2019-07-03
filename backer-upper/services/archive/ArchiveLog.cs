using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace backer_upper.services.archive {

  /// <summary>
  /// Enumerates possible file states for logged, achived files
  /// </summary>
  public enum ArchiveLogFileArchivingStatus {
    CREATED,
    UNCHANGED,
    CHANGED,
    DELETED
  }

  /// <summary>
  /// Provides interaction with archive log files
  /// </summary>
  public class ArchiveLog {

    #region Properties

    /// <summary>
    /// Holds path to archive log file
    /// </summary>
    private string path { get; set; }

    /// <summary>
    /// Holds reference to writable file stream to the archive log file
    /// </summary>
    private FileStream stream { get; set; }

    /// <summary>
    /// Holds status of the log being empty (if empty, when disposed log file can be deleted)
    /// </summary>
    private bool empty { get; set; } = true;

    /// <summary>
    /// Holds a dictionary of log entries, keyed by path
    /// </summary>
    private Dictionary<string, ArchiveLogEntry> entries = new Dictionary<string, ArchiveLogEntry>();

    #endregion

    #region Static Methods

    /// <summary>
    /// Finds last existing log file for the same source root and returns it as ArchiveLog if found
    /// </summary>
    /// <param name="dirPath">Path in which to search</param>
    /// <returns>Last existing log file for the same source root as ArchiveLog</returns>
    public static ArchiveLog GetLastArchiveLog (string dirPath) {

      // Check if target directory exists
      if (!Directory.Exists(dirPath)) { return null; }

      // Find last log file in the target directory
      string[] files = Directory.GetFiles(dirPath, "*.log");
      string logFile = null;
      DateTime logFileArchivalTime = DateTime.MinValue;
      foreach (string file in files) {
        DateTime archivalTime = DateTime.FromFileTimeUtc(long.Parse(Path.GetFileNameWithoutExtension(file).Split(' ')[0]));
        if ((logFileArchivalTime == null) || (logFileArchivalTime < archivalTime)) {
          logFile = file;
          logFileArchivalTime = archivalTime;
        }
      }

      // If log file found, parse log file
      if (logFile != null) {
        return new ArchiveLog(logFile);
      } else {
        return null;
      }

    }

    #endregion

    #region Constructors

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="path">Path to archive log file</param>
    public ArchiveLog (string path) {
      // Set properties
      this.path = path;
    }

    #endregion

    #region Methods (public)

    /// <summary>
    /// Creates a new archive log
    /// </summary>
    public void Create () {
      // Initialize a file stream
      this.stream = File.OpenWrite(this.path);
    }

    /// <summary>
    /// Writes an entry into the archive log
    /// </summary>
    /// <param name="entry"></param>
    public void WriteEntry (ArchiveLogEntry entry) {

      // Write entry to file
      byte[] logLine = Encoding.UTF8.GetBytes(
        String.Format("{0}\n", entry.ToString())
      );
      this.stream.Write(logLine, 0, logLine.Length);

      // Store entry
      this.entries.Add(entry.path, entry);

      // Update empty status
      this.empty = false;

    }

    /// <summary>
    /// Disposes of archive log
    /// </summary>
    public void Dispose () {

      // Dispose of file stream
      if (this.stream != null) {
        this.stream.Close();
        this.stream.Dispose();
      }

      // If empty, delete log file
      if (this.empty) {
        File.Delete(this.path);
      }

      // Clear loaded entries
      this.entries.Clear();

    }

    /// <summary>
    /// Reads all entries from existing archive log file
    /// </summary>
    public void ReadAllEntries () {
      // Read entries
      FileStream stream = File.OpenRead(this.path);
      while (stream.Position < (stream.Length - 1)) {

        // Read line
        List<byte> encodedLine = new List<byte>();
        while (true) {
          byte b = (byte)stream.ReadByte();
          if (b == '\n') {
            break;
          } else {
            encodedLine.Add(b);
          }
        }

        // Process line into an entry and store
        ArchiveLogEntry entry = new ArchiveLogEntry(
          Encoding.UTF8.GetString(encodedLine.ToArray())
        );
        this.entries.Add(entry.path, entry);

      }
      // Mark as not empty
      this.empty = false;
    }

    /// <summary>
    /// Searches for a stored entry by it's path
    /// </summary>
    /// <param name="path">Entry path to search by</param>
    /// <returns>Found, stored entry</returns>
    public ArchiveLogEntry FindEntry (string path) {
      if (this.entries.ContainsKey(path)) {
        return this.entries[path];
      } else {
        return null;
      }
    }

    /// <summary>
    /// Transfers unmatched entries from this log to another
    /// </summary>
    /// <param name="log">Log to match entries agains and transfer unmatched entries into</param>
    /// <returns>Number of entries transfered</returns>
    public long TransferMissingEntries (ArchiveLog log) {
      long count = 0;
      // Loop all entries
      foreach (ArchiveLogEntry entry in this.entries.Values) {
        // Check if entry not present in target log
        if (log.FindEntry(entry.path) == null) {
          // Write entry into new log as deleted
          log.WriteEntry(new ArchiveLogEntry() {
            path    = entry.path,
            status  = ArchiveLogFileArchivingStatus.DELETED,
            mtime   = entry.mtime,
            size    = entry.size,
            archive = entry.archive
          });
        }
      }
      return count;
    }

    #endregion

  }

  /// <summary>
  /// Single aarchive log file line/entry
  /// </summary>
  public class ArchiveLogEntry {

    #region Properties
    /// <summary>
    /// File path on disk
    /// </summary>
    public string path { get; set; }

    /// <summary>
    ///  File archiving status
    /// </summary>
    public ArchiveLogFileArchivingStatus status { get; set; }

    /// <summary>
    /// File last-modified time
    /// </summary>
    public long mtime { get; set; }

    /// <summary>
    /// File size
    /// </summary>
    public long size { get; set; }

    /// <summary>
    /// Archive filename where file was last stored
    /// </summary>
    public string archive { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Constructor
    /// </summary>
    public ArchiveLogEntry () { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="text">String representation of the entry to be parsed</param>
    public ArchiveLogEntry (string text) {
      // Parse string representation
      string[] parsed = text.Split('|');
      this.path       = parsed[0];
      this.status     = (ArchiveLogFileArchivingStatus)Enum.Parse(typeof(ArchiveLogFileArchivingStatus), parsed[1], true);
      this.mtime      = long.Parse(parsed[2]);
      this.size       = long.Parse(parsed[3]);
      this.archive    = parsed[4];
    }

    #endregion

    #region Methods

    /// <summary>
    /// Converts log entry into a string representation
    /// </summary>
    /// <returns>String representation of the entry</returns>
    public override string ToString () {
      // Return string representation
      return String.Format(
        "{0}|{1}|{2}|{3}|{4}",
        this.path,
        this.status.ToString(),
        this.mtime,
        this.size,
        this.archive
      );
    }

    #endregion

  }

}
