using System.Collections.Immutable;
using TS3CallsignHelper.Api;

namespace TS3CallsignHelper.Game.Services;
public interface IAirportFrequencyService {
  public abstract ImmutableDictionary<string, AirportFrequency> Load(string installation, string airport, string database);
}
