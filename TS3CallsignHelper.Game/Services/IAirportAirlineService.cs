using System.Collections.Immutable;
using TS3CallsignHelper.Api;

namespace TS3CallsignHelper.Game.Services;
public interface IAirportAirlineService {
  public abstract ImmutableDictionary<string, AirportAirline> Load(string installation, string airport, string database);
}
