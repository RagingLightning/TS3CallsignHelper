using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using TS3CallsignHelper.API;
using TS3CallsignHelper.API.Dependencies;
using TS3CallsignHelper.API.Exceptions;
using TS3CallsignHelper.API.Logging;
using TS3CallsignHelper.Game.Exceptions;

namespace TS3CallsignHelper.Game.Services;
public partial class AirportAirlineService : IAirportAirlineService {
  [GeneratedRegex("^(?<q0>\"?)(?<airline>[A-Z0-9]{3})\\k<q0>,(?<q1>\"?)(?<callsign>.+?)\\k<q1>,(?<q2>\"?)(?<name>.+?)\\k<q2>,(?<q3>\"?)(?<country>.+?)\\k<q3>$")]
  private static partial Regex Parser();
  private readonly ILogger<AirportAirlineService>? _logger;
  private readonly IInitializationProgressService _initializationProgressService;

  /// <summary>
  /// Requires <seealso cref="ILoggerService"/>, <seealso cref="IInitializationProgressService"/>
  /// </summary>
  /// <param name="dependencyStore"></param>
  /// <exception cref="MissingDependencyException"></exception>
  public AirportAirlineService(IDependencyStore dependencyStore) {
    _logger = dependencyStore.TryGet<ILoggerService>()?.GetLogger<AirportAirlineService>();
    _initializationProgressService = dependencyStore.TryGet<IInitializationProgressService>() ?? throw new MissingDependencyException(typeof(IInitializationProgressService));
  }

  public ImmutableDictionary<string, AirportAirline> Load(string installation, GameInfo info) {

    var airlines = new Dictionary<string, AirportAirline>();

    _initializationProgressService.StatusMessage = "State_Airlines";

    var airport = info.AirportICAO ?? throw new IncompleteGameInfoException(info, nameof(info.AirportICAO));
    var database = info.DatabaseFolder ?? throw new IncompleteGameInfoException(info, nameof(info.DatabaseFolder));

    var configFile = Path.Combine(installation, "Airports", airport, "databases", database, "airlines.csv");
    _logger?.LogDebug("Loading airlines from {Config}", configFile);
    var stream = File.Open(configFile, FileMode.Open, FileAccess.Read, FileShare.Read);
    using var reader = new StreamReader(stream);
    reader.ReadLine(); // first line contains headers
    while (reader.ReadLine() is string line) {
      _logger.LogTrace("Loading airline from {Line}", line);
      var groups = Parser().Match(line).Groups;
      if (groups.Count == 1) throw new AirlineDefinitionFormatException(line);
      if (!MakeAirline(groups, out var airline)) continue;
      airlines.Add(airline.Code, airline);
      _logger.LogDebug("Added Airline {@Airline}", airline);
      _initializationProgressService.AirlineProgess = ((float) stream.Position) / stream.Length;
    }
    _initializationProgressService.AirlineProgess = 1;

    _logger?.LogInformation("Loaded airplanes from {Config}", configFile);
    return airlines.ToImmutableDictionary();
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


