using System.Collections.Immutable;
using TS3CallsignHelper.API;

namespace TS3CallsignHelper.Game.Services;
public interface IAirportAirplaneService {
  public abstract ImmutableDictionary<string, AirportAirplane> Load(string installation, GameInfo info);
}
