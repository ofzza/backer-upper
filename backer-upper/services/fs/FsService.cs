using backer_upper.services.config;
using System;
using System.IO;
using System.Collections.Generic;

namespace backer_upper.services.fs {
  
  /// <summary>
  /// Provides FS interaction functionality
  /// </summary>
  public class FsService {

    #region Static Methods (public)

    /// <summary>
    /// Scans source root directory
    /// </summary>
    /// <returns>Tuple of ([source configuration], [source root directory path], [files enumerator])</returns>
    public static IEnumerable<(ConfigService.ConfigSourceModel, string, IEnumerable<string>)> ScanSourceRootDirectory () {
      // Scan source directories
      foreach (ConfigService.ConfigSourceModel config in ConfigService.config.sources) {

        // Process path to extract source root depth
        string[] exclusions       = (config.exclude != null ? config.exclude : new string[0]);
        string[] parsedSourcePath = config.path.Replace("\\", "/").Split("/");

        //Get root directory and source depth
        int firstWidlcardIndex = Array.FindIndex<string>(parsedSourcePath, (p) => {
          return p.Contains("*");
        });
        if (firstWidlcardIndex == 0) {
          throw new Exception("Source path must start with a non-wildcard path!");
        }
        uint depth  = (firstWidlcardIndex == -1 ? 0 : (uint)(parsedSourcePath.Length - firstWidlcardIndex));
        string root = FsService.FormatDirectoryPathString(
          String.Join("/", (new ArraySegment<string>(parsedSourcePath, 0, (firstWidlcardIndex == -1 ? parsedSourcePath.Length : firstWidlcardIndex))))
        );

        // Get all source directories and act on those matching source path
        foreach (string dir in GetSourceDirectories(root, exclusions, depth)) {

          // Check if directory matches source path
          if (firstWidlcardIndex >= 0) {
            // Parse scanned directory
            string[] parsedDir = dir.Replace("\\", "/").Split("/");
            // Check if directory has fewer levels than seource path
            if (parsedDir.Length < parsedSourcePath.Length) {
              continue;
            }
            // Check if directory directory mismatches source path at any level
            for (int i = 0; i < parsedSourcePath.Length; i++) {
              if (parsedSourcePath[i] != "*" && parsedDir[i] != parsedSourcePath[i]) {
                continue;
              }
            }
          }

          // Yield found source root directory
          yield return (config, dir, ScanForFilesInSourceRootDirectory(dir, exclusions));
        }

      }
    }

    /// <summary>
    /// Formats a directory path to use "/" as delimiters
    /// </summary>
    /// <param name="path">Path to format</param>
    /// <returns>Formatted path</returns>
    public static string FormatDirectoryPathString (string path) {
      path = path.Replace("\\", "/");
      return (path.EndsWith("/") ? path : path + "/");
    }

    /// <summary>
    /// Formats a file path to use "/" as delimiters
    /// </summary>
    /// <param name="path">Path to format</param>
    /// <returns>Formatted path</returns>
    public static string FormatFilePathString (string path) {
      return path.Replace("\\", "/");
    }

    #endregion

    #region Static Methods (private)

    /// <summary>
    /// Finds all directories (recursivelly, up to given depth) in a given path, excluding those matching given exclusions
    /// </summary>
    /// <param name="path">Path to search in</param>
    /// <param name="exclusions">Array of exclusion patterns</param>
    /// <param name="depth">Maxximum depth of recursive search</param>
    /// <returns>All directories found in the given path</returns>
    private static IEnumerable<string> GetSourceDirectories (string path, string[] exclusions, uint depth) {
      // Check if directory path mattches an exclusion rule
      bool excluded = null != Array.Find<string>(exclusions, (exclusion) => {
        return (path.Contains(FormatFilePathString(exclusion)));
      });
      if (!excluded) {
        if (depth == 0) {

          // Yield directory
          yield return FormatDirectoryPathString(path);

        } else {

          // Process direct children
          string[] dirs = new string[0];
          try { dirs = Directory.GetDirectories(path); } catch (Exception) { }
          foreach (string dir in dirs) {
            // Yield nested directories
            foreach (string childDir in GetSourceDirectories(dir, exclusions, depth - 1)) {
              yield return childDir;
            };
          }

        }
      }
    }

    /// <summary>
    /// Finds all files (recursivelly) in a given path, excluding those matching given exclusions
    /// </summary>
    /// <param name="path">Path to search in</param>
    /// <param name="exclusions">Array of exclusion patterns</param>
    /// <returns>All files found in the given path</returns>
    private static IEnumerable<string> GetFiles (string path, string[] exclusions) {

      // Append files
      string[] files = new string[0];
      try { files = Directory.GetFiles(path); } catch (Exception) { }
      foreach (string file in files) {
        // Check if file path mattches an exclusion rule
        string filePath = FormatFilePathString(file);
        bool excluded = null != Array.Find<string>(exclusions, (exclusion) => {
          return (filePath.Contains(FormatFilePathString(exclusion)));
        });
        if (!excluded) {
          // Yield file
          yield return filePath;
        }
      }

      // Append files nested in child directories
      string[] dirs = new string[0];
      try { dirs = Directory.GetDirectories(path); } catch (Exception) { }
      foreach (string dir in dirs) {
        // Check if directory path mattches an exclusion rule
        bool excluded = null != Array.Find<string>(exclusions, (exclusion) => {
          return (dir.Contains(FormatFilePathString(exclusion)));
        });
        if (!excluded) {
          // Yield nested files
          foreach (string filePath in GetFiles(dir, exclusions)) {
            yield return filePath;
          }
        }
      }

    }

    /// <summary>
    /// Scans for files in a given directory, excluding those whose path matches any of the speciified exclusion patterns
    /// </summary>
    /// <param name="dir">Direcotry to scan in</param>
    /// <param name="exclusions">Array of patterns to exclude files based on</param>
    /// <returns>Found files</returns>
    private static IEnumerable<string> ScanForFilesInSourceRootDirectory (string dir, string[] exclusions) {

      // Scan directory for fiels
      foreach (string file in GetFiles(dir, exclusions)) {
        // Yield scanned files
        if (file.Substring(0, dir.Length) == dir) {
          yield return file.Substring(dir.Length);
        } else {
          throw new Exception("Somehow file is not located inside the directory being scanned!");
        }
      }

    }

    #endregion

  }
}
