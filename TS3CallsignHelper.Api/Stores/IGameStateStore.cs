using System;
using System.Collections.Immutable;
using TS3CallsignHelper.Api.Events;

namespace TS3CallsignHelper.Api.Stores;
public interface IGameStateStore {
  ImmutableDictionary<string, AirportAirline>? Airlines { get; }
  ImmutableDictionary<string, AirportAirplane>? Airplanes { get; }
  string CurrentAirplane { get; }
  GameInfo? CurrentGameInfo { get; }
  ImmutableDictionary<string, AirportFrequency>? DepartureFrequencies { get; }
  ImmutableDictionary<string, AirportGa>? GaPlanes { get; }
  ImmutableDictionary<string, AirportFrequency>? GroundFrequencies { get; }
  ImmutableDictionary<string, PlaneState> PlaneStates { get; }
  ImmutableList<PlayerPosition> PlayerPositions { get; }
  ImmutableDictionary<string, AirportScheduleEntry>? Schedule { get; }
  ImmutableDictionary<string, AirportFrequency>? TowerFrequencies { get; }

  event PlayerPositionChangedEvent? ActivePositionChanged;
  event AirplaneChangedEvent? CurrentAirplaneChanged;
  event Action? GameSessionEnded;
  event GameSessionStartedEventHandler? GameSessionStarted;
  event PlaneStateChangedEvent? PlaneStateChanged;

  void Dispose();
  void SetPlayerPosition(PlayerPosition position, bool active);
}