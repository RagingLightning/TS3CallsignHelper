using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using TS3CallsignHelper.Game.Exceptions;
using TS3CallsignHelper.Game.Services;

namespace TS3CallsignHelper.Game.Models.Raw;
public partial class AirportFrequencyConfig {
  [GeneratedRegex("^(?<q0>\"?)(?<frequency>[0-9.]+?)\\k<q0>,(?<q1>\"?)(?<writename>.+?)\\k<q1>,(?<q2>\"?)(?<sayname>.+?)\\k<q2>,(?<q3>\"?)(?<readback>.+?)\\k<q3>,(?<q4>\"?)(?<controlarea>.+?)\\k<q4>$")]
  private static partial Regex Parser();
  private readonly ILogger<AirportFrequencyConfig> _logger;

  public ImmutableList<AirportFrequency> DepartureFrequencies => _departureFrequencies.ToImmutableList();
  public ImmutableList<AirportFrequency> TowerFrequencies => _towerFrequencies.ToImmutableList();
  public ImmutableList<AirportFrequency> GroundFrequencies => _groundFrequencies.ToImmutableList();

  private readonly List<AirportFrequency> _departureFrequencies;
  private readonly List<AirportFrequency> _towerFrequencies;
  private readonly List<AirportFrequency> _groundFrequencies;

  public AirportFrequencyConfig(string configPath, IServiceProvider serviceProvider, InitializationProgressService initializationProgress) {
    _logger = serviceProvider.GetRequiredService<ILogger<AirportFrequencyConfig>>();

    _departureFrequencies = new List<AirportFrequency>();
    _towerFrequencies = new List<AirportFrequency>();
    _groundFrequencies = new List<AirportFrequency>();

    initializationProgress.StatusMessage = "Loading frequencies...";
    _logger.LogDebug("Loading frequencies from {Config}", configPath);
    var stream = File.Open(configPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    using var reader = new StreamReader(stream);
    reader.ReadLine(); // first line contains headers
    while (reader.ReadLine() is string line) {
      _logger.LogTrace("Loading frequency from {Line}", line);
      var groups = Parser().Match(line).Groups;
      if (groups.Count == 1) throw new FrequencyDefinitionFormatException(line);

      var frequency = new AirportFrequency(groups["frequency"].Value, groups["writename"].Value, groups["sayname"].Value, groups["readback"].Value, groups["controlarea"].Value);
      var writeName = groups["writename"].Value.ToUpper();

      if (writeName.Contains("DEPARTURE") || writeName.Contains("CENTER")) {
        _departureFrequencies.Add(frequency);
        _logger.LogDebug("Added departure frequency {@Frequency}", frequency);
      }
      else if (writeName.Contains("TOWER")) {
        _towerFrequencies.Add(frequency);
        _logger.LogDebug("Added tower frequency {@Frequency}", frequency);
      } 
      else if (writeName.Contains("GROUND") || writeName.Contains("APRON")) {
        _groundFrequencies.Add(frequency);
        _logger.LogDebug("Added ground frequency {@Frequency}", frequency);
      }
      else
        throw new UnknownFrequencyTypeException(line);
      initializationProgress.FrequencyProgress = ((float) stream.Position) / stream.Length;
    }
    initializationProgress.FrequencyProgress = 1;
  }
}

public class AirportFrequency {
  public string Frequency { get; }
  public string WriteName { get; }
  public string SayName { get; }
  public string Readback { get; }
  public string ControlArea { get; }

  public AirportFrequency(string frequency, string writeName, string sayName, string readback, string controlArea) {
    Frequency = frequency;
    WriteName = writeName;
    SayName = sayName;
    Readback = readback;
    ControlArea = controlArea;
  }

}
