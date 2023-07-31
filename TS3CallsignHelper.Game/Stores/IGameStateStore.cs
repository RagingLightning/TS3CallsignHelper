using System.Collections.Immutable;
using TS3CallsignHelper.Common.DTOs;
using TS3CallsignHelper.Game.Enums;
using TS3CallsignHelper.Game.Models;

namespace TS3CallsignHelper.Game.Stores;
public class IGameStateStore {

  public event Action<string>? CurrentAirplaneChanged;
  public event Action<string, PlaneState>? PlaneStateChanged;
  public event Action<PlayerPosition, bool>? ActivePositionChanged;
  public event Action<GameInfo>? GameSessionStarted;
  public event Action? GameSessionEnded;

  protected virtual void RaiseCurrentAirplaneChanged(string callsign) => CurrentAirplaneChanged?.Invoke(callsign);
  protected virtual void RaisePlaneStateChanged(string callsign, PlaneState state) => PlaneStateChanged?.Invoke(callsign, state);
  protected virtual void RaiseActivePositionChanged(PlayerPosition position, bool active) => ActivePositionChanged?.Invoke(position, active);
  protected virtual void RaiseGameSessionStarted(GameInfo info) => GameSessionStarted?.Invoke(info);
  protected virtual void RaiseGameSessionEnded() => GameSessionEnded?.Invoke();

  public IAirportAirlineConfig? AirlineConfig { get; protected set; }
  public IAirportAirplaneConfig? AirplaneConfig { get; protected set; }
  public IAirportFrequencyConfig? FrequencyConfig { get; protected set; }
  public IAirportGaConfig? GaConfig { get; protected set; }
  public IScheduleConfig? ScheduleConfig { get; protected set; }

  public virtual ImmutableDictionary<string, PlaneState> PlaneStates => _planeStates.ToImmutableDictionary();

  protected readonly Dictionary<string, PlaneState> _planeStates;

  public virtual string CurrentAirplane {
    get => _currentAirplane ?? string.Empty;
    protected set {
      _currentAirplane = value;
      CurrentAirplaneChanged?.Invoke(value);
    }
  }

  private string? _currentAirplane;

  public virtual void Dispose() { }
}
