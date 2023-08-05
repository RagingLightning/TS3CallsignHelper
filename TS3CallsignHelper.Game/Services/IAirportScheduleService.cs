using System.Collections.Immutable;
using TS3CallsignHelper.Api;

namespace TS3CallsignHelper.Game.Services;
public interface IAirportScheduleService {
  public abstract ImmutableDictionary<string, AirportScheduleEntry> Load(string installation, string airport, string database,
    ImmutableDictionary<string, AirportAirplane> airplanes, ImmutableDictionary<string, AirportAirline> airlines);
}
