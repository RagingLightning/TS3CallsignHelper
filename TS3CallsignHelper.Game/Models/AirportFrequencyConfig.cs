using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using TS3CallsignHelper.Common.DTOs;
using TS3CallsignHelper.Common.Services;
using TS3CallsignHelper.Game.Exceptions;
using TS3CallsignHelper.Game.Services;

namespace TS3CallsignHelper.Game.Models;
public partial class AirportFrequencyConfig : IAirportFrequencyConfig{
  [GeneratedRegex("^(?<q0>\"?)(?<frequency>[0-9.]+?)\\k<q0>,(?<q1>\"?)(?<writename>.+?)\\k<q1>,(?<q2>\"?)(?<sayname>.+?)\\k<q2>,(?<q3>\"?)(?<readback>.+?)\\k<q3>,(?<q4>\"?)(?<controlarea>.+?)\\k<q4>$")]
  private static partial Regex Parser();
  private readonly ILogger<AirportFrequencyConfig>? _logger;

  public AirportFrequencyConfig(string configPath, InitializationProgressService initializationProgress) {
    _logger = LoggingService.GetLogger<AirportFrequencyConfig>();

    initializationProgress.StatusMessage = "Loading frequencies...";
    _logger?.LogDebug("Loading frequencies from {Config}", configPath);
    var stream = File.Open(configPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    using var reader = new StreamReader(stream);
    reader.ReadLine(); // first line contains headers
    while (reader.ReadLine() is string line) {
      _logger?.LogTrace("Loading frequency from {Line}", line);
      var groups = Parser().Match(line).Groups;
      if (groups.Count == 1) throw new FrequencyDefinitionFormatException(line);

      if (!MakeFrequency(groups, out var frequency)) continue;

      switch (frequency.Type) {
        case AirportFrequencyType.DEPARTURE:
          _departureFrequencies.Add(frequency.Frequency, frequency);
          break;
        case AirportFrequencyType.TOWER:
          _towerFrequencies.Add(frequency.Frequency, frequency);
          break;
        case AirportFrequencyType.GROUND:
          _groundFrequencies.Add(frequency.Frequency, frequency);
          break;
      }
      _logger?.LogDebug("Added frequency {@Frequency}", frequency);
      initializationProgress.FrequencyProgress = ((float) stream.Position) / stream.Length;
    }
    initializationProgress.FrequencyProgress = 1;
  }

  private bool MakeFrequency(GroupCollection groups, out AirportFrequency result) {
    result = new AirportFrequency();
    var frequency = groups["frequency"].Value;
    var writename = groups["writename"].Value;
    var sayname = groups["sayname"].Value;
    var readback = groups["readback"].Value;
    var controlArea = groups["controlarea"].Value;

    var typeCheck = writename.ToUpper();
    AirportFrequencyType type;
    if (typeCheck.Contains("DEPARTURE") || typeCheck.Contains("CENTER"))
      type = AirportFrequencyType.DEPARTURE;
    else if (typeCheck.Contains("TOWER"))
      type = AirportFrequencyType.TOWER;
    else if (typeCheck.Contains("GROUND") || typeCheck.Contains("APRON"))
      type = AirportFrequencyType.GROUND;
    else {
      _logger?.LogWarning("Could not determine frequency type: {Frequency}", writename);
      return false;
    }

    result.Type = type;
    result.Frequency = frequency;
    result.Writename = writename;
    result.Sayname = sayname;
    result.Readback = readback;
    result.ControlArea = controlArea;
    return true;
  }
}
