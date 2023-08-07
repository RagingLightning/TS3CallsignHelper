using System.Collections.Immutable;
using TS3CallsignHelper.API;

namespace TS3CallsignHelper.Game.Services;
public interface IAirportFrequencyService {
  public abstract ImmutableDictionary<string, AirportFrequency> Load(string installation, GameInfo info);
}
