using System;
using System.Collections.Generic;
using System.Text;

namespace backer_upper.services.console {

  public class ConsoleService {

    private static ConsoleColor? defaultColor { get; set; } = null;

    public static void WriteLine (string leftText, int lines = 1, bool newLine = true) {
      ConsoleService.WriteLine(
        leftText,
        null,
        null,
        lines,
        newLine
      );
    }
    public static void WriteLine (string leftText, ConsoleColor? color, int lines = 1, bool newLine = true) {
      ConsoleService.WriteLine(
        leftText,
        null,
        color,
        lines,
        newLine
      );
    }
    public static void WriteLine (string leftText, string rightText, int lines = 1, bool newLine = true) {
      ConsoleService.WriteLine(
        leftText,
        rightText,
        null,
        lines,
        newLine
      );
    }
    public static void WriteLine (string leftText, string rightText, ConsoleColor? color, int lines = 1, bool newLine = true) {
      ConsoleService.WriteLine(
        (leftText != null ? new (string, ConsoleColor?)[] {(leftText, color) } : null),
        (rightText != null ? new(string, ConsoleColor?)[] { (rightText, color) } : null),
        color,
        lines,
        newLine
      );
    }
    public static void WriteLine ((string, ConsoleColor?)[] leftText, int lines = 1, bool newLine = true) {
      ConsoleService.WriteLine(
        leftText,
        null,
        null,
        lines,
        newLine
      );
    }
    public static void WriteLine ((string, ConsoleColor?)[] leftText, ConsoleColor? color, int lines = 1, bool newLine = true) {
      ConsoleService.WriteLine(
        leftText,
        null,
        color,
        lines,
        newLine
      );
    }
    public static void WriteLine ((string, ConsoleColor?)[] leftText, (string, ConsoleColor?)[] rightText, int lines = 1, bool newLine = true) {
      ConsoleService.WriteLine(
        leftText,
        rightText,
        null,
        lines,
        newLine
      );
    }
    public static void WriteLine ((string, ConsoleColor?)[] leftText, (string, ConsoleColor?)[] rightText, ConsoleColor? color, int lines = 1, bool newLine = true) {

      // Check if default color set
      if (ConsoleService.defaultColor == null) {
        ConsoleService.defaultColor = Console.ForegroundColor;
      }

      // Check layout
      if (rightText != null) {

        // Left/right layout
        int leftLength = 0,
            rightLength = 0;
        if (leftText != null) foreach ((string, ConsoleColor?) section in leftText) { leftLength += section.Item1.Replace("\n", "").Replace("\r", "").Replace("\t", "").Length; }
        if (rightText != null) foreach ((string, ConsoleColor?) section in rightText) { rightLength += section.Item1.Replace("\n", "").Replace("\r", "").Replace("\t", "").Length; }
        int padding = (lines * Console.WindowWidth) - leftLength - rightLength;

        foreach ((string, ConsoleColor?) segment in leftText) {
          Console.ForegroundColor = (ConsoleColor)(segment.Item2 != null ? segment.Item2 : (color != null ? color : ConsoleService.defaultColor));
          Console.Write(segment.Item1);
        }
        if (padding > 0) {
          Console.ForegroundColor = ConsoleColor.DarkGray;
          Console.Write("".PadLeft(padding, '.'));
        }
        foreach ((string, ConsoleColor?) segment in rightText) {
          Console.ForegroundColor = (ConsoleColor)(segment.Item2 != null ? segment.Item2 : (color != null ? color : ConsoleService.defaultColor));
          Console.Write(segment.Item1);
        }
        if (newLine) {
          Console.WriteLine();
        }

      } else if (lines == 1 && rightText == null) {

        // Single-line, linear layout
        foreach ((string, ConsoleColor?) segment in leftText) {
          Console.ForegroundColor = (ConsoleColor)(segment.Item2 != null ? segment.Item2 : (color != null ? color : ConsoleService.defaultColor));
          Console.Write(segment.Item1);
        }
        if (newLine) {
          Console.WriteLine();
        }

      } else if (lines > 1 && rightText == null) {

        // Multi line, linear layout
        foreach ((string, ConsoleColor?) segment in leftText) {
          Console.ForegroundColor = (ConsoleColor)(segment.Item2 != null ? segment.Item2 : (color != null ? color : ConsoleService.defaultColor));
          Console.Write(segment.Item1.PadRight(Console.WindowWidth, ' '));
        }
        if (newLine) {
          Console.WriteLine();
        }

      }

      // Reset console color
      Console.ForegroundColor = (ConsoleColor)ConsoleService.defaultColor;

    }

    public static void Error (string leftText) {
      ConsoleService.Error(
        new string[] {  leftText }
      );
    }
    public static void Error (string leftText, string rightText) {
      ConsoleService.Error(
        new string[] { leftText },
        new string[] { rightText }
      );
    }
    public static void Error (string[] leftText) {
      ConsoleService.Error(
        leftText,
        null
      );
    }
    public static void Error (string[] leftText, string[] rightText) {

      // Check if default color set
      if (ConsoleService.defaultColor == null) {
        ConsoleService.defaultColor = Console.ForegroundColor;
      }

      // Write box
      Console.ForegroundColor = ConsoleColor.Red;
      //Console.WriteLine("\r");
      //Console.WriteLine("");
      //Console.WriteLine(" ERROR ".PadLeft(10, '=').PadRight(Console.WindowWidth, '='));
      ConsoleService.WriteLine(
        (leftText != null ? "\rERROR: " + String.Concat(leftText) : null),
        (rightText != null ? String.Concat(rightText) : null),
        ConsoleColor.Red
      );
      Console.ForegroundColor = ConsoleColor.Red;
      //Console.WriteLine("".PadLeft(Console.WindowWidth, '='));
      //Console.WriteLine();

      // Reset console color
      Console.ForegroundColor = (ConsoleColor)ConsoleService.defaultColor;

    }

  }

}
