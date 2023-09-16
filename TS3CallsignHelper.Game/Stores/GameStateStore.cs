using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using TS3CallsignHelper.API;
using TS3CallsignHelper.API.Dependencies;
using TS3CallsignHelper.API.Events;
using TS3CallsignHelper.API.Exceptions;
using TS3CallsignHelper.API.Logging;
using TS3CallsignHelper.API.Services;
using TS3CallsignHelper.API.Stores;
using TS3CallsignHelper.Game.Exceptions;
using TS3CallsignHelper.Game.Services;
using TS3CallsignHelper.Wpf.Translation;

namespace TS3CallsignHelper.Game.Stores;
public class GameStateStore : IGameStateStore {
  private readonly ILogger<GameStateStore>? _logger;
  private readonly IGuiMessageService? _guiMessageService;
  private readonly IInitializationProgressService _initializationProgressService;

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
      if (value != string.Empty)
        CurrentAirplaneChanged?.Invoke(new AirplaneChangedEventArgs(value));
    }
  }

  private GameInfo? _gameInfo = null;
  public GameInfo? CurrentGameInfo {
    get => _gameInfo;
    set => _gameInfo = value;
  }
  private string _installDir = string.Empty;
  public string InstallDir {
    get => _installDir;
    set {
      if (!string.IsNullOrEmpty(_installDir)) {
        _logger?.LogError("Attempted to change install location {OldInstallation} -> {NewInstallation}", _installDir, value);
        return;
      }
      _logger?.LogInformation("Tower Simulator installation directory: {Installation}", value);
      _installDir = value;
    }
  }

  private readonly IAirportDataStore _airportDataStore;
  public ImmutableDictionary<string, AirportAirline>? Airlines => _airportDataStore.Airlines;
  public ImmutableDictionary<string, AirportAirplane>? Airplanes => _airportDataStore.Airplanes;
  public ImmutableDictionary<string, AirportGa>? GaPlanes => _airportDataStore.GaPlanes;
  public ImmutableDictionary<string, AirportScheduleEntry>? Schedule => _airportDataStore.Schedule;
  public ImmutableDictionary<string, AirportFrequency>? GroundFrequencies => _airportDataStore.GroundFrequencies;
  public ImmutableDictionary<string, AirportFrequency>? TowerFrequencies => _airportDataStore.TowerFrequencies;
  public ImmutableDictionary<string, AirportFrequency>? DepartureFrequencies => _airportDataStore.DepartureFrequencies;

  private readonly Dictionary<string, PlaneStateInfo> _planeStates = new();
  public ImmutableDictionary<string, PlaneStateInfo> PlaneStates => _planeStates.ToImmutableDictionary();

  private readonly List<PlayerPosition> _playerPositions = new();
  public ImmutableList<PlayerPosition> PlayerPositions => _playerPositions.ToImmutableList();

  /// <summary>
  /// Requires <seealso cref="IInitializationProgressService"/>, <seealso cref="IAirportDataStore"/>
  /// </summary>
  /// <param name="dependencyStore"></param>
  /// <exception cref="MissingDependencyException"></exception>
  internal GameStateStore(IDependencyStore dependencyStore) {
    _logger = dependencyStore.TryGet<ILoggerService>()?.GetLogger<GameStateStore>();
    _guiMessageService = dependencyStore.TryGet<IGuiMessageService>();
    _initializationProgressService = dependencyStore.TryGet<IInitializationProgressService>() ?? throw new MissingDependencyException(typeof(IInitializationProgressService));
    _airportDataStore = dependencyStore.TryGet<IAirportDataStore>() ?? throw new MissingDependencyException(typeof(IAirportDataStore));
  }

  public void Dispose() {
    _airportDataStore?.Unload();
  }

  public void StartGame(GameInfo info) {
    _logger?.LogDebug("Starting game session {@GameInfo}", info);
    _initializationProgressService.Details = $"{info.AirportICAO}/{info.DatabaseFolder} @ {info.StartHour}:00";
    _initializationProgressService.AirplaneProgress = 0;
    _initializationProgressService.AirlineProgess = 0;
    _initializationProgressService.FrequencyProgress = 0;
    _initializationProgressService.GaProgress = 0;
    _initializationProgressService.ScheduleProgress = 0;
    CurrentAirplane = "";
    CurrentGameInfo = null;
    _planeStates.Clear();

    if (string.IsNullOrEmpty(InstallDir)) {
      _guiMessageService?.ShowError(ExceptionMessages.GameStart_Installation);
      _logger?.LogError("Tower Simulator installation not located yet");
      return;
    }
    if (string.IsNullOrEmpty(info.AirportICAO)) {
      _guiMessageService?.ShowError(ExceptionMessages.GameStart_Icao);
      _logger?.LogError("Game info is missing ICAO code of airport");
      return;
    }
    if (string.IsNullOrEmpty(info.DatabaseFolder)) {
      _guiMessageService?.ShowError(ExceptionMessages.GameStart_Database);
      _logger?.LogError("Game info is missing selected database");
      return;
    }
    if (string.IsNullOrEmpty(info.AirplaneSetFolder)) {
      _guiMessageService?.ShowError(ExceptionMessages.GameStart_AirplaneSet);
      _logger?.LogError("Game info is missing selected airplane set");
      return;
    }

    _logger?.LogInformation("Starting new game session for {Airport} / {Database}", info.AirportICAO, info.DatabaseFolder);
    try {
      _airportDataStore.Load(InstallDir, info);

      CurrentGameInfo = info;
      _logger?.LogDebug("Raising {Event}", nameof(GameSessionStarted));
      GameSessionStarted?.Invoke(new GameSessionStartedEventArgs(info));
    }
    catch (AirlineDefinitionFormatException ex) {
      _guiMessageService?.ShowError(ExceptionMessages.GameStart_Airlines);
      _logger?.LogError(ex, "Failed to load airlines");
    }
    catch (AirplaneDefinitionFormatException ex) {
      _guiMessageService?.ShowError(ExceptionMessages.GameStart_Airplanes);
      _logger?.LogError(ex, "Failed to load airplanes");
    }
    catch (GaDefinitionFormatException ex) {
      _guiMessageService?.ShowError(ExceptionMessages.GameStart_Ga);
      _logger?.LogError(ex, "Failed to load ga schedule");
    }
    catch (ScheduleDefinitionFormatException ex) {
      _guiMessageService?.ShowError(ExceptionMessages.GameStart_Schedule);
      _logger?.LogError(ex, "Failed to load schedule");
    }
    catch (FrequencyDefinitionFormatException ex) {
      _guiMessageService?.ShowError(ExceptionMessages.GameStart_Frequencies);
      _logger?.LogError(ex, "Failed to load frequencies");
    }
    catch (Exception ex) {
      _guiMessageService?.ShowError(ExceptionMessages.GameStart_Unknown);
      _logger?.LogError(ex, "Failed to handle game start");
    } finally {
      _initializationProgressService.StatusMessage = "State_LogFile";
      _initializationProgressService.AirlineProgess = 1;
      _initializationProgressService.AirplaneProgress = 1;
      _initializationProgressService.GaProgress = 1;
      _initializationProgressService.ScheduleProgress = 1;
      _initializationProgressService.FrequencyProgress = 1;
    }
  }

  public void EndGame() {
    _logger?.LogDebug("Ending game session");
    _initializationProgressService.Details = "";
    CurrentGameInfo = null;
    CurrentAirplane = "";
    _planeStates.Clear();

    _airportDataStore.Unload();

    _logger?.LogDebug("Raising {Event}", nameof(GameSessionEnded));
    GameSessionEnded?.Invoke();
  }

  /// <inheritdoc/>
  public void SetPlaneState(string callsign, PlaneStateInfo state) {
    if (!ValidatePlaneState(callsign, state)) return;
    _logger?.LogDebug("State of {Airplane} changed to {State}", callsign, state);
    _planeStates[callsign] = state;
    PlaneStateChanged?.Invoke(new PlaneStateChangedEventArgs(callsign, state));
  }

  internal void ForcePlaneState(string callsign, PlaneStateInfo state) {
    _logger?.LogDebug("State of {Airplane} changed to {State}", callsign, state);
    _planeStates[callsign] = state;
    PlaneStateChanged?.Invoke(new PlaneStateChangedEventArgs(callsign, state));
  }

  /// <summary>
  /// Validates the rough validity of a plane state
  /// this takes into account the active positions and restrictions for initial contact
  /// </summary>
  /// <param name="airplane">airplane</param>
  /// <param name="stateInfo">state to validate</param>
  /// <returns><c>true</c>, if the assignment is valid, <c>false</c> otherwise</returns>
  private bool ValidatePlaneState(string airplane, PlaneStateInfo stateInfo) {
    if (stateInfo.State == PlaneState.UNKNOWN)
      return !_planeStates.ContainsKey(airplane) || _planeStates[airplane].State == PlaneState.UNKNOWN;
    foreach (var position in _playerPositions) {
      if (stateInfo.State.Is(position) && (_planeStates.ContainsKey(airplane) || stateInfo.State.IsInitial(position)))
        return true;
    }
    _logger?.LogWarning("Airplane {Airplane} state {@State} is not valid with {@Positions} selected", airplane, stateInfo, _playerPositions);
    return false;
  }

  public void SetPlayerPosition(PlayerPosition position, bool active) {
    if (active == _playerPositions.Contains(position)) return;
    if (active)
      _playerPositions.Add(position);
    else
      _playerPositions.Remove(position);
  }
}
