using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TS3CallsignHelper.Wpf.Extensions;
internal static class SerilogConfigExtension {
  private const string fileSection = "Serilog:WriteTo:FileSink:Args:path";
  private const string clefSection = "Serilog:WriteTo:ClefSink:Args:path";

  public static IConfigurationRoot ApplyTimestamp(this IConfigurationRoot config) {
    string timestamp = DateTime.Now.ToString("yyMMdd_HHmmss");
    if (config.GetSection(fileSection).Exists())
      config[fileSection] += $"{timestamp}.log";
    if (config.GetSection(clefSection).Exists())
      config[clefSection] += $"{timestamp}.log";
    return config;
  }

  public static void CleanFolder(this IConfigurationRoot config, int filesToKeep) {
    if (config.GetSection(fileSection).Exists() && !string.IsNullOrEmpty(config[fileSection]))
      foreach (var fi in new DirectoryInfo(config[fileSection]).EnumerateFiles("*.log").OrderByDescending(f => f.LastWriteTime).Skip(filesToKeep))
        fi.Delete();

    if (config.GetSection(clefSection).Exists() && !string.IsNullOrEmpty(config[clefSection]))
      foreach (var fi in new DirectoryInfo(config[clefSection]).EnumerateFiles("*.clef").OrderByDescending(f => f.LastWriteTime).Skip(filesToKeep))
        fi.Delete();
  }
}
