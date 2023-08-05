using System.Collections.Immutable;
using TS3CallsignHelper.Api;
using TS3CallsignHelper.Api.Dependencies;
using TS3CallsignHelper.Api.Exceptions;
using TS3CallsignHelper.Api.Stores;
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

  public override void Load(string installation, string airport, string database, string airplaneSet) {
    _airlines = _airlineService.Load(installation, airport, database);
    _airplanes = _airplaneService.Load(installation, airplaneSet);
    _gaPlanes = _gaService.Load(installation, airport, database, _airplanes);
    _schedule = _scheduleService.Load(installation, airport, database, _airplanes, _airlines);

    var departureFrequencies = new Dictionary<string, AirportFrequency>();
    var towerFrequencies = new Dictionary<string, AirportFrequency>();
    var groundFrequencies = new Dictionary<string, AirportFrequency>();
    foreach (var entry in _frequencyService.Load(installation, airport, database)) {
      switch (entry.Value.Type) {
        case AirportFrequencyType.DEPARTURE: departureFrequencies.Add(entry.Key, entry.Value); break;
        case AirportFrequencyType.TOWER: towerFrequencies.Add(entry.Key, entry.Value); break;
        case AirportFrequencyType.GROUND: groundFrequencies.Add(entry.Key, entry.Value); break;
      }
    }
    _departureFrequencies = departureFrequencies.ToImmutableDictionary();
    _towerFrequencies = towerFrequencies.ToImmutableDictionary();
    _groundFrequencies = groundFrequencies.ToImmutableDictionary();
  }
}
