using System;
using System.Collections.Immutable;
using TS3CallsignHelper.API.Events;

namespace TS3CallsignHelper.API.Stores;
public interface IGameStateStore {
  ImmutableDictionary<string, AirportAirline>? Airlines { get; }
  ImmutableDictionary<string, AirportAirplane>? Airplanes { get; }
  string CurrentAirplane { get; set; }
  GameInfo? CurrentGameInfo { get; }
  ImmutableDictionary<string, AirportFrequency>? DepartureFrequencies { get; }
  ImmutableDictionary<string, AirportGa>? GaPlanes { get; }
  ImmutableDictionary<string, AirportFrequency>? GroundFrequencies { get; }
  ImmutableDictionary<string, PlaneStateInfo> PlaneStates { get; }
  ImmutableList<PlayerPosition> PlayerPositions { get; }
  ImmutableDictionary<string, AirportScheduleEntry>? Schedule { get; }
  ImmutableDictionary<string, AirportFrequency>? TowerFrequencies { get; }

  event PlayerPositionChangedEvent? ActivePositionChanged;
  event AirplaneChangedEvent? CurrentAirplaneChanged;
  event Action? GameSessionEnded;
  event GameSessionStartedEventHandler? GameSessionStarted;
  event PlaneStateChangedEvent? PlaneStateChanged;

  void Dispose();
  void StartGame(GameInfo gameInfo);
  void EndGame();
  void SetPlayerPosition(PlayerPosition position, bool active);

  /// <summary>
  /// Sets the state of an airplane
  /// </summary>
  /// <param name="callsign">the airplane's callsign</param>
  /// <param name="state">state to set</param>
  /// <exception cref="InvalidPlaneStateException"><paramref name="state"/> is not valid for <paramref name="callsign"/></exception>
  void SetPlaneState(string callsign, PlaneStateInfo state);
}