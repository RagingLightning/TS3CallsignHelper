using System.Collections.Immutable;
using TS3CallsignHelper.API;
using TS3CallsignHelper.API.Dependencies;
using TS3CallsignHelper.API.Exceptions;
using TS3CallsignHelper.API.Stores;
using TS3CallsignHelper.Game.Services;

namespace TS3CallsignHelper.Game.Stores;
public class AirportDataStore : IAirportDataStore {
  private readonly IAirportAirlineService _airlineService;
  private readonly IAirportAirplaneService _airplaneService;
  private readonly IAirportFrequencyService _frequencyService;
  private readonly IAirportGaService _gaService;
  private readonly IAirportScheduleService _scheduleService;

  /// <summary>
  /// Requires <seealso cref="IAirportAirlineService"/>, <seealso cref="IAirportAirplaneService"/>, <seealso cref="IAirportFrequencyService"/>, <seealso cref="IAirportGaService"/>, <seealso cref="IAirportScheduleService"/>, 
  /// </summary>
  /// <param name="dependencyStore"></param>
  public AirportDataStore(IDependencyStore dependencyStore) {
    _airlineService = dependencyStore.TryGet<IAirportAirlineService>() ?? throw new MissingDependencyException(typeof(IAirportAirlineService));
    _airplaneService = dependencyStore.TryGet<IAirportAirplaneService>() ?? throw new MissingDependencyException(typeof(IAirportAirplaneService));
    _frequencyService = dependencyStore.TryGet<IAirportFrequencyService>() ?? throw new MissingDependencyException(typeof(IAirportFrequencyService));
    _gaService = dependencyStore.TryGet<IAirportGaService>() ?? throw new MissingDependencyException(typeof(IAirportGaService));
    _scheduleService = dependencyStore.TryGet<IAirportScheduleService>() ?? throw new MissingDependencyException(typeof(IAirportScheduleService));
  }

  public override void Load(string installation, GameInfo info) {
    try {
      _airlines = _airlineService.Load(installation, info);
      _airplanes = _airplaneService.Load(installation, info);
      _gaPlanes = _gaService.Load(installation, info, _airplanes);
      _schedule = _scheduleService.Load(installation, info, _airplanes, _airlines);

      var departureFrequencies = new Dictionary<string, AirportFrequency>();
      var towerFrequencies = new Dictionary<string, AirportFrequency>();
      var groundFrequencies = new Dictionary<string, AirportFrequency>();
      foreach (var entry in _frequencyService.Load(installation, info)) {
        switch (entry.Value.Position) {
          case PlayerPosition.Unknown: departureFrequencies.Add(entry.Key, entry.Value); break;
          case PlayerPosition.Departure: departureFrequencies.Add(entry.Key, entry.Value); break;
          case PlayerPosition.Tower: towerFrequencies.Add(entry.Key, entry.Value); break;
          case PlayerPosition.Ground: groundFrequencies.Add(entry.Key, entry.Value); break;
        }
      }
      _departureFrequencies = departureFrequencies.ToImmutableDictionary();
      _towerFrequencies = towerFrequencies.ToImmutableDictionary();
      _groundFrequencies = groundFrequencies.ToImmutableDictionary();
    } catch(FormatException ex) {
      _airlines = null;
      _airplanes = null;
      _gaPlanes = null;
      _schedule = null;
      _departureFrequencies = null;
      _towerFrequencies = null;
      _groundFrequencies = null;
      throw;
    }
  }
}
