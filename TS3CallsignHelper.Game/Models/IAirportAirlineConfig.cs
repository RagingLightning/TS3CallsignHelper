using System.Collections.Immutable;
using TS3CallsignHelper.Common.DTOs;

namespace TS3CallsignHelper.Game.Models;
public abstract class IAirportAirlineConfig {

  public virtual ImmutableDictionary<string, AirportAirline> Airlines => _airlines.ToImmutableDictionary();

  protected Dictionary<string, AirportAirline> _airlines = new();

  public virtual bool Contains(string airlineName) => _airlines.ContainsKey(airlineName);

  public virtual bool TryGet(string airlineName, out AirportAirline airline) {
    if (_airlines.TryGetValue(airlineName, out var val)) {
      airline = val;
      return true;
    }
    airline = new AirportAirline();
    return false;
  }

}
