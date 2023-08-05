using Microsoft.Extensions.Logging;
using TS3CallsignHelper.Api;
using TS3CallsignHelper.Api.Dependencies;
using TS3CallsignHelper.Api.Events;
using TS3CallsignHelper.Api.Exceptions;
using TS3CallsignHelper.Api.Logging;
using TS3CallsignHelper.Api.Stores;
using TS3CallsignHelper.Game.Enums;
using TS3CallsignHelper.Game.Services;
using TS3CallsignHelper.Game.Stores;

namespace TS3CallsignHelper.Game.LogParsers;
public class GameLogParserPlacesTwo : IGameLogParser {
  private readonly ILogger<GameLogParserPlacesTwo>? _logger;
  private readonly IInitializationProgressService _initializationProgressService;
  private readonly IAirportDataStore _airportDataStore;

  public event LogLineReadEventHandler? LogLineRead;
  public event Action<float, string>? InitializationProgress;
  public event Action<string>? InstallDirDetermined;
  public event Action<GameInfo>? GameSessionSarted;
  public event Action? GameSessionEnded;
  public event Action<Metar>? MetarUpdated;
  public event Action<string>? NewActivePlane;
  public event Action<string, PlaneState>? NewPlaneState;

  private ParserState _state;
  public ParserState State => _state;

  private readonly GameLogReader _reader;

  private ParserMode _mode;
  private IEntryParser? _entryParser;

  private string? _acapelaPlane;

  //only used during ParserState.INIT_CATCHUP
  private GameInfo? _currentGame;
  private Metar? _currentMetar;
  private readonly Dictionary<string, PlaneState> _planeStates;

  /// <summary>
  /// Requires <seealso cref="IInitializationProgressService"/>, <seealso cref="IAirportDataStore"/>
  /// </summary>
  /// <param name="dependencyStore"></param>
  public GameLogParserPlacesTwo(IDependencyStore dependencyStore) {
    _logger = dependencyStore.TryGet<ILoggerService>()?.GetLogger<GameLogParserPlacesTwo>();
    _initializationProgressService = dependencyStore.TryGet<IInitializationProgressService>() ?? throw new MissingDependencyException(typeof(IInitializationProgressService));
    _airportDataStore = dependencyStore.TryGet<IAirportDataStore>() ?? throw new MissingDependencyException(typeof(IAirportDataStore));

    _state = ParserState.NOT_INITIALIZED;
    _mode = ParserMode.NORMAL;
    _reader = new GameLogReader(Parse, _initializationProgressService);
    _reader.EndOfLog += PostInit;

    _currentGame = null;
    _currentMetar = null;
    _planeStates = new Dictionary<string, PlaneState>();
  }

  public void Init(string logFolder) {
    _logger?.LogDebug("Initializing log parser for {logFile}", Path.Combine(logFolder, "Player.log"));
    _state = ParserState.INIT_START;
    _reader.Init(logFolder);
  }

  public void PostInit() {
    if (_state != ParserState.INIT_CATCHUP) return;
    _logger?.LogDebug("Finished initial catchup");
    _reader.EndOfLog -= PostInit;
    _initializationProgressService.LogFileProgress = 1;


    if (_currentGame == null) {
      _initializationProgressService.Completed = true;
      _state = ParserState.RUNNING;
      return;
    }

    _logger?.LogTrace("Raising GameSessionSarted for determined game session {@session}", _currentGame);
    GameSessionSarted?.Invoke(_currentGame);
    if (_currentMetar != null) {
      _logger?.LogTrace("Raising MetarUpdated for determined metar {@metar}", _currentMetar);
      MetarUpdated?.Invoke(_currentMetar);
    }
    foreach (var callsign in _planeStates.Keys) {
      _logger?.LogTrace("Raising NewPlaneState for {airplane} state {state}", callsign, _planeStates[callsign]);
      NewPlaneState?.Invoke(callsign, _planeStates[callsign]);
    }
    _state = ParserState.RUNNING;
  }

  public void Start() {
    _reader.Start();
  }

  public void Stop() {
    //throw new NotImplementedException();
  }

