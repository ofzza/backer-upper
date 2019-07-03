using System.Text.Json;
using System.Text.Json.Serialization;

namespace backer_upper.services.config {

  /// <summary>
  /// Provides configuration management
  /// </summary>
  public class ConfigService {

    #region Properties

    /// <summary>
    /// Holds loaded configuration
    /// </summary>
    static public ConfigModel config { get; set; }

    #endregion

    #region Static Methods
    
    /// <summary>
    /// Loads configuration from a JSON config file
    /// </summary>
    /// <param name="path">Path to the JSON config file</param>
    static public void Load (string path) {

      // Read configuration file
      config = JsonSerializer.Parse<ConfigModel>(
        System.IO.File.ReadAllText(path),
        new JsonSerializerOptions {
          AllowTrailingCommas = true,
          ReadCommentHandling = JsonCommentHandling.Skip
        }
      );

    }

    #endregion

    #region Model

    /// <summary>
    /// Configuration
    /// </summary>
    public class ConfigModel {

      /// <summary>
      /// List of sources
      /// </summary>
      public ConfigSourceModel[] sources { get; set; }

    }

    /// <summary>
    /// Holds backup source configuration
    /// </summary>
    public class ConfigSourceModel {

      /// <summary>
      /// Source path
      /// </summary>
      public string path { get; set; }

      /// <summary>
      /// Path exclusion criteria
      /// </summary>
      public string[] exclude { get; set; }

    }

    #endregion

  }

}
