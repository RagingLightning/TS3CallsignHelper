using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;

namespace TS3CallsignHelper.Wpf.Extensions;
internal static class SerilogConfigExtensions {
  private const string fileSection = "Serilog:WriteTo:FileSink:Args:path";
  private const string clefSection = "Serilog:WriteTo:ClefSink:Args:path";

  public static IConfigurationRoot ApplyTimestamp(this IConfigurationRoot config) {
    string timestamp = DateTime.Now.ToString("yyMMdd_HHmmss");
    if (config.GetSection(fileSection).Exists())
      config[fileSection] += $"{timestamp}.log";
    if (config.GetSection(clefSection).Exists())
      config[clefSection] += $"{timestamp}.clef";
    return config;
  }

  public static void CleanFolder(this IConfigurationRoot config, int filesToKeep) {
    try {
      if (config.GetSection(fileSection).Exists() && !string.IsNullOrEmpty(config[fileSection]))
        foreach (var fi in new DirectoryInfo(Path.GetDirectoryName(config[fileSection].Replace("%temp%", Environment.GetEnvironmentVariable("temp")))).EnumerateFiles("*.log").OrderByDescending(f => f.LastWriteTime).Skip(filesToKeep))
          fi.Delete();
    }
    catch (Exception) { }

    try {
      if (config.GetSection(clefSection).Exists() && !string.IsNullOrEmpty(config[clefSection]))
      foreach (var fi in new DirectoryInfo(Path.GetDirectoryName(config[clefSection].Replace("%temp%", Environment.GetEnvironmentVariable("temp")))).EnumerateFiles("*.clef").OrderByDescending(f => f.LastWriteTime).Skip(filesToKeep))
        fi.Delete();
    }
    catch (Exception) { }
  }
}
