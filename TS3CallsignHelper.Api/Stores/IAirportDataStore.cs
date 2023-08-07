using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TS3CallsignHelper.API;

namespace TS3CallsignHelper.API.Stores;
public abstract class IAirportDataStore {
  public ImmutableDictionary<string, AirportAirline>? Airlines => _airlines;
  public ImmutableDictionary<string, AirportAirplane>? Airplanes => _airplanes;
  public ImmutableDictionary<string, AirportFrequency>? DepartureFrequencies => _departureFrequencies;
  public ImmutableDictionary<string, AirportFrequency>? TowerFrequencies => _towerFrequencies;
  public ImmutableDictionary<string, AirportFrequency>? GroundFrequencies => _groundFrequencies;
  public ImmutableDictionary<string, AirportGa>? GaPlanes => _gaPlanes;
  public ImmutableDictionary<string, AirportScheduleEntry>? Schedule => _schedule;

  protected ImmutableDictionary<string, AirportAirline>? _airlines;
  protected ImmutableDictionary<string, AirportAirplane>? _airplanes;
  protected ImmutableDictionary<string, AirportFrequency>? _departureFrequencies;
  protected ImmutableDictionary<string, AirportFrequency>? _towerFrequencies;
  protected ImmutableDictionary<string, AirportFrequency>? _groundFrequencies;
  protected ImmutableDictionary<string, AirportGa>? _gaPlanes;
  protected ImmutableDictionary<string, AirportScheduleEntry>? _schedule;

  public abstract void Load(string installation, GameInfo info);

  public virtual void Unload() {
    _airlines = null;
    _airplanes = null;
    _departureFrequencies = null;
    _towerFrequencies = null;
    _groundFrequencies = null;
    _gaPlanes = null;
    _schedule = null;
  }
}
