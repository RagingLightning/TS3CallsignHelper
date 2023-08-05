using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using TS3CallsignHelper.Api;
using TS3CallsignHelper.Api.Dependencies;
using TS3CallsignHelper.Api.Events;
using TS3CallsignHelper.Api.Exceptions;
using TS3CallsignHelper.Api.Logging;
using TS3CallsignHelper.Api.Stores;
using TS3CallsignHelper.Game.Enums;
using TS3CallsignHelper.Game.Exceptions;
using TS3CallsignHelper.Game.LogParsers;
using TS3CallsignHelper.Game.Services;

namespace TS3CallsignHelper.Game.Stores;
public class GameStateStore : IGameStateStore {
  private readonly ILogger<GameStateStore>? _logger;
  private readonly IInitializationProgressService _initializationProgressService;

  private readonly IGameLogParser _logParser;
  private string? _installationPath;

  public event PlayerPositionChangedEvent? ActivePositionChanged;
  public event AirplaneChangedEvent? CurrentAirplaneChanged;
  public event Action? GameSessionEnded;
  public event GameSessionStartedEventHandler? GameSessionStarted;
  public event PlaneStateChangedEvent? PlaneStateChanged;

  private string _currentAirplane = string.Empty;
  public string CurrentAirplane {
    get => _currentAirplane;
    set {
      _currentAirplane = value;
      CurrentAirplaneChanged?.Invoke(new AirplaneChangedEventArgs(value));
    }
  }

  private GameInfo? _gameInfo = null;
  public GameInfo? CurrentGameInfo {
    get => _gameInfo;
    set => _gameInfo = value;
  }

  private readonly IAirportDataStore _airportDataStore;
  public ImmutableDictionary<string, AirportAirline>? Airlines => _airportDataStore.Airlines;
  public ImmutableDictionary<string, AirportAirplane>? Airplanes => _airportDataStore.Airplanes;
  public ImmutableDictionary<string, AirportGa>? GaPlanes => _airportDataStore.GaPlanes;
  public ImmutableDictionary<string, AirportScheduleEntry>? Schedule => _airportDataStore.Schedule;
  public ImmutableDictionary<string, AirportFrequency>? GroundFrequencies => _airportDataStore.GroundFrequencies;
  public ImmutableDictionary<string, AirportFrequency>? TowerFrequencies => _airportDataStore.TowerFrequencies;
  public ImmutableDictionary<string, AirportFrequency>? DepartureFrequencies => _airportDataStore.DepartureFrequencies;

  private readonly Dictionary<string, PlaneState> _planeStates = new();
  public ImmutableDictionary<string, PlaneState> PlaneStates => _planeStates.ToImmutableDictionary();

  private readonly List<PlayerPosition> _playerPositions = new();
  public ImmutableList<PlayerPosition> PlayerPositions => _playerPositions.ToImmutableList();

  /// <summary>
  /// Requires <seealso cref="IInitializationProgressService"/>, <seealso cref="IGameLogParser"/>, <seealso cref="IAirportDataStore"/>
  /// </summary>
  /// <param name="dependencyStore"></param>
  /// <exception cref="MissingDependencyException"></exception>
  public GameStateStore(IDependencyStore dependencyStore) {
    _logger = dependencyStore.TryGet<ILoggerService>()?.GetLogger<GameStateStore>();
    _initializationProgressService = dependencyStore.TryGet<IInitializationProgressService>() ?? throw new MissingDependencyException(typeof(IInitializationProgressService));
    _logParser = dependencyStore.TryGet<IGameLogParser>() ?? throw new MissingDependencyException(typeof(IGameLogParser));
    _airportDataStore = dependencyStore.TryGet<IAirportDataStore>() ?? throw new MissingDependencyException(typeof(IAirportDataStore));

    _logger?.LogInformation("Initializing with log parser {$Parser}", _logParser);

    _logger?.LogDebug("Registering log reader event handlers");
    _logParser.InstallDirDetermined += OnInstallDirDetermined;
    _logger?.LogTrace("{Method} registered", nameof(OnInstallDirDetermined));
    _logParser.GameSessionSarted += OnGameSessionStarted;
    _logger?.LogTrace("{Method} registered", nameof(OnGameSessionStarted));
    _logParser.GameSessionEnded += OnGameSessionEnded;
    _logger?.LogTrace("{Method} registered", nameof(OnGameSessionEnded));
    _logParser.NewPlaneState += OnNewPlaneState;
    _logger?.LogTrace("{Method} registered", nameof(OnNewPlaneState));
    _logParser.NewActivePlane += OnNewActivePlane;
    _logger?.LogTrace("{Method} registered", nameof(OnNewActivePlane));
  }

  public void Dispose() {
    _airportDataStore?.Unload();

    _logger?.LogDebug("Unregistering log reader event handlers");
    _logParser.InstallDirDetermined -= OnInstallDirDetermined;
    _logger?.LogTrace("{Method} unregistered", nameof(OnInstallDirDetermined));
    _logParser.GameSessionSarted -= OnGameSessionStarted;
    _logger?.LogTrace("{Method} unregistered", nameof(OnGameSessionStarted));
    _logParser.GameSessionEnded -= OnGameSessionEnded;
    _logger?.LogTrace("{Method} unregistered", nameof(OnGameSessionEnded));
    _logParser.NewPlaneState -= OnNewPlaneState;
    _logger?.LogTrace("{Method} unregistered", nameof(OnNewPlaneState));
    _logParser.NewActivePlane -= OnNewActivePlane;
    _logger?.LogTrace("{Method} registered", nameof(OnNewActivePlane));
  }

