using System.Collections.Immutable;
using TS3CallsignHelper.API;

namespace TS3CallsignHelper.Game.Services;
public interface IAirportAirlineService {
  public abstract ImmutableDictionary<string, AirportAirline> Load(string installation, GameInfo info);
}