  private void Parse(string logLine) {
    _logger?.LogTrace("Start parsing of {line}", logLine);
    if (_mode == ParserMode.GAME_START || logLine.StartsWith("GAME START"))
      ParseGameStart(logLine);
    else if (logLine.StartsWith("UnloadTime: "))
      ParseGameEnd();
    else if (logLine.StartsWith("Mono config path = ") && _state == ParserState.INIT_START)
      ParseInstallationDir(logLine);
    else if (logLine.StartsWith("METAR DOWNLOADED: "))
      ParseMetar(logLine);
    //else if (logLine.StartsWith("COMMAND: "))
    //  ParseCommand(logLine);
    else if (logLine.StartsWith("SET PlANE: "))
      ParseSetPlane(logLine);
    else if (logLine.StartsWith("Gen TTS hash"))
      ParseAcapelaHash(logLine);
    else if (logLine.StartsWith("ADD TTS to Acapela: "))
      ParseAcapelaTTS(logLine);
    else
      LogLineRead?.Invoke(new LogLineReadEventArgs(logLine));
  }

  private void ParseInstallationDir(string logLine) {
    string installDir = logLine[20..].Replace("/MonoBleedingEdge/etc'", string.Empty);
    _logger?.LogDebug("Raising InstallDirDetermined for {installDir}", installDir);
    InstallDirDetermined?.Invoke(installDir);
    _state = ParserState.INIT_CATCHUP;
  }

  private void ParseGameStart(string logLine) {
    if (logLine.StartsWith("GAME START")) {
      _logger?.LogDebug("Start parsing game info for new session");
      _mode = ParserMode.GAME_START;
      _entryParser = new GameStartEntryParser();
      return;
    }
    if (_entryParser?.Parse(logLine) is GameInfo info) {
      _logger?.LogDebug("Finished parsing new game info: {@info}", info);
      _mode = ParserMode.NORMAL;
      _currentGame = info;
      if (_state == ParserState.RUNNING) {
        _logger?.LogTrace("Raising GameSessionSarted");
        GameSessionSarted?.Invoke(info);
      }
      else
        _initializationProgressService.Details = $"Detected session {_currentGame.AirportICAO} / {_currentGame.DatabaseFolder}";
    }
  }

  private void ParseGameEnd() {
    _currentGame = null;
    _currentMetar = null;
    _planeStates.Clear();
    if (_state == ParserState.RUNNING) {
      _logger?.LogTrace("Raising GameSessionEnded");
      GameSessionEnded?.Invoke();
    }
    else
      _initializationProgressService.Details = string.Empty;
  }

  private void ParseMetar(string logLine) {
    var result = new MetarEntryParser().Parse(logLine);
    if (result is Metar metar) {
      _logger?.LogDebug("Finished parsing metar: {@metar}", metar);
      _currentMetar = metar;
      if (_state == ParserState.RUNNING) {
        _logger?.LogTrace("Raising MetarUpdated");
        MetarUpdated?.Invoke(metar);
      }
    }
    else
      _logger?.LogWarning("Failed to parse metar: {metar}", result);
  }

  private void ParseCommand(string logLine) {
    string plane = logLine[9..].Split(" ")[0];
    if (_state == ParserState.RUNNING) {
      _logger?.LogDebug("Raising NewActivePlane for {callsign}", plane);
      NewActivePlane?.Invoke(plane);
    }
  }

  private void ParseSetPlane(string logLine) {
    string plane = logLine.Split(" ")[^1];
    if (_state == ParserState.RUNNING) {
      _logger?.LogDebug("Raising NewActivePlane for {callsign}", plane);
      NewActivePlane?.Invoke(plane);
    }
  }

  private void ParseAcapelaHash(string logLine) {
    var plane = logLine.Split(" ")[^1];
    if (!string.IsNullOrEmpty(_acapelaPlane))
      _logger?.LogWarning("New Acapela hash for {newPlane} before {oldPlane} was evaluated", plane, _acapelaPlane);
    else
      _logger?.LogDebug("New Acapela hash for {newPlane}", plane);
    _acapelaPlane = plane;
  }

