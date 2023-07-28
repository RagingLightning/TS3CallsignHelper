using System.Text.RegularExpressions;
using TS3CallsignHelper.Game.Exceptions;

namespace TS3CallsignHelper.Game.Models.Raw;
public partial class AirportScheduleConfig {
  [GeneratedRegex("^(?<q0>\"?)(?<operator>[A-Z]{3})\\k<q0>,(?<q1>\"?)(?<airline>[A-Z]{3})\\k<q1>,(?<q2>\"?)(?<flightnumber>.+?)\\k<q2>,(?<q3>\"?)(?<airplanetype>.+?)\\k<q3>,(?<q4>\"?)(?<from>\\p{Lu}{4})\\k<q4>,(?<q5>\"?)(?<to>\\p{Lu}{4})\\k<q5>,(?<q6>\"?)(?<arrival>[0-9:]*?)\\k<q6>,(?<q7>\"?)(?<departure>[0-9:]*?)\\k<q7>,(?<q8>\"?)(?<approachaltitude>\\d*?)\\k<q7>,(?<q8>\"?)(?<special>.*?)\\k<q8>$")]
  private static partial Regex Parser();



  public IEnumerable<AirportScheduleEntry> Schedule => _schedule.Values;

  private Dictionary<string, AirportScheduleEntry> _schedule;

  public AirportScheduleConfig(string configPath) {
    _schedule = new Dictionary<string, AirportScheduleEntry>();

    var configFile = File.Open(configPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    using var reader = new StreamReader(configFile);
    reader.ReadLine(); // first line contains headers
    while (reader.ReadLine() is string line) {
      var groups = Parser().Match(line).Groups;
      if (groups.Count == 1) throw new ScheduleDefinitionFormatException(line);

      var callsign = groups["operator"].Value + groups["flightnumber"].Value;
      _schedule.Add(callsign, new AirportScheduleEntry(groups["operator"].Value,
                                             groups["airline"].Value,
                                             groups["flightnumber"].Value,
                                             groups["airplanetype"].Value,
                                             groups["from"].Value,
                                             groups["to"].Value,
                                             groups["arrival"].Value,
                                             groups["departure"].Value,
                                             groups["approacaltitude"].Value,
                                             groups["special"].Value));
    }
  }
  internal bool TryGet(string plane, out AirportScheduleEntry entry) => _schedule.TryGetValue(plane, out entry);
}

public class AirportScheduleEntry {
  public string Operator { get; }
  public string Airline { get; }
  public string FlightNumber { get; }
  public string AirplaneType { get; }
  public string From { get; }
  public string To { get; }
  public DateTime? Arrival { get; }
  public DateTime? Departure { get; }
  public int ApproachAltitude { get; }
  public string Special { get; }

  internal AirportScheduleEntry(string op, string airline, string flightNumber, string airplaneType, string from, string to, string arrival, string departure, string approachAltitude, string special) :
           this(op, airline, flightNumber, airplaneType, from, to, null, null, 0, special) {
    Arrival = DateTime.TryParse(arrival, out var res0) ? res0 : null;
    Departure = DateTime.TryParse(departure, out var res1) ? res1 : null;
    ApproachAltitude = int.TryParse(approachAltitude, out var res2) ? res2 : 0;
  }

  public AirportScheduleEntry(string op, string airline, string flightNumber, string airplaneType, string from, string to, DateTime? arrival, DateTime? departure, int approachAltitude, string special) {
    Operator = op;
    Airline = airline;
    FlightNumber = flightNumber;
    AirplaneType = airplaneType;
    From = from;
    To = to;
    Arrival = arrival;
    Departure = departure;
    ApproachAltitude = approachAltitude;
    Special = special;
  }
}
