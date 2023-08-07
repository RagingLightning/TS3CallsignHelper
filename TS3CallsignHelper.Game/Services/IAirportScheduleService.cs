using System.Collections.Immutable;
using TS3CallsignHelper.API;

namespace TS3CallsignHelper.Game.Services;
public interface IAirportScheduleService {
  public abstract ImmutableDictionary<string, AirportScheduleEntry> Load(string installation, GameInfo info,
    ImmutableDictionary<string, AirportAirplane> airplanes, ImmutableDictionary<string, AirportAirline> airlines);
}