  private void ParseAcapelaTTS(string logLine) {
    if (string.IsNullOrEmpty(_acapelaPlane)) {
      _logger?.LogWarning("Acapela readback without hash beforehand");
      return;
    }

    PlaneState planeState = PlaneState.UNKNOWN;
    logLine = logLine.ToUpper();

    if (logLine.Contains("APPROVED")) {
      if (logLine.Contains("PUSHBACK"))
        planeState = PlaneState.OUT_PUSHBACK_PROGRESS;
      else
        planeState = PlaneState.OUT_STARTUP_PROGRESS;
    }
    if (logLine.Contains("ON FINAL"))
      planeState = PlaneState.IN_RWY_APPROACH;
    else if (logLine.Contains("RUNWAY")) {
      if (logLine.Contains("CLEARED")) {
        if (logLine.Contains("TAKEOFF"))
          planeState = PlaneState.OUT_RWY_TAKEOFF; // RUNWAY 00 CLEARED for TAKEOFF // RUNWAY 00 CLEARED for immediate TAKEOFF
        else if (logLine.Contains("LAND") && logLine.Contains("HOLD SHORT"))
          planeState = PlaneState.IN_RWY_CLR_LAHS;
        else if (logLine.Contains("LAND"))
          planeState = PlaneState.IN_RWY_CLR_LAND;
        else if (logLine.Contains("LOW APPROACH"))
          planeState = PlaneState.IN_RWY_CLR_LAPP;
        else if (logLine.Contains("TOUCH AND GO"))
          planeState = PlaneState.IN_RWY_CLR_TAGO;
        else if (logLine.Contains("STOP AND GO"))
          planeState = PlaneState.IN_RWY_CLR_SAGO;
      }
      else if (logLine.Contains("LINE UP")) {
        if (logLine.Contains("BEHIND"))
          planeState = PlaneState.OUT_RWY_LINE_UP_BEHIND; // RUNWAY 00 LINE UP and wait BEHIND next landing airplane
        else
          planeState = PlaneState.OUT_RWY_LINE_UP; // RUNWAY 00 LINE UP and wait
      }
    }
    else if (logLine.Contains("REQUESTING")) {
      if (logLine.Contains("PUSH"))
        planeState = PlaneState.OUT_PUSHBACK_REQUEST;
    }
    else if (logLine.Contains("READY")) {
      if (logLine.Contains("TAXI"))
        planeState = PlaneState.OUT_TAXI_REQUEST;
      else if (logLine.Contains("START"))
        planeState = PlaneState.OUT_STARTUP_REQUEST;
    }
    else if (logLine.Contains("GOING AROUND"))
      planeState = PlaneState.IN_RWY_GO_AROUND;

    if (planeState == PlaneState.UNKNOWN && _airportDataStore.DepartureFrequencies != null)
      foreach (var freq in _airportDataStore.DepartureFrequencies) {
        if (logLine.Contains(freq.Value.Readback.ToUpper())) {
          planeState = PlaneState.IS_RADIO_AWAY | PlaneState.IS_DEP;
          break;
        }
      }

    if (planeState == PlaneState.UNKNOWN && _airportDataStore.TowerFrequencies != null)
      foreach (var freq in _airportDataStore.TowerFrequencies) {
        if (logLine.Contains(freq.Value.Readback.ToUpper())) {
          planeState = PlaneState.IS_RADIO_AWAY | PlaneState.IS_TWR;
          break;
        }
      }

    if (planeState == PlaneState.UNKNOWN && _airportDataStore.GroundFrequencies != null)
      foreach (var freq in _airportDataStore.GroundFrequencies) {
        if (logLine.Contains(freq.Value.Readback.ToUpper())) {
          planeState = PlaneState.IS_RADIO_AWAY | PlaneState.IS_GND;
          break;
        }
      }

    if (planeState == PlaneState.UNKNOWN)
      _logger?.LogWarning("Failed to identify plane state for {Callsign} from {Readback}", _acapelaPlane, logLine);
    else if (State == ParserState.INIT_CATCHUP)
      _planeStates[_acapelaPlane] = planeState;
    else {
      NewPlaneState?.Invoke(_acapelaPlane, planeState);
      NewActivePlane?.Invoke(_acapelaPlane);
    }
    _acapelaPlane = null;
  }
}
