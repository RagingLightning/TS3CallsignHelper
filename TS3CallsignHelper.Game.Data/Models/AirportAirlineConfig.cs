using System.Text.RegularExpressions;
using TS3CallsignHelper.Game.Exceptions;

namespace TS3CallsignHelper.Game.Models;
public partial class AirportAirlineConfig {
  [GeneratedRegex("^(?<q0>\"?)(?<airline>[A-Z]{4})\\k<q0>,(?<q1>\"?)(?<callsign>.+?)\\k<q1>,(?<q2>\"?)(?<name>.+?)\\k<q2>,(?<q3>\"?)(?<country>.+?)\\k<q3>$")]
  private static partial Regex Parser();

  public IEnumerable<AirportAirline> Airlines => _airlines;

  private List<AirportAirline> _airlines;

  public AirportAirlineConfig(string configPath) {
    _airlines = new List<AirportAirline>();

    var configFile = File.Open(configPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    using var reader = new StreamReader(configFile);
    reader.ReadLine(); // first line contains headers
    while (reader.ReadLine() is string line) {
      var groups = Parser().Match(line).Groups;
      if (groups.Count == 1) throw new AirlineDefinitionFormatException(line);

      _airlines.Add(new AirportAirline(groups["airline"].Value, groups["callsign"].Value, groups["name"].Value, groups["country"].Value));
    }
}

  public class AirportAirline {

    public string Airline { get; }
    public string Callsign { get; }
    public string Name { get; }
    public string Country { get; }

    public AirportAirline(string airline, string callsign, string name, string country) {
      Airline = airline;
      Callsign = callsign;
      Name = name;
      Country = country;
    }
  }
}