  private void OnNewActivePlane(string airplane) {
    if (string.IsNullOrEmpty(airplane)) return;
    _logger?.LogDebug("Recieved NewActivePlane event for {Airplane}", airplane);
    CurrentAirplane = airplane;
  }

  /**
   * Sets the state of the airplane
   * 
   * <param name="airplane">airplane</param>
   * <param name="state">state to set</param>
   * <exception cref="InvalidPlaneStateException"><paramref name="state"/> is not valid for <paramref name="airplane"/></exception>
   */
  private void SetPlaneState(string airplane, PlaneState state) {
    if (_logParser.State != ParserState.INIT_CATCHUP && !ValidatePlaneState(airplane, state)) return;
    _logger?.LogDebug("State of {Airplane} changed to {State}", airplane, state);
    _planeStates[airplane] = state;
    PlaneStateChanged?.Invoke(new PlaneStateChangedEventArgs(airplane, state));
  }

  private void OnInstallDirDetermined(string installDir) {
    _logger?.LogDebug("Recieved {Event} with {Installation}", nameof(OnInstallDirDetermined)[2..], installDir);
    if (!string.IsNullOrEmpty(_installationPath))
      _logger?.LogWarning("Installation directory was changed during runtime! {OldInstallation} -> {NewInstallation}", _installationPath, installDir);
    else
      _logger?.LogInformation("Tower Simulator installation directory: {Installation}", installDir);
    _installationPath = installDir;
  }

  private void OnGameSessionStarted(GameInfo info) {
    _logger?.LogDebug("Recieved {Event} with {@GameInfo}", nameof(OnGameSessionStarted)[2..], info);
    CurrentAirplane = "";
    _planeStates.Clear();

    if (string.IsNullOrEmpty(_installationPath)) {
      _logger?.LogError("Tower Simulator installation not located yet");
      throw new IncompleteGameInfoException(info, "Installation");
    }
    if (string.IsNullOrEmpty(info.AirportICAO)) {
      _logger?.LogError("Game info is missing ICAO code of airport");
      throw new IncompleteGameInfoException(info, nameof(info.AirportICAO));
    }
    if (string.IsNullOrEmpty(info.DatabaseFolder)) {
      _logger?.LogError("Game info is missing selected database");
      throw new IncompleteGameInfoException(info, nameof(info.DatabaseFolder));
    }
    if (string.IsNullOrEmpty(info.AirplaneSetFolder)) {
      _logger?.LogError("Game info is missing selected airplane set");
      throw new IncompleteGameInfoException(info, nameof(info.AirplaneSetFolder));
    }

    _logger?.LogInformation("Starting new game session for {Airport} / {Database}", info.AirportICAO, info.DatabaseFolder);
    _airportDataStore.Load(_installationPath, info.AirportICAO, info.DatabaseFolder, info.AirplaneSetFolder);

    _initializationProgressService.Completed = true;

    CurrentGameInfo = info;
    _logger?.LogDebug("Raising {Event}", nameof(GameSessionStarted));
    GameSessionStarted?.Invoke(new GameSessionStartedEventArgs(info));
  }

  private void OnGameSessionEnded() {
    _logger?.LogDebug("Received {Event}", nameof(OnGameSessionEnded)[2..]);
    CurrentGameInfo = null;
    CurrentAirplane = "";
    _planeStates.Clear();

    _airportDataStore.Unload();

    _logger?.LogDebug("Raising {Event}", nameof(GameSessionEnded));
    GameSessionEnded?.Invoke();
  }

  private void OnNewPlaneState(string airplane, PlaneState state) {
    _logger?.LogDebug("Recieved {Event} for {Airplane}, new state: {State}", nameof(OnNewPlaneState)[2..], airplane, state);
    SetPlaneState(airplane, state);
  }

  /**
   * Validates the rough validity of a plane state
   * this takes into account the active positions and restrictions for initial contact
   * 
   * <param name="airplane">airplane</param>
   * <param name="state">state to validate</param>
   * 
   * <returns><c>true</c>, if the assignment is valid, <c>false</c> otherwise</returns>
   */
  private bool ValidatePlaneState(string airplane, PlaneState state) {
    if (_playerPositions.Contains(PlayerPosition.Ground) && (state & PlaneState.IS_GND) != 0)
      if (_planeStates.ContainsKey(airplane) || (state & PlaneState.IS_GND_INIT) != 0)
        return true;
    if (_playerPositions.Contains(PlayerPosition.Tower) && (state & PlaneState.IS_TWR) != 0)
      if (_planeStates.ContainsKey(airplane) || (state & PlaneState.IS_TWR_INIT) != 0)
        return true;
    if (_playerPositions.Contains(PlayerPosition.Departure) && (state & PlaneState.IS_DEP) != 0)
      if (_planeStates.ContainsKey(airplane) || (state & PlaneState.IS_DEP_INIT) != 0)
        return true;
    _logger?.LogWarning("Airplane {Airplane} state {State} is not valid with {@Positions} selected", airplane, state, _playerPositions);
    return false;
  }

  public void SetPlayerPosition(PlayerPosition position, bool active) {
    throw new NotImplementedException();
  }
}
