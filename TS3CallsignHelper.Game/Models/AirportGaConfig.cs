using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using TS3CallsignHelper.Common.DTOs;
using TS3CallsignHelper.Common.Services;
using TS3CallsignHelper.Game.Exceptions;
using TS3CallsignHelper.Game.Services;

namespace TS3CallsignHelper.Game.Models;
public partial class AirportGaConfig : IAirportGaConfig {
  [GeneratedRegex("^(?<q0>\"?)(?<writename>.+?)\\k<q0>,(?<q1>\"?)(?<sayname>.+?)\\k<q1>,(?<q2>\"?)(?<airplanetype>.+?)\\k<q2>,(?<q3>\"?)(?<from>[A-Z]{4})\\k<q3>,(?<q4>\"?)(?<to>[A-Z]{4})\\k<q4>,(?<q5>\"?)(?<arrival>[0-9:]*?)\\k<q5>,(?<q6>\"?)(?<departure>[0-9:]*?)\\k<q6>,(?<q7>\"?)(?<approachaltitude>[0-9]*?)\\k<q7>,(?<q8>\"?)(?<stopandgo>\\d)\\k<q8>,(?<q9>\"?)(?<touchandgo>\\d)\\k<q9>,(?<q10>\"?)(?<lowapproach>\\d)\\k<q10>,(?<q11>\"?)(?<special>.*?)\\k<q11>$")]
  private static partial Regex Parser();
  private readonly ILogger<AirportGaConfig>? _logger;

  public AirportGaConfig(string configPath, InitializationProgressService initializationProgress, IAirportAirplaneConfig airplaneConfig) {
    _logger = LoggingService.GetLogger<AirportGaConfig>();

    initializationProgress.StatusMessage = "Loading GA schedule...";
    _logger?.LogDebug("Loading ga schedule from {Config}", configPath);
    var stream = File.Open(configPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    using var reader = new StreamReader(stream);
    reader.ReadLine(); // first line contains headers
    while (reader.ReadLine() is string line) {
      _logger?.LogTrace("Loading ga flight from {Line}", line);
      var groups = Parser().Match(line).Groups;
      if (groups.Count == 1) throw new GaDefinitionFormatException(line);

      if (!MakeGaPlane(groups, airplaneConfig, out var gaPlane)) continue;

      _gaPlanes.Add(gaPlane.Writename, gaPlane);
      _logger?.LogDebug("Added ga schedule entry {@GaSchedule}", gaPlane);
      initializationProgress.GaProgress = ((float) stream.Position) / stream.Length;
    }
    initializationProgress.GaProgress = 1;
  }

  private bool MakeGaPlane(GroupCollection groups, IAirportAirplaneConfig airplaneConfig, out AirportGa result) {
    result = new AirportGa();

    var writename = groups["writename"].Value;
    var sayname = groups["sayname"].Value;
    if (!airplaneConfig.TryGet(groups["airplanetype"].Value, out var airplaneType)) {
      _logger?.LogWarning("{Callsign}: Airplane type {Type} does not exist", writename, groups["airplanetype"].Value);
      return false;
    }
    var origin = groups["from"].Value;
    var destination = groups["to"].Value;
    DateTime? arrival = null;
    if (groups["arrival"].Value != string.Empty) {
      if (!DateTime.TryParse(groups["arrival"].Value, out var arr)) {
        _logger?.LogWarning("{Callsign}: Failed to parse arrival time {Time}", writename, groups["arrival"].Value);
        return false;
      }
      arrival = arr;
    }
    DateTime? departure = null;
    if (groups["departure"].Value != string.Empty) {
      if (!DateTime.TryParse(groups["departure"].Value, out var dep)) {
        _logger?.LogWarning("{Callsign}: Failed to parse departure time {Time}", writename, groups["departure"].Value);
        return false;
      }
      departure = dep;
    }
    if (!int.TryParse(groups["approachaltitude"].Value, out var approachAltitude) && groups["approachaltitude"].Value != string.Empty) {
      _logger?.LogWarning("{Callsign}: Failed to parse approach altitude {Altitude}", writename, groups["approachaltitude"].Value);
      return false;
    }
    var stopAndGo = groups["stopandgo"].Value == "1";
    var touchAndGo = groups["touchandgo"].Value == "1";
    var lowApproach = groups["lowapproach"].Value == "1";
    var special = groups["special"].Value;

    result.Writename = writename;
    result.Sayname = sayname;
    result.AirplaneType = airplaneType;
    result.Origin = origin;
    result.Destination = destination;
    result.Arrival = arrival;
    result.Departure = departure;
    result.ApproachAltitude = approachAltitude;
    result.StopAndGo = stopAndGo;
    result.TouchAndGo = touchAndGo;
    result.LowApproach = lowApproach;
    result.Special = special;
    return true;
  }
}