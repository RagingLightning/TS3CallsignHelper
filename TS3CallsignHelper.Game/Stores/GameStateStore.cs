using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using TS3CallsignHelper.Game.DTOs;
using TS3CallsignHelper.Game.Enums;
using TS3CallsignHelper.Game.Exceptions;
using TS3CallsignHelper.Game.LogParsers;
using TS3CallsignHelper.Game.Models;
using TS3CallsignHelper.Game.Models.Raw;
using TS3CallsignHelper.Game.Services;

namespace TS3CallsignHelper.Game.Stores;
public class GameStateStore {
  private readonly IServiceProvider _serviceProvider;
  private readonly ILogger<GameStateStore> _logger;

  public event Action<string> CurrentAirplaneChanged;
  public event Action<string, PlaneState> PlaneStateChanged;
  public event Action<PlayerPosition, bool> ActivePositionsChanged;
  public event Action<GameInfo> GameSessionStarted;
  public event Action GameSessionEnded;

  public bool CurrentPlaneIsAirline {
    get {
      if (_airportAirlineConfig is null) {
        _logger.LogWarning("No airlines have been loaded");
        throw new InvalidOperationException();
      }
      return _airportAirlineConfig.Contains(CurrentAirplane[..3]);
    }
  }

  private AirportAirlineConfig? _airportAirlineConfig;
  public ImmutableDictionary<string, AirportAirline>? Airlines {
    get {
      if (_airportAirlineConfig == null) {
        _logger.LogWarning("No airlines have been loaded");
        return null;
      }
      return _airportAirlineConfig.Airlines;
    }
  }

  private AirportFrequencyConfig? _airportFrequencyConfig;
  public ImmutableList<AirportFrequency>? DepartureFrequencies {
    get {
      if (_airportFrequencyConfig == null) {
        _logger.LogWarning("No frequencies have been loaded");
        return null;
      }
      return _airportFrequencyConfig.DepartureFrequencies;
    }
  }
  public ImmutableList<AirportFrequency>? TowerFrequencies {
    get {
      if (_airportFrequencyConfig == null) {
        _logger.LogWarning("No frequencies have been loaded");
        return null;
      }
      return _airportFrequencyConfig.TowerFrequencies;
    }
  }
  public ImmutableList<AirportFrequency>? GroundFrequencies {
    get {
      if (_airportFrequencyConfig == null) {
        _logger.LogWarning("No frequencies have been loaded");
        return null;
      }
      return _airportFrequencyConfig.GroundFrequencies;
    }
  }

  private AirportGaConfig? _airportGaConfig;
  public ImmutableDictionary<string, AirportGa>? GaPlanes {
    get {
      if (_airportGaConfig == null) {
        _logger.LogWarning("No ga planes have been loaded");
        return null;
      }
      return _airportGaConfig.GaPlanes;
    }
  }

  private AirportScheduleConfig? _airportScheduleConfig;
  public ImmutableDictionary<string, AirportScheduleEntry>? Schedule {
    get {
      if (_airportScheduleConfig == null) {
        _logger.LogWarning("No schedule has been loaded");
        return null;
      }
      return _airportScheduleConfig.Schedule;
    }
  }

  private AirportAirplaneConfig? _airportAirplaneConfig;
  public ImmutableDictionary<string, AirportAirplane>? Airplanes {
    get {
      if (_airportAirplaneConfig == null) {
        _logger.LogWarning("No airplanes have been loaded");
        return null;
      }
      return _airportAirplaneConfig.Airplanes;
    }
  }

  private string _currentAirplane;
  public string CurrentAirplane {
    get => _currentAirplane;
    private set {
      _currentAirplane = value;
      _logger.LogDebug("Raising CurrentAirplaneChanged");
      CurrentAirplaneChanged?.Invoke(value);
    }
  }
  private readonly Dictionary<string, PlaneState> _planeStates;
  private readonly List<PlayerPosition> _activePositions;

  private IGameLogParser _logParser;
  private string _installationPath;

  public GameStateStore(IGameLogParser logParser, IServiceProvider serviceProvider) {
    _serviceProvider = serviceProvider;
    _logger = _serviceProvider.GetRequiredService<ILogger<GameStateStore>>();

    _logger.LogInformation("Initializing with log parser {$Parser}", logParser);
    _planeStates = new Dictionary<string, PlaneState>();
    _activePositions = new List<PlayerPosition>();

    _logParser = logParser;

    _logger.LogDebug("Registering log reader event handlers");
    _logParser.InstallDirDetermined += OnInstallDirDetermined;
    _logger.LogTrace("{Method} registered", nameof(OnInstallDirDetermined));
    _logParser.GameSessionSarted += OnGameSessionStarted;
    _logger.LogTrace("{Method} registered", nameof(OnGameSessionStarted));
    _logParser.GameSessionEnded += OnGameSessionEnded;
    _logger.LogTrace("{Method} registered", nameof(OnGameSessionEnded));
    _logParser.NewPlaneState += OnNewPlaneState;
    _logger.LogTrace("{Method} registered", nameof(OnNewPlaneState));
    _logParser.NewActivePlane += OnNewActivePlane;
    _logger.LogTrace("{Method} registered", nameof(OnNewActivePlane));
  }

  public void Dispose() {
    _logger.LogDebug("Unregistering log reader event handlers");
    _logParser.InstallDirDetermined -= OnInstallDirDetermined;
    _logger.LogTrace("{Method} unregistered", nameof(OnInstallDirDetermined));
    _logParser.GameSessionSarted -= OnGameSessionStarted;
    _logger.LogTrace("{Method} unregistered", nameof(OnGameSessionStarted));
    _logParser.GameSessionEnded -= OnGameSessionEnded;
    _logger.LogTrace("{Method} unregistered", nameof(OnGameSessionEnded));
    _logParser.NewPlaneState -= OnNewPlaneState;
    _logger.LogTrace("{Method} unregistered", nameof(OnNewPlaneState));
    _logParser.NewActivePlane -= OnNewActivePlane;
    _logger.LogTrace("{Method} registered", nameof(OnNewActivePlane));
  }

  private void OnNewActivePlane(string airplane) {
    _logger.LogDebug("Recieved NewActivePlane event for {Airplane}", airplane);
    CurrentAirplane = airplane;
  }

  /**
   * Gets the current state of the airplane
   * 
   * <param name="airplane">airplane</param>
   * <returns>state of <paramref name="airplane"/> or unknown if not yet assigned</returns>
   */
  public PlaneState GetPlaneState(string airplane) {
    return _planeStates.TryGetValue(airplane, out var state) ? state : PlaneState.UNKNOWN;
  }

  /**
   * Sets the state of the airplane
   * 
   * <param name="airplane">airplane</param>
   * <param name="state">state to set</param>
   * <exception cref="InvalidPlaneStateException"><paramref name="state"/> is not valid for <paramref name="airplane"/></exception>
   */
  public void SetPlaneState(string airplane, PlaneState state) {
    if (_logParser.State != ParserState.INIT_CATCHUP && !ValidatePlaneState(airplane, state))
      throw new InvalidPlaneStateException(airplane, state);
    _logger.LogDebug("State of {Airplane} changed to {State}", airplane, state);
    _planeStates[airplane] = state;
    PlaneStateChanged?.Invoke(airplane, state);
  }

  /**
   * <param name="position">player position</param>
   * <returns>whether <paramref name="position"/> is marked as active</returns>
   */
  public bool IsOnPosition(PlayerPosition position) {
    return _activePositions.Contains(position);
  }

  /**
   * Sets the status of a position
   * 
   * <param name="position">position to change</param>
   * <param name="active">whether the position is active or not</param>
   */
  public void SetOnPosition(PlayerPosition position, bool active) {
    if (active != _activePositions.Contains(position)) {
      if (active)
        _activePositions.Add(position);
      else
        _activePositions.Remove(position);
      _logger.LogInformation("Position {Position} changed to {Active}", position, active);
      ActivePositionsChanged?.Invoke(position, active);
    }
  }

