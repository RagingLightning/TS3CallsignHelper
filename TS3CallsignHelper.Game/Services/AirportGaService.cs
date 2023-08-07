using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using TS3CallsignHelper.API;
using TS3CallsignHelper.API.Dependencies;
using TS3CallsignHelper.API.Exceptions;
using TS3CallsignHelper.API.Logging;
using TS3CallsignHelper.Game.Exceptions;

namespace TS3CallsignHelper.Game.Services;
public partial class AirportGaService : IAirportGaService {
  [GeneratedRegex("^(?<q0>\"?)(?<writename>.+?)\\k<q0>,(?<q1>\"?)(?<sayname>.+?)\\k<q1>,(?<q2>\"?)(?<airplanetype>.+?)\\k<q2>,(?<q3>\"?)(?<from>[A-Z]{4})\\k<q3>,(?<q4>\"?)(?<to>[A-Z]{4})\\k<q4>,(?<q5>\"?)(?<arrival>[0-9:]*?)\\k<q5>,(?<q6>\"?)(?<departure>[0-9:]*?)\\k<q6>,(?<q7>\"?)(?<approachaltitude>[0-9]*?)\\k<q7>,(?<q8>\"?)(?<stopandgo>\\d)\\k<q8>,(?<q9>\"?)(?<touchandgo>\\d)\\k<q9>,(?<q10>\"?)(?<lowapproach>\\d)\\k<q10>,(?<q11>\"?)(?<special>.*?)\\k<q11>$")]
  private static partial Regex Parser();
  private readonly ILogger<AirportGaService>? _logger;
  private readonly IInitializationProgressService _initializationProgressService;

  /// <summary>
  /// Requires <seealso cref="IInitializationProgressService"/>
  /// </summary>
  /// <param name="dependencyStore"></param>
  public AirportGaService(IDependencyStore dependencyStore) {
    _logger = dependencyStore.TryGet<ILoggerService>()?.GetLogger<AirportGaService>();
    _initializationProgressService = dependencyStore.TryGet<IInitializationProgressService>() ?? throw new MissingDependencyException(typeof(IInitializationProgressService));
  }

  public ImmutableDictionary<string, AirportGa> Load(string installation, GameInfo info,
    ImmutableDictionary<string, AirportAirplane> airplanes) {

    var gaPlanes = new Dictionary<string, AirportGa>();

    _initializationProgressService.StatusMessage = "Loading GA schedule...";

    var airport = info.AirportICAO ?? throw new IncompleteGameInfoException(info, nameof(info.AirportICAO));
    var database = info.DatabaseFolder ?? throw new IncompleteGameInfoException(info, nameof(info.DatabaseFolder));
    var startTime = info.StartHour ?? throw new IncompleteGameInfoException(info, nameof(info.StartHour));

    var configFile = Path.Combine(installation, "Airports", airport, "databases", database, "ga.csv");
    _logger?.LogDebug("Loading ga schedule from {Config}", configFile);
    var stream = File.Open(configFile, FileMode.Open, FileAccess.Read, FileShare.Read);
    using var reader = new StreamReader(stream);
    reader.ReadLine(); // first line contains headers
    while (reader.ReadLine() is string line) {
      _logger?.LogTrace("Loading ga flight from {Line}", line);
      var groups = Parser().Match(line).Groups;
      if (groups.Count == 1) throw new GaDefinitionFormatException(line);

      if (!MakeGaPlane(groups, airplanes, out var gaPlane)) continue;
      if (gaPlane.Departure?.Hour < startTime-1) continue;
      if (gaPlane.Arrival?.Hour < startTime-1) continue;

      try {
        gaPlanes.Add(gaPlane.Writename, gaPlane);
        _logger?.LogDebug("Added ga schedule entry {@GaSchedule}", gaPlane);
      }
      catch (ArgumentException ex) {
        _logger?.LogWarning(ex, "Failed to add ga schedule entry {Line}", line);
      }
      _initializationProgressService.GaProgress = ((float) stream.Position) / stream.Length;
    }
    _initializationProgressService.GaProgress = 1;

    return gaPlanes.ToImmutableDictionary();
  }

  private bool MakeGaPlane(GroupCollection groups, ImmutableDictionary<string, AirportAirplane> airplanes, out AirportGa result) {
    result = new AirportGa();

    var writename = groups["writename"].Value;
    var sayname = groups["sayname"].Value;
    if (!airplanes.TryGetValue(groups["airplanetype"].Value, out var airplaneType)) {
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