using System.Collections.Immutable;
using TS3CallsignHelper.Common.DTOs;

namespace TS3CallsignHelper.Game.Models;
public abstract class IAirportAirplaneConfig {

  public virtual ImmutableDictionary<string, AirportAirplane> Airplanes => _airplanes.ToImmutableDictionary();

  protected readonly Dictionary<string, AirportAirplane> _airplanes = new();

  public virtual bool Contains(string airlineName) => _airplanes.ContainsKey(airlineName);

  public virtual bool TryGet(string airlineName, out AirportAirplane airplane) {
    if (_airplanes.TryGetValue(airlineName, out var ap)) {
      airplane = ap;
      return true;
    }
    airplane = new AirportAirplane();
    return false;
  }
}
