using System.Text.RegularExpressions;
using TS3CallsignHelper.Game.Exceptions;

namespace TS3CallsignHelper.Game.Models;
public partial class AirportAirlineConfig {
  [GeneratedRegex("^(?<q0>\"?)(?<airline>[A-Z]{3})\\k<q0>,(?<q1>\"?)(?<callsign>.+?)\\k<q1>,(?<q2>\"?)(?<name>.+?)\\k<q2>,(?<q3>\"?)(?<country>.+?)\\k<q3>$")]
  private static partial Regex Parser();

  public IEnumerable<AirportAirline> Airlines => _airlines.Values;

  private Dictionary<string, AirportAirline> _airlines;

  public AirportAirlineConfig(string configPath) {
    _airlines = new Dictionary<string, AirportAirline>();

    var configFile = File.Open(configPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    using var reader = new StreamReader(configFile);
    reader.ReadLine(); // first line contains headers
    while (reader.ReadLine() is string line) {
      var groups = Parser().Match(line).Groups;
      if (groups.Count == 1) throw new AirlineDefinitionFormatException(line);

      _airlines.Add(groups["airline"].Value, new AirportAirline(groups["airline"].Value, groups["callsign"].Value, groups["name"].Value, groups["country"].Value));
    }
  }

  public bool Contains(string code) => _airlines.ContainsKey(code);

  public bool TryGet(string code, out AirportAirline airline) {
    return _airlines.TryGetValue(code, out airline);
  }
}

public class AirportAirline {

  public string Code { get; }
  public string Callsign { get; }
  public string Name { get; }
  public string Country { get; }

  public AirportAirline(string airline, string callsign, string name, string country) {
    Code = airline;
    Callsign = callsign;
    Name = name;
    Country = country;
  }
}