  private void OnInstallDirDetermined(string installDir) {
    _logger.LogDebug("Recieved {Event} with {Installation}", nameof(OnInstallDirDetermined)[2..], installDir);
    if (!string.IsNullOrEmpty(_installationPath))
      _logger.LogWarning("Installation directory was changed during runtime! {OldInstallation} -> {NewInstallation}", _installationPath, installDir);
    else
      _logger.LogInformation("Tower Simulator installation directory: {Installation}", installDir);
    _installationPath = installDir;
  }

  private void OnGameSessionStarted(GameInfo info) {
    _logger.LogDebug("Recieved {Event} with {@GameInfo}", nameof(OnGameSessionStarted)[2..], info);
    CurrentAirplane = "";
    _planeStates.Clear();

    if (string.IsNullOrEmpty(_installationPath)) {
      _logger.LogError("Tower Simulator installation not located yet");
      throw new IncompleteGameInfoException("Installation");
    }
    if (string.IsNullOrEmpty(info.AirportICAO)) {
      _logger.LogError("Game info is missing ICAO code of airport");
      throw new IncompleteGameInfoException(nameof(info.AirportICAO));
    }
    if (string.IsNullOrEmpty(info.DatabaseFolder)) {
      _logger.LogError("Game info is missing selected database");
      throw new IncompleteGameInfoException(nameof(info.DatabaseFolder));
    }
    if (string.IsNullOrEmpty(info.AirplaneSetFolder)) {
      _logger.LogError("Game info is missing selected airplane set");
      throw new IncompleteGameInfoException(nameof(info.AirplaneSetFolder));
    }
    var databaseFolder = Path.Combine(_installationPath, "Airports", info.AirportICAO, "databases", info.DatabaseFolder);
    var airplaneSetFolder = Path.Combine(_installationPath, "Airplanes", info.AirplaneSetFolder);

    _logger.LogInformation("Starting new game session for {Airport} / {Database}", info.AirportICAO, info.DatabaseFolder);
    var initializationProgress = _serviceProvider.GetRequiredService<InitializationProgressService>();
    _airportAirlineConfig = new AirportAirlineConfig(Path.Combine(databaseFolder, "airlines.csv"), _serviceProvider, initializationProgress);
    _airportFrequencyConfig = new AirportFrequencyConfig(Path.Combine(databaseFolder, "freq.csv"), _serviceProvider, initializationProgress);
    _airportGaConfig = new AirportGaConfig(Path.Combine(databaseFolder, "ga.csv"), _serviceProvider, initializationProgress);
    _airportScheduleConfig = new AirportScheduleConfig(Path.Combine(databaseFolder, "schedule.csv"), _serviceProvider, initializationProgress);
    _airportAirplaneConfig = new AirportAirplaneConfig(airplaneSetFolder, _serviceProvider, initializationProgress);
    initializationProgress.Completed = true;

    _logger.LogDebug("Raising {Event}", nameof(GameSessionStarted));
    GameSessionStarted?.Invoke(info);
  }

  private void OnGameSessionEnded() {
    _logger.LogDebug("Received {Event}", nameof(OnGameSessionEnded)[2..]);
    CurrentAirplane = "";
    _planeStates.Clear();
    _airportAirlineConfig = null;
    _airportFrequencyConfig = null;
    _airportGaConfig = null;
    _airportScheduleConfig = null;
    _airportAirplaneConfig = null;

    _logger.LogDebug("Raising {Event}", nameof(GameSessionEnded));
    GameSessionEnded?.Invoke();
  }

  private void OnNewPlaneState(string airplane, PlaneState state) {
    _logger.LogDebug("Recieved {Event} for {Airplane}, new state: {State}", nameof(OnNewPlaneState)[2..],airplane, state);
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
    if (!_planeStates.ContainsKey(airplane) && (state & PlaneState.IS_INITIAL) == 0) {
      _logger.LogWarning("Airplane {Airplane} state {State} is not valid for initial contact", airplane, state);
      return false;
    }
    if (_activePositions.Contains(PlayerPosition.Ground) && (state & PlaneState.IS_GND) != 0) return true;
    if (_activePositions.Contains(PlayerPosition.Tower) && (state & PlaneState.IS_TWR) != 0) return true;
    _logger.LogWarning("Airplane {Airplane} state {State} is not valid with {@Positions} selected", airplane, state, _activePositions);
    return false;
  }
}
