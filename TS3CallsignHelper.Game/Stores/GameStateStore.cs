using Microsoft.Extensions.Logging;
using TS3CallsignHelper.Game.Enums;
using TS3CallsignHelper.Game.Exceptions;
using TS3CallsignHelper.Game.LogParsers;
using TS3CallsignHelper.Game.Models;
using TS3CallsignHelper.Game.Models.Raw;

namespace TS3CallsignHelper.Game.Stores;
public class GameStateStore {
  private ILogger<GameStateStore> _logger;

  public event Action CurrentAirplaneChanged;
  public event Action<string, PlaneState> PlaneStateChanged;
  public event Action<PlayerPosition, bool> ActivePositionsChanged;
  public string CurrentAirplane {
    get => _currentAirplane; set {
      if (_currentAirplane == value)
        return;
      _currentAirplane = value;
      CurrentAirplaneChanged?.Invoke();
    }
  }

  public string CurrentAirline { get => _airportAirlineConfig.Contains(_currentAirplane[..3]) ? _currentAirplane[..3] : "GA"; }
  public string CurrentFlight { get => _airportAirlineConfig.Contains(_currentAirplane[..3]) ? _currentAirplane[3..] : _currentAirplane; }

  private AirportAirlineConfig? _airportAirlineConfig;
  private AirportFrequencyConfig? _airportFrequencyConfig;
  private AirportGaConfig? _airportGaConfig;
  private AirportScheduleConfig? _airportScheduleConfig;

  private string _currentAirplane;
  private readonly Dictionary<string, PlaneState> _planeStates;
  private readonly List<PlayerPosition> _activePositions;

  private IGameLogParser _logParser;
  private string _installationPath;

  public GameStateStore(ILogger<GameStateStore> logger, IGameLogParser logParser) {
    _logger = logger;
    _logger.LogInformation("Initializing with log parser {$parser}", logParser);
    _planeStates = new Dictionary<string, PlaneState>();
    _activePositions = new List<PlayerPosition>();

    _logParser = logParser;

    _logger.LogDebug("Registering log reader event handlers");
    _logParser.InstallDirDetermined += OnInstallDirDetermined;
    _logger.LogTrace("{$method} registered", OnInstallDirDetermined);
    _logParser.GameSessionSarted += OnGameSessionStarted;
    _logger.LogTrace("{$method} registered", OnGameSessionStarted);
    _logParser.NewPlaneState += OnNewPlaneState;
    _logger.LogTrace("{$method} registered", OnNewPlaneState);
  }

  public void Dispose() {
    _logger.LogInformation("Disposing");
    _logger.LogDebug("Unregistering log reader event handlers");
    _logParser.InstallDirDetermined -= OnInstallDirDetermined;
    _logger.LogTrace("{$method} unregistered", OnInstallDirDetermined);
    _logParser.GameSessionSarted -= OnGameSessionStarted;
    _logger.LogTrace("{$method} unregistered", OnGameSessionStarted);
    _logParser.NewPlaneState -= OnNewPlaneState;
    _logger.LogTrace("{$method} unregistered", OnNewPlaneState);
  }

  private void OnInstallDirDetermined(string installDir) {
    if (!string.IsNullOrEmpty(_installationPath))
      _logger.LogWarning("Installation directory was changed during runtime! {oldPath} -> {newPath}", _installationPath, installDir);
    else
      _logger.LogInformation("Tower Simulator installation directory: {newPath}", installDir);
    _installationPath = installDir;
  }

  private void OnGameSessionStarted(GameInfo info) {
    _logger.LogInformation("New Game session starting: {@gameInfo}", info);
    _currentAirplane = "";
    _planeStates.Clear();

    if (string.IsNullOrEmpty(_installationPath)) return;
    if (string.IsNullOrEmpty(info.AirportICAO)) throw new IncompleteGameInfoException(nameof(info.AirportICAO));
    if (string.IsNullOrEmpty(info.DatabaseFolder)) throw new IncompleteGameInfoException(nameof(info.AirportICAO));
    var databaseFolder = Path.Combine(_installationPath, "Airports", info.AirportICAO, "databases", info.DatabaseFolder);

    _airportAirlineConfig = new AirportAirlineConfig(Path.Combine(databaseFolder,"airlines.csv"));
    _airportFrequencyConfig = new AirportFrequencyConfig(Path.Combine(databaseFolder, "freq.csv"));
    _airportGaConfig = new AirportGaConfig(Path.Combine(databaseFolder, "ga.csv"));
    _airportScheduleConfig = new AirportScheduleConfig(Path.Combine(databaseFolder, "schedule.csv"));
  }

  private void OnNewPlaneState(string airplane, PlaneState state) {
    SetPlaneState(airplane, state);
  }

  public AirportGa GetCurrentGaPlane() {
    if (CurrentAirline != "GA") throw new InvalidPlaneTypeException();
    if (!_airportGaConfig.TryGet(_currentAirplane, out var plane)) throw new UnknownPlaneException();
    return plane;
  }

  public AirportAirline GetCurrentAirline() {
    if (CurrentAirline == "GA") throw new InvalidPlaneTypeException();
    if (!_airportAirlineConfig.TryGet(CurrentAirline, out var airline)) throw new UnknownPlaneException();
    return airline;
  }

  public AirportScheduleEntry GetCurrentScheduleEntry() {
    if (CurrentAirline == "GA") throw new InvalidPlaneTypeException();
    if (!_airportScheduleConfig.TryGet(CurrentAirline, out var entry)) throw new UnknownPlaneException();
    return entry;
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
   * <exception cref="InvalidPlaneStateException">If <paramref name="state"/> is not valid for <paramref name="airplane"/></exception>
   */
  public void SetPlaneState(string airplane, PlaneState state) {
    if (!ValidatePlaneState(airplane, state))
      throw new InvalidPlaneStateException(airplane, state);
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
      ActivePositionsChanged?.Invoke(position, active);
    }
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
    if (_activePositions.Contains(PlayerPosition.Ground) && (state & PlaneState.IS_GND) == 0) return false;
    if (_activePositions.Contains(PlayerPosition.Tower) && (state & PlaneState.IS_TWR) == 0) return false;
    if (!_planeStates.ContainsKey(airplane) && (state & PlaneState.IS_INITIAL) == 0) return false;
    return true;
  }
}
