using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using TS3CallsignHelper.API;
using TS3CallsignHelper.API.Dependencies;
using TS3CallsignHelper.API.Exceptions;
using TS3CallsignHelper.API.Logging;
using TS3CallsignHelper.Game.Exceptions;

namespace TS3CallsignHelper.Game.Services;
public partial class AirportScheduleService : IAirportScheduleService {
  [GeneratedRegex("^(?<q0>\"?)(?<operator>[A-Z]{3}|Default)\\k<q0>,(?<q1>\"?)(?<airline>[A-Z]{3})\\k<q1>,(?<q2>\"?)(?<flightnumber>.+?)\\k<q2>,(?<q3>\"?)(?<airplanetype>.+?)\\k<q3>,(?<q4>\"?)(?<from>\\p{Lu}{4})\\k<q4>,(?<q5>\"?)(?<to>\\p{Lu}{4})\\k<q5>,(?<q6>\"?)(?<arrival>[0-9:]*?)\\k<q6>,(?<q7>\"?)(?<departure>[0-9:]*?)\\k<q7>,(?<q8>\"?)(?<approachaltitude>\\d*?)\\k<q7>,(?<q8>\"?)(?<special>.*?)\\k<q8>$")]
  private static partial Regex Parser();
  private readonly ILogger<AirportScheduleService>? _logger;
  private readonly IInitializationProgressService _initializationProgressService;

  /// <summary>
  /// Requires <seealso cref="IInitializationProgressService"/>
  /// </summary>
  /// <param name="dependencyStore"></param>
  public AirportScheduleService(IDependencyStore dependencyStore) {
    _logger = dependencyStore.TryGet<ILoggerService>()?.GetLogger<AirportScheduleService>();
    _initializationProgressService = dependencyStore.TryGet<IInitializationProgressService>() ?? throw new MissingDependencyException(typeof(IInitializationProgressService));
  }

  public ImmutableDictionary<string, AirportScheduleEntry> Load(string installation, GameInfo info,
    ImmutableDictionary<string, AirportAirplane> airplanes, ImmutableDictionary<string, AirportAirline> airlines) {

    var schedule = new Dictionary<string, AirportScheduleEntry>();

    _initializationProgressService.StatusMessage = "State_Schedule";

    var airport = info.AirportICAO ?? throw new IncompleteGameInfoException(info, nameof(info.AirportICAO));
    var database = info.DatabaseFolder ?? throw new IncompleteGameInfoException(info, nameof(info.DatabaseFolder));
    var startTime = info.StartHour ?? throw new IncompleteGameInfoException(info, nameof(info.StartHour));

    var configFile = Path.Combine(installation, "Airports", airport, "databases", database, "schedule.csv");
    _logger.LogDebug("Loading airport schedule from {Config}", configFile);
    var stream = File.Open(configFile, FileMode.Open, FileAccess.Read, FileShare.Read);
    using var reader = new StreamReader(stream);
    reader.ReadLine(); // first line contains headers
    while (reader.ReadLine() is string line) {
      _logger.LogTrace("Parsing schedule information from {Line}", line);
      var groups = Parser().Match(line).Groups;
      if (groups.Count == 1) throw new ScheduleDefinitionFormatException(line);

      if (!MakeScheduleEntry(groups, airlines, airplanes, out var entry) || entry.Operator is null) continue;
      if (entry.Arrival?.Hour < startTime-1) continue;
      if (entry.Departure?.Hour < startTime - 1) continue;

      try {
        schedule.Add(entry.Operator.Code + entry.FlightNumber, entry);
        _logger.LogDebug("Added schedule entry {@Schedule}", schedule);
      }
      catch (ArgumentException ex) {
        _logger.LogWarning(ex, "Failed to add schedule entry {Line}", line);
      }

      _initializationProgressService.ScheduleProgress = ((float) stream.Position) / stream.Length;
    }
    _initializationProgressService.ScheduleProgress = 1;

    return schedule.ToImmutableDictionary();
  }

  private bool MakeScheduleEntry(GroupCollection groups, ImmutableDictionary<string, AirportAirline> airlines,
    ImmutableDictionary<string, AirportAirplane> airplanes, out AirportScheduleEntry result) {
    result = new AirportScheduleEntry();

    var flightNumber = groups["flightnumber"].Value;
    if (!airlines.TryGetValue(groups["operator"].Value, out var op)) {
      _logger.LogWarning("{Callsign}: Operator {Operator} does not exist", groups["operator"].Value + flightNumber, groups["operator"].Value);
      if (groups["operator"].Value != "Default")
        return false;
    }
    if (!airlines.TryGetValue(groups["airline"].Value, out var airline)) {
      _logger.LogWarning("{Callsign}: Airline {Airline} does not exist", op.Code + flightNumber, groups["airline"].Value);
      return false;
    }
    if (!airplanes.TryGetValue(groups["airplanetype"].Value, out var airplaneType)) {
      _logger.LogWarning("{Callsign}: Airplane type {Type} does not exist", op.Code + flightNumber, groups["airplanetype"].Value);
      return false;
    }
    var origin = groups["from"].Value;
    var destination = groups["to"].Value;
    DateTime? arrival = null;
    if (groups["arrival"].Value != string.Empty) {
      if (!DateTime.TryParse(groups["arrival"].Value, out var arr)) {
        _logger.LogWarning("{Callsign}: Failed to parse arrival time {Time}", op.Code + flightNumber, groups["arrival"].Value);
        return false;
      }
      arrival = arr;
    }
    DateTime? departure = null;
    if (groups["departure"].Value != string.Empty) {
      if (!DateTime.TryParse(groups["departure"].Value, out var dep)) {
        _logger.LogWarning("{Callsign}: Failed to parse departure time {Time}", op.Code + flightNumber, groups["departure"].Value);
        return false;
      }
      departure = dep;
    }
    if (!int.TryParse(groups["approachaltitude"].Value, out var approachAltitude) && groups["approachaltitude"].Value != string.Empty) {
      _logger.LogWarning("{Callsign}: Failed to parse approach altitude {Altitude}", op.Code + flightNumber, groups["approachaltitude"].Value);
      return false;
    }
    var special = groups["special"].Value;

    result.Operator = op;
    result.Airline = airline;
    result.FlightNumber = flightNumber;
    result.AirplaneType = airplaneType;
    result.Origin = origin;
    result.Destination = destination;
    result.Arrival = arrival;
    result.Departure = departure;
    result.ApproachAltitude = approachAltitude;
    result.Special = special;
    return true;
  }
}
