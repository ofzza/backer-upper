using backer_upper.services.archive;
using backer_upper.services.config;
using backer_upper.services.console;
using backer_upper.services.fs;
using System;
using System.Collections.Generic;
using System.Text;

namespace backer_upper {

  public class doBackup {

    public static void execute () {

      // Track errors
      List<string> failedReadingFiles = new List<string>();

      // Fetch source files
      ConsoleService.WriteLine(
        new (string, ConsoleColor?)[] {
          ("- Scanning ", ConsoleColor.White),
        },
        new (string, ConsoleColor?)[] {
          (String.Format(" [{0}]", DateTime.Now.ToShortTimeString()), ConsoleColor.Green),
        }
      );
      foreach ((ConfigService.ConfigSourceModel, string, IEnumerable<string>) source in FsService.ScanSourceRootDirectory()) {

        // Get yielded values
        ConfigService.ConfigSourceModel config  = source.Item1;
        IEnumerable<string> filesGenerator      = source.Item3;
        string path                             = source.Item2;

        // Ready target archive
        Archive archive = new Archive("e:/_temp/backup/", path, true);
        archive.Create();

        // Scan source root directory for files
        long  countTotal                = 0,
              countCreated              = 0,
              countUnchanged            = 0,
              countChanged              = 0,
              countDeleted              = 0,
              lastStatusLineTimestamp   = 0;
        foreach (string file in filesGenerator) {

          // Prompt scanning status line
          if (lastStatusLineTimestamp < (DateTime.Now.Ticks - 1e7)) {
            ConsoleService.WriteLine(
              new (string, ConsoleColor?)[] {
                ("\r", null),
                ("  ... scanning archive root directory ", ConsoleColor.Green),
                (path, ConsoleColor.White),
                (" for files: ", ConsoleColor.Green),
                (String.Format("{0} files processed ", countTotal), ConsoleColor.White)
              },
              new (string, ConsoleColor?)[] {
                (String.Format("{0,6}", countCreated), ConsoleColor.White),
                (" created, ", null),
                (String.Format("{0,6}", countChanged), ConsoleColor.White),
                (" changed, ", null),
                (String.Format("{0,6}", countUnchanged), ConsoleColor.White),
                (" unchanged, ", null),
                (String.Format("{0,6}", countUnchanged), ConsoleColor.White),
                (" deleted", null),
                (String.Format(" [{0}]", DateTime.Now.ToShortTimeString()), ConsoleColor.Green),
              },
              1,
              false
            );
            lastStatusLineTimestamp = DateTime.Now.Ticks;
          }

          // Add file to archive
          try {
            ArchiveLogFileArchivingStatus status = archive.WriteFile(path, file);
            countTotal++;
            if (status == ArchiveLogFileArchivingStatus.CREATED) {
              countCreated++;
            } else if (status == ArchiveLogFileArchivingStatus.UNCHANGED) {
              countUnchanged++;
            } else if (status == ArchiveLogFileArchivingStatus.CHANGED) {
              countChanged++;
            } else if (status == ArchiveLogFileArchivingStatus.DELETED) {
              countDeleted++;
            }
          } catch (Exception ex) {
            failedReadingFiles.Add(String.Format("{0}{1}", path, file));
            ConsoleService.Error(String.Format("Failed reading and archiving file \"{0}\"", file));
          }
        }

        // Dispose of archive
        countDeleted += archive.Dispose();
        // Prompt scanned source root directory
        ConsoleService.WriteLine(
          new (string, ConsoleColor?)[] {
            ("\r", null),
            ("  - archive root ", ConsoleColor.White),
            (path, null),
            (": ", ConsoleColor.White),
            (String.Format("{0} files archived ", countTotal), ConsoleColor.White)
          },
          new (string, ConsoleColor?)[] {
            (String.Format("{0,6}", countCreated), ConsoleColor.White),
            (" created, ", null),
            (String.Format("{0,6}", countChanged), ConsoleColor.White),
            (" changed, ", null),
            (String.Format("{0,6}", countUnchanged), ConsoleColor.White),
            (" unchanged, ", null),
            (String.Format("{0,6}", countDeleted), ConsoleColor.White),
            (" deleted", null),
            (String.Format(" [{0}]", DateTime.Now.ToShortTimeString()), ConsoleColor.Green),
          }
        );

      };

      // Prompt done
      ConsoleService.WriteLine(
        new (string, ConsoleColor?)[] {
          ("\n\n", null),
          ("DONE! ", ConsoleColor.Green)
        },
        new (string, ConsoleColor?)[] {
          (String.Format(" [{0}]", DateTime.Now.ToShortTimeString()), ConsoleColor.Green),
        }
      );

      // If errors, list errors
      if (failedReadingFiles.Count > 0) {
        ConsoleService.WriteLine(
          new (string, ConsoleColor?)[] {
            ("\n", null),
            (String.Format("WARNING: Skipped {0} files: ", failedReadingFiles.Count), ConsoleColor.Red)
          }
        );
        foreach (string file in failedReadingFiles) {
          ConsoleService.WriteLine(String.Format("    {0}", file));
        }
      }

    }

  }

}
