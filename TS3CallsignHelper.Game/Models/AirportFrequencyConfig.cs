using System.Text.RegularExpressions;
using TS3CallsignHelper.Game.Exceptions;

namespace TS3CallsignHelper.Game.Models.Raw;
public partial class AirportFrequencyConfig {
  [GeneratedRegex("^(?<q0>\"?)(?<frequency>[0-9.]+?)\\k<q0>,(?<q1>\"?)(?<writename>.+?)\\k<q1>,(?<q2>\"?)(?<sayname>.+?)\\k<q2>,(?<q3>\"?)(?<readback>.+?)\\k<q3>,(?<q4>\"?)(?<controlarea>.+?)\\k<q4>$")]
  private static partial Regex Parser();

  public IEnumerable<AirportFrequency> DepartureFrequencies => _departureFrequencies;
  public IEnumerable<AirportFrequency> TowerFrequencies => _towerFrequencies;
  public IEnumerable<AirportFrequency> GroundFrequencies => _groundFrequencies;

  private readonly List<AirportFrequency> _departureFrequencies;
  private readonly List<AirportFrequency> _towerFrequencies;
  private readonly List<AirportFrequency> _groundFrequencies;

  public AirportFrequencyConfig(string configPath) {
    _departureFrequencies = new List<AirportFrequency>();
    _towerFrequencies = new List<AirportFrequency>();
    _groundFrequencies = new List<AirportFrequency>();
    var configFile = File.Open(configPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    using var reader = new StreamReader(configFile);
    reader.ReadLine(); // first line contains headers
    while (reader.ReadLine() is string line) {
      var groups = Parser().Match(line).Groups;
      if (groups.Count == 1) throw new FrequencyDefinitionFormatException(line);
      var writeName = groups["writeName"].Value.ToUpper();

      if (writeName.Contains("DEPARTURE") || writeName.Contains("CENTER"))
        _departureFrequencies.Add(new AirportFrequency(groups["frequency"].Value, groups["writename"].Value, groups["sayname"].Value, groups["readback"].Value, groups["controlarea"].Value));
      else if (writeName.Contains("TOWER"))
        _towerFrequencies.Add(new AirportFrequency(groups["frequency"].Value, groups["writename"].Value, groups["sayname"].Value, groups["readback"].Value, groups["controlarea"].Value));
      else if (writeName.Contains("GROUND") || writeName.Contains("APRON"))
        _groundFrequencies.Add(new AirportFrequency(groups["frequency"].Value, groups["writename"].Value, groups["sayname"].Value, groups["readback"].Value, groups["controlarea"].Value));
      else
        throw new UnknownFrequencyTypeException(writeName);
    }
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
