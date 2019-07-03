using backer_upper.services.fs;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace backer_upper.services.archive {

  /// <summary>
  /// Provides interaction with archive files
  /// </summary>
  public class Archive {

    #region Properties

    /// <summary>
    /// Default compression level to use
    /// </summary>
    public static CompressionLevel compression { get; set; } = CompressionLevel.Fastest;

    /// <summary>
    /// Path of the archive directory
    /// </summary>
    private string archivePath { get; set; }
    /// <summary>
    /// Archive filename
    /// </summary>
    private string archiveName { get; set; }

    /// <summary>
    /// Path of the archive filename
    /// </summary>
    private string archiveFileFullPath { get; set; }

    /// <summary>
    /// Internal ZipArchive instance
    /// </summary>
    private ZipArchive zip { get; set; }

    /// <summary>
    /// Holds reference to current log instance for this archive
    /// </summary>
    private ArchiveLog currentLog { get; set; }
    /// <summary>
    /// Holds reference to previous arcive log instance
    /// </summary>
    private ArchiveLog previousLog { get; set; }

    /// <summary>
    /// Holds status of the archive having no files written to it
    /// </summary>
    private bool empty { get; set; } = true;

    #endregion

    #region Constructors

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="archivePath">Path of the archive directory</param>
    /// <param name="contentPath">Path of the source root directory being archived</param>
    /// <param name="readPreviousArchiveLog">If true, previous archive will be loaded (if found) and only changed files will be (re)archived</param>
    public Archive (string archivePath, string contentPath, bool readPreviousArchiveLog) {

      // Compose and set properties
      this.archivePath = FsService.FormatDirectoryPathString(
        Path.Join(
          archivePath,
          (contentPath.EndsWith("/") ? contentPath.Substring(0, contentPath.Length - 1) : contentPath).Replace(":", "").Replace(' ', '_').Replace('/', ' ')
        )
      );
      this.archiveName = String.Format(
        "{0} ({1:0000}-{2:00}-{3:00} {4:00}-{5:00}-{6:00})",
        DateTime.UtcNow.ToFileTimeUtc(),
        DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second
      );
      string path = String.Format("{0}{1}", this.archivePath, this.archiveName);
      this.archiveFileFullPath = String.Format("{0}.zip", path);

      // Initialize logs (load previous log firs, or previous will equal current)
      if (readPreviousArchiveLog) {
        this.previousLog = ArchiveLog.GetLastArchiveLog(this.archivePath);
        if (this.previousLog != null) {
          this.previousLog.ReadAllEntries();
        }
      }
      this.currentLog = new ArchiveLog(String.Format("{0}.log", path));

    }

    #endregion

    #region Methods (public)

    /// <summary>
    /// Creates a new archive
    /// </summary>
    public void Create () {

      // Create archive file
      Directory.CreateDirectory(Path.GetDirectoryName(this.archiveFileFullPath));
      FileStream zipStream = File.OpenWrite(this.archiveFileFullPath);
      this.zip = new ZipArchive(zipStream, ZipArchiveMode.Create, false);

      // Create log file
      this.currentLog.Create();

    }

    /// <summary>
    /// Writes a file to archive
    /// </summary>
    /// <param name="dirPath">Path to the root directory of the file being archived</param>
    /// <param name="filePath">Relative path within the root directory of the file being archived</param>
    /// <returns>File status</returns>
    public ArchiveLogFileArchivingStatus WriteFile (string dirPath, string filePath) {
      if (this.zip == null) {
        throw new Exception("Can't write to archive before opening it! Call .Create() first!");
      } else {

        // Get file info
        string          fullPath      = String.Format("{0}{1}", dirPath, filePath);
        FileInfo        info          = new FileInfo(fullPath);
        ArchiveLogEntry currentEntry  = new ArchiveLogEntry() {
          path  = filePath,
          mtime = (info.CreationTimeUtc > info.LastWriteTimeUtc ? info.CreationTimeUtc : info.LastWriteTimeUtc).ToFileTimeUtc(),
          size  = info.Length,
        };

        // Check if file changed since last backup        
        ArchiveLogEntry previousEntry = (this.previousLog != null ? this.previousLog.FindEntry(filePath) : null);
        if (previousEntry != null) {
          if ((previousEntry.status != ArchiveLogFileArchivingStatus.DELETED) && (currentEntry.mtime == previousEntry.mtime) && (currentEntry.size == previousEntry.size)) {
            // File unchanged - set status and copy archive path
            currentEntry.status = ArchiveLogFileArchivingStatus.UNCHANGED;
            currentEntry.archive = previousEntry.archive;
          } else if (previousEntry.status == ArchiveLogFileArchivingStatus.DELETED) {
            // File recreated - set status and archive path
            currentEntry.status = ArchiveLogFileArchivingStatus.CREATED;
            currentEntry.archive = this.archiveName;
          } else {
            // File changed - set status and archive path
            currentEntry.status = ArchiveLogFileArchivingStatus.CHANGED;
            currentEntry.archive = this.archiveName;
          }
        } else {
          // File created - set status and archive path
          currentEntry.status = ArchiveLogFileArchivingStatus.CREATED;
          currentEntry.archive = this.archiveName;
        }

        // Check if file needs to be archived
        if ((currentEntry.status == ArchiveLogFileArchivingStatus.CREATED) || (currentEntry.status == ArchiveLogFileArchivingStatus.CHANGED)) {
          // Add file entry to archive
          zip.CreateEntryFromFile(fullPath, filePath, Archive.compression);
          // Updated empty status
          this.empty = false;
        }

        // Log file entry information
        this.currentLog.WriteEntry(currentEntry);

        // Return file status
        return currentEntry.status;

      }
    }

    /// <summary>
    /// Disposes of archive
    /// </summary>
    /// <returns>Number of deleted files</returns>
    public long Dispose () {

      // Write missing/deleted files to log
      long deletedCount = 0;
      if (this.previousLog != null) {
        deletedCount = this.previousLog.TransferMissingEntries(this.currentLog);
      }

      // Dispose of archive
      if (this.zip != null) {
        this.zip.Dispose();
        this.zip = null;
      }
      // Delete archive file if not needed
      if (this.empty) {
        File.Delete(this.archiveFileFullPath);
      }

      // Dispose of log files
      this.currentLog.Dispose();
      if (this.previousLog != null) {
        this.previousLog.Dispose();
      }

      // Return number of deleted files
      return deletedCount;

    }

    #endregion

  }
}
