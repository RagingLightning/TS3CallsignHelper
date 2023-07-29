using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.RegularExpressions;
using TS3CallsignHelper.Game.Exceptions;
using TS3CallsignHelper.Game.Services;

namespace TS3CallsignHelper.Game.Models.Raw;
public partial class AirportScheduleConfig {
  [GeneratedRegex("^(?<q0>\"?)(?<operator>[A-Z]{3})\\k<q0>,(?<q1>\"?)(?<airline>[A-Z]{3})\\k<q1>,(?<q2>\"?)(?<flightnumber>.+?)\\k<q2>,(?<q3>\"?)(?<airplanetype>.+?)\\k<q3>,(?<q4>\"?)(?<from>\\p{Lu}{4})\\k<q4>,(?<q5>\"?)(?<to>\\p{Lu}{4})\\k<q5>,(?<q6>\"?)(?<arrival>[0-9:]*?)\\k<q6>,(?<q7>\"?)(?<departure>[0-9:]*?)\\k<q7>,(?<q8>\"?)(?<approachaltitude>\\d*?)\\k<q7>,(?<q8>\"?)(?<special>.*?)\\k<q8>$")]
  private static partial Regex Parser();
  private readonly ILogger<AirportScheduleConfig> _logger;

  public IEnumerable<AirportScheduleEntry> Schedule => _schedule.Values;

  private Dictionary<string, AirportScheduleEntry> _schedule;

  public AirportScheduleConfig(string configPath, IServiceProvider serviceProvider, InitializationProgressService initializationProgress) {
    _logger = serviceProvider.GetRequiredService<ILogger<AirportScheduleConfig>>();

    _schedule = new Dictionary<string, AirportScheduleEntry>();

    initializationProgress.StatusMessage = "Loading schedule...";
    _logger.LogDebug("Loading airport schedule from {Config}", configPath);
    var stream = File.Open(configPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    using var reader = new StreamReader(stream);
    reader.ReadLine(); // first line contains headers
    while (reader.ReadLine() is string line) {
      _logger.LogTrace("Parsing schedule information from {Line}", line);
      var groups = Parser().Match(line).Groups;
      if (groups.Count == 1) throw new ScheduleDefinitionFormatException(line);

      var callsign = groups["operator"].Value + groups["flightnumber"].Value;
      var schedule = new AirportScheduleEntry(groups["operator"].Value,
                                             groups["airline"].Value,
                                             groups["flightnumber"].Value,
                                             groups["airplanetype"].Value,
                                             groups["from"].Value,
                                             groups["to"].Value,
                                             groups["arrival"].Value,
                                             groups["departure"].Value,
                                             groups["approacaltitude"].Value,
                                             groups["special"].Value);
      _schedule.Add(callsign, schedule);
      _logger.LogDebug("Added schedule entry {@Schedule}", schedule);
      initializationProgress.ScheduleProgress = ((float) stream.Position) / stream.Length;
    }
    initializationProgress.ScheduleProgress = 1;
  }
  internal bool TryGet(string plane, out AirportScheduleEntry entry) => _schedule.TryGetValue(plane, out entry);
}

public class AirportScheduleEntry {
  /// <summary>
  /// Determines the airplane livery
  /// </summary>
  public string Operator { get; }

  /// <summary>
  /// Determines the airplane callsign
  /// </summary>
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
