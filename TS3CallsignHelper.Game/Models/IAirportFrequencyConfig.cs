using System.Collections.Immutable;
using TS3CallsignHelper.Common.DTOs;

namespace TS3CallsignHelper.Game.Models;
public abstract class IAirportFrequencyConfig {

  public virtual ImmutableDictionary<string, AirportFrequency> DepartureFrequencies => _departureFrequencies.ToImmutableDictionary();
  public virtual ImmutableDictionary<string, AirportFrequency> TowerFrequencies => _towerFrequencies.ToImmutableDictionary();
  public virtual ImmutableDictionary<string, AirportFrequency> GroundFrequencies => _groundFrequencies.ToImmutableDictionary();

  protected readonly Dictionary<string, AirportFrequency> _departureFrequencies = new();
  protected readonly Dictionary<string, AirportFrequency> _towerFrequencies = new();
  protected readonly Dictionary<string, AirportFrequency> _groundFrequencies = new();

  public AirportFrequencyType? GetFrequencyType(string frequencyValue) {
    if (GroundFrequencies.ContainsKey(frequencyValue)) return AirportFrequencyType.GROUND;
    if (TowerFrequencies.ContainsKey(frequencyValue)) return AirportFrequencyType.TOWER;
    if (DepartureFrequencies.ContainsKey(frequencyValue)) return AirportFrequencyType.DEPARTURE;
    return null;
  }

  public bool TryGet(string frequencyValue, out AirportFrequency? frequency) {
    switch (GetFrequencyType(frequencyValue)) {
      case AirportFrequencyType.GROUND:
        return _groundFrequencies.TryGetValue(frequencyValue, out frequency);
      case AirportFrequencyType.TOWER:
        return _towerFrequencies.TryGetValue(frequencyValue, out frequency);
      case AirportFrequencyType.DEPARTURE:
        return _departureFrequencies.TryGetValue(frequencyValue, out frequency);
    }
    frequency = null;
    return false;
  }
}
