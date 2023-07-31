using System.Collections.Immutable;
using TS3CallsignHelper.Common.DTOs;

namespace TS3CallsignHelper.Game.Models;
public class IAirportGaConfig {

  public virtual ImmutableDictionary<string, AirportGa> GaPlanes => _gaPlanes.ToImmutableDictionary();

  protected readonly Dictionary<string, AirportGa> _gaPlanes = new();

  public bool Contains(string callsign) => _gaPlanes.ContainsKey(callsign);

  public bool TryGet(string callsign, out AirportGa gaPlane) {
    if (_gaPlanes.TryGetValue(callsign, out var value)) {
      gaPlane = value;
      return true;
    }
    gaPlane = new AirportGa();
    return false;
  }
}
