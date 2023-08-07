using System.Collections.Immutable;
using TS3CallsignHelper.API;

namespace TS3CallsignHelper.Game.Services;
public interface IAirportGaService {
  public abstract ImmutableDictionary<string, AirportGa> Load(string installation, GameInfo info,
    ImmutableDictionary<string, AirportAirplane> airplanes);
}
