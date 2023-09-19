using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using TS3CallsignHelper.API;
using TS3CallsignHelper.API.Dependencies;
using TS3CallsignHelper.API.Exceptions;
using TS3CallsignHelper.API.Logging;
using TS3CallsignHelper.Game.Exceptions;

namespace TS3CallsignHelper.Game.Services;
public partial class AirportAirplaneService : IAirportAirplaneService {
  [GeneratedRegex("^(?<q0>\"?)(?<property>.+?)\\k<q0>,(?<q1>\"?)(?<value>.+?)\\k<q1>,(?<information>.+)?$")]
  private static partial Regex Parser();
  private readonly ILogger<AirportAirplaneService>? _logger;
  private readonly IInitializationProgressService _initializationProgressService;

  /// <summary>
  /// Requires <seealso cref="IInitializationProgressService"/>
  /// </summary>
  /// <param name="dependencyStore"></param>
  public AirportAirplaneService(IDependencyStore dependencyStore) {
    _logger = dependencyStore.TryGet<ILoggerService>()?.GetLogger<AirportAirplaneService>();
    _initializationProgressService = dependencyStore.TryGet<IInitializationProgressService>() ?? throw new MissingDependencyException(typeof(IInitializationProgressService));
  }

  public ImmutableDictionary<string, AirportAirplane> Load(string installation, GameInfo info) {

    var airplanes = new Dictionary<string, AirportAirplane>();

    _initializationProgressService.StatusMessage = "State_Airplanes";

    var airplaneSet = info.AirplaneSetFolder ?? throw new IncompleteGameInfoException(info, nameof(info.AirplaneSetFolder));

    var configFolder = Path.Combine(installation, "Airplanes", airplaneSet);
    _logger.LogDebug("Loading airplane set from {Config}", configFolder);
    var airplaneCount = new DirectoryInfo(configFolder).EnumerateDirectories().Count();
    foreach (var di in new DirectoryInfo(configFolder).EnumerateDirectories()) {
      var airplaneConfig = Path.Combine(configFolder, di.Name, di.Name + ".csv");

      _logger.LogTrace("Loading airplane type {AirplaneType} from {Config}", di.Name, airplaneConfig);
      var configFile = File.Open(airplaneConfig, FileMode.Open, FileAccess.Read, FileShare.Read);
      using var reader = new StreamReader(configFile);
      reader.ReadLine(); // first line contains headers

      if (!MakeAirplane(di.Name, reader, out var airplane)) continue;

      airplanes.Add(airplane.Code, airplane);
      _logger.LogDebug("Added airplane type {@AirplaneType}", airplane);
      _initializationProgressService.AirplaneProgress += 1.0f / airplaneCount;
    }
    _initializationProgressService.AirplaneProgress = 1;

    _logger?.LogInformation("Loaded airplane set from {Config}", configFolder);
    return airplanes.ToImmutableDictionary();
  }

  private bool MakeAirplane(string code, StreamReader reader, out AirportAirplane result) {
    result = new AirportAirplane();
    while (reader.ReadLine() is string line) {
      var groups = Parser().Match(line).Groups;
      if (groups.Count == 1) throw new AirplaneDefinitionFormatException(line);
      try {
        result.SetProperty(groups["property"].Value.Trim(), groups["value"].Value.Trim());
      }
      catch (InvalidDataException ex) {
        _logger.LogWarning("{Type}: {Key} is not a valid airplane property", code, ex.Message);
        return false;
      }
      catch (Exception ex) {
        _logger.LogWarning(ex, "Failed to add {Key} to {Type}", code, groups["property"].Value);
        return false;
      }
    }
    return true;
  }
}