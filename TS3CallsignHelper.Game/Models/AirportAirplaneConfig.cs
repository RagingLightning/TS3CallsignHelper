using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using TS3CallsignHelper.Common.DTOs;
using TS3CallsignHelper.Common.Services;
using TS3CallsignHelper.Game.Exceptions;
using TS3CallsignHelper.Game.Services;

namespace TS3CallsignHelper.Game.Models;
public partial class AirportAirplaneConfig : IAirportAirplaneConfig {
  [GeneratedRegex("^(?<q0>\"?)(?<property>.+?)\\k<q0>,(?<q1>\"?)(?<value>.+?)\\k<q1>,(?<information>.+)$")]
  private static partial Regex Parser();
  private readonly ILogger<AirportAirplaneConfig>? _logger;

  public AirportAirplaneConfig(string configPath, InitializationProgressService initializationProgress) {
    _logger = LoggingService.GetLogger<AirportAirplaneConfig>();

    initializationProgress.StatusMessage = "Loading airplanes...";
    _logger?.LogDebug("Loading airplane set from {Config}", configPath);
    var airplaneCount = new DirectoryInfo(configPath).EnumerateDirectories().Count();
    foreach (var di in new DirectoryInfo(configPath).EnumerateDirectories()) {
      var airplaneConfig = Path.Combine(configPath, di.Name, di.Name + ".csv");

      _logger?.LogTrace("Loading airplane type {AirplaneType} from {Config}", di.Name, airplaneConfig);
      var configFile = File.Open(airplaneConfig, FileMode.Open, FileAccess.Read, FileShare.Read);
      using var reader = new StreamReader(configFile);
      reader.ReadLine(); // first line contains headers

      if (!MakeAirplane(di.Name, reader, out var airplane)) continue;

      _airplanes.Add(airplane.Code, airplane);
      _logger?.LogDebug("Added airplane type {@AirplaneType}", airplane);
      initializationProgress.AirplaneProgress += 1.0f / airplaneCount;
    }
  }

  private bool MakeAirplane(string code, StreamReader reader, out AirportAirplane result) {
    result = new AirportAirplane();
    try {
      while (reader.ReadLine() is string line) {
        var groups = Parser().Match(line).Groups;
        if (groups.Count == 1) throw new AirplaneDefinitionFormatException(line);

        result.SetProperty(groups["property"].Value.Trim(), groups["value"].Value.Trim());
      }
      return true;
    }
    catch (Exception ex) {
      _logger?.LogWarning("{Type}: {Key} is not a valid airplane property", code, ex.Message);
      return false;
    }
  }
}