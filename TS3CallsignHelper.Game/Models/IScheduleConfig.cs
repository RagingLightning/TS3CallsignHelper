using System.Collections.Immutable;
using TS3CallsignHelper.Common.DTOs;

namespace TS3CallsignHelper.Game.Models;
public abstract class IScheduleConfig {

  public ImmutableDictionary<string, AirportScheduleEntry> Schedule => _schedule.ToImmutableDictionary();

  protected readonly Dictionary<string, AirportScheduleEntry> _schedule = new();

  public bool Contains(string callsign) => _schedule.ContainsKey(callsign);

  public bool TryGet(string callsign, out AirportScheduleEntry scheduleEntry) {
    if (_schedule.TryGetValue(callsign, out var value)) {
      scheduleEntry = value;
      return true;
    }
    scheduleEntry = new AirportScheduleEntry();
    return false;
  }
}
