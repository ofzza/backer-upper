using backer_upper.services.archive;
using backer_upper.services.config;
using backer_upper.services.console;
using backer_upper.services.fs;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;

namespace backer_upper
{
  class Program
  {

    /// <summary>
    /// Program entry point
    /// </summary>
    /// <param name="args">Runtime arguments</param>
    static void Main(string[] args) => CommandLineApplication.Execute<Program>(args);

    [Option(
      CommandOptionType.SingleOrNoValue,
      ShortName = "b",
      LongName = "backup",
      Description = "Perform backup. Takes a path to a backup configuration file, or expects --source and --target arguments to be set"
    )]
    public (bool hasValue, string value) actionBackup { get; }

    [Option(
      CommandOptionType.SingleValue,
      ShortName = "s",
      LongName = "source",
      Description = "Backup source. Path to a directory (or if using wildcards, potentially multiple matched directories) that will have it's content backed up. Example \"c:/projects/*\""
    )]
    public string backupSource { get; }

    [Option(
      CommandOptionType.SingleValue,
      ShortName = "t",
      LongName = "target",
      Description = "Backup target. Path to a directory where backus will be written to."
    )]
    public string backupTarget { get; }

    [Option(
      CommandOptionType.MultipleValue,
      ShortName = "e",
      LongName = "exclude",
      Description = "Backup exclusion. Exclusion rules, which will cause files and paths matching any of them not to be included in the backup."
    )]
    public string[] backupExclusions { get; }

    [Option(
      CommandOptionType.SingleOrNoValue,
      ShortName = "r",
      LongName = "restore",
      Description = "Restore from backup"
    )]
    public (bool hasValue, string value) actionRestore { get; }

    /// <summary>
    /// Executed CLI command handler
    /// </summary>
    private void OnExecute () {

      // Check action
      if (this.actionBackup.hasValue) {

        // Prompt performing backup
        ConsoleService.WriteLine("Performing BACKUP ...", String.Format(" [{0}]", DateTime.Now.ToShortTimeString()), ConsoleColor.Green);

        // Process arguments
        if (this.actionBackup.value != null) {

          // Load configuration
          ConsoleService.WriteLine(
            new (string, ConsoleColor?)[] {
              ("- Loading backup configuration: ", ConsoleColor.White),
              (this.actionBackup.value, null)
            }
          );
          ConfigService.Load(this.actionBackup.value);

        } else if (this.actionRestore.value == null && this.backupSource != null && this.backupTarget != null) {

          // Set configuration
          ConfigService.config = new ConfigService.ConfigModel() {
            sources = new ConfigService.ConfigSourceModel[] {
              new ConfigService.ConfigSourceModel() {
                path    = this.backupSource,
                exclude = this.backupExclusions
              }
            }
          };

        } else {
          // Prompt missions arguments error
          ConsoleService.Error("Either a path to a backup configuration file or --source and --target arguments are required!");
          return;
        }

        // Prompt backup parameters
        foreach (ConfigService.ConfigSourceModel source in ConfigService.config.sources) {
          ConsoleService.WriteLine(
            new (string, ConsoleColor?)[] {
              ("  - Source path: ", ConsoleColor.White),
              ( source.path, null)
            }
          );
          if (source.exclude != null && source.exclude.Length > 0) {
            ConsoleService.WriteLine("      - Excluding:", ConsoleColor.White);
            foreach (string exclusion in source.exclude) {
              ConsoleService.WriteLine(String.Format("          \"{0}\"", exclusion));
            }
          }
        }

        // Perform backup
        Console.WriteLine();
        doBackup.execute();

      } else if (this.actionRestore.hasValue) {

        // Do restore

      } else {
        // Prompt no action error
        ConsoleService.Error("No action selected! Please specify either \"--backup\" or \"--restore\" action ...");
        return;
      }

    }
  }
}
