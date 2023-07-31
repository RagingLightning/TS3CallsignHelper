using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using TS3CallsignHelper.Common.DTOs;
using TS3CallsignHelper.Common.Services;
using TS3CallsignHelper.Game.Exceptions;
using TS3CallsignHelper.Game.Services;

namespace TS3CallsignHelper.Game.Models;
public partial class AirportAirlineConfig : IAirportAirlineConfig {
  [GeneratedRegex("^(?<q0>\"?)(?<airline>[A-Z]{3})\\k<q0>,(?<q1>\"?)(?<callsign>.+?)\\k<q1>,(?<q2>\"?)(?<name>.+?)\\k<q2>,(?<q3>\"?)(?<country>.+?)\\k<q3>$")]
  private static partial Regex Parser();

  public AirportAirlineConfig(string configPath, InitializationProgressService initializationProgress) {
    var logger = LoggingService.GetLogger<AirportAirlineConfig>();

    initializationProgress.StatusMessage = "Loading airlines...";
    logger?.LogDebug("Loading airlines from {Config}", configPath);
    var stream = File.Open(configPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    using var reader = new StreamReader(stream);
    reader.ReadLine(); // first line contains headers
    while (reader.ReadLine() is string line) {
      logger?.LogTrace("Loading airline from {Line}", line);
      var groups = Parser().Match(line).Groups;
      if (groups.Count == 1) throw new AirlineDefinitionFormatException(line);
      if (!MakeAirline(groups, out var airline)) continue;
      _airlines.Add(airline.Code, airline);
      logger?.LogDebug("Added Airline {@Airline}", airline);
      initializationProgress.AirlineProgess = ((float) stream.Position) / stream.Length;
    }
    initializationProgress.AirlineProgess = 1;
  }

  private bool MakeAirline(GroupCollection groups, out AirportAirline result) {
    result = new AirportAirline();

    var code = groups["airline"].Value;
    var callsign = groups["callsign"].Value;
    var name = groups["name"].Value;
    var country = groups["country"].Value;

    result.Code = code;
    result.Callsign = callsign;
    result.Name = name;
    result.Country = country;
    return true;
  }
}


