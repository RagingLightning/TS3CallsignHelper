using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TS3CallsignHelper.Common.DTOs;
using TS3CallsignHelper.Game.Enums;
using TS3CallsignHelper.Game.Services;

namespace TS3CallsignHelper.Game.LogParsers;
public class GameLogParserPlacesTwo : IGameLogParser {
  private readonly ILogger _logger;
  private readonly InitializationProgressService _initializationProgress;

  public event Action<float, string>? InitializationProgress;
  public event Action<string>? InstallDirDetermined;
  public event Action<GameInfo>? GameSessionSarted;
  public event Action? GameSessionEnded;
  public event Action<Metar>? MetarUpdated;
  public event Action<string>? NewActivePlane;
  public event Action<string, PlaneState>? NewPlaneState;

  private ParserState _state;
  public ParserState State => _state;

  private GameLogReader _reader;

  private ParserMode _mode;
  private IEntryParser _entryParser;

  private string? _acapelaPlane;

  //only used during ParserState.INIT_CATCHUP
  private GameInfo? _currentGame;
  private Metar? _currentMetar;
  private Dictionary<string, PlaneState> _planeStates;

  public GameLogParserPlacesTwo(IServiceProvider serviceProvider) {
    _logger = serviceProvider.GetRequiredService<ILogger<GameLogParserPlacesTwo>>();
    _initializationProgress = serviceProvider.GetRequiredService<InitializationProgressService>();

    _state = ParserState.NOT_INITIALIZED;
    _mode = ParserMode.NORMAL;
    _reader = new GameLogReader(Parse, _initializationProgress);
    _reader.EndOfLog += PostInit;

    _currentGame = null;
    _currentMetar = null;
    _planeStates = new Dictionary<string, PlaneState>();
  }

  public void Init(string logFolder) {
    _logger.LogDebug("Initializing log parser for {logFile}", Path.Combine(logFolder, "Player.log"));
    _state = ParserState.INIT_START;
    _reader.Init(logFolder);
  }

  public void PostInit() {
    if (_state != ParserState.INIT_CATCHUP) return;
    _logger.LogDebug("Finished initial catchup");
    _reader.EndOfLog -= PostInit;
    _initializationProgress.LogFileProgress = 1;

    if (_currentGame == null) { _initializationProgress.Completed = true; return; }

    _logger.LogTrace("Raising GameSessionSarted for determined game session {@session}", _currentGame);
    GameSessionSarted?.Invoke(_currentGame);
    if (_currentMetar != null) {
      _logger.LogTrace("Raising MetarUpdated for determined metar {@metar}", _currentMetar);
      MetarUpdated?.Invoke(_currentMetar);
    }
    foreach (var callsign in _planeStates.Keys) {
      _logger.LogTrace("Raising NewPlaneState for {airplane} state {state}", callsign, _planeStates[callsign]);
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
    _logger.LogTrace("Start parsing of {line}", logLine);
    if (_mode == ParserMode.GAME_START || logLine.StartsWith("GAME START"))
      ParseGameStart(logLine);
    else if (logLine.StartsWith("UnloadTime: "))
      ParseGameEnd(logLine);
    else if (logLine.StartsWith("Mono config path = ") && _state == ParserState.INIT_START)
      ParseInstallationDir(logLine);
    else if (logLine.StartsWith("METAR DOWNLOADED: "))
      ParseMetar(logLine);
    else if (logLine.StartsWith("COMMAND: "))
      ParseCommand(logLine);
    else if (logLine.StartsWith("Clicked: "))
      ParseClicked(logLine);
    else if (logLine.StartsWith("Gen TTS hash"))
      ParseAcapelaHash(logLine);
    else if (logLine.StartsWith("ADD TTS to Acapela: "))
      ParseAcapelaTTS(logLine);
  }

  private void ParseInstallationDir(string logLine) {
    string installDir = logLine[20..].Replace("/MonoBleedingEdge/etc'", string.Empty);
    _logger.LogDebug("Raising InstallDirDetermined for {installDir}", installDir);
    InstallDirDetermined?.Invoke(installDir);
    _state = ParserState.INIT_CATCHUP;
  }

  private void ParseGameStart(string logLine) {
    if (logLine.StartsWith("GAME START")) {
      _logger.LogDebug("Start parsing game info for new session");
      _mode = ParserMode.GAME_START;
      _entryParser = new GameStartEntryParser();
      return;
    }
    if (_entryParser.Parse(logLine) is GameInfo info) {
      _logger.LogDebug("Finished parsing new game info: {@info}", info);
      _mode = ParserMode.NORMAL;
      _currentGame = info;
      if (_state == ParserState.RUNNING) {
        _logger.LogTrace("Raising GameSessionSarted");
        GameSessionSarted?.Invoke(info);
      }
      else
        _initializationProgress.Details = $"Detected session {_currentGame.AirportICAO} / {_currentGame.DatabaseFolder}";
    }
  }

  private void ParseGameEnd(string logLine) {
    //_currentGame = null;
    //_currentMetar = null;
    //_planeStates.Clear();
    if (_state == ParserState.RUNNING) {
      _logger.LogTrace("Raising GameSessionEnded");
      GameSessionEnded?.Invoke();
    }
    else
      _initializationProgress.Details = string.Empty;
  }

  private void ParseMetar(string logLine) {
    var result = new MetarEntryParser().Parse(logLine);
    if (result is Metar metar) {
      _logger.LogDebug("Finished parsing metar: {@metar}", metar);
      _currentMetar = metar;
      if (_state == ParserState.RUNNING) {
        _logger.LogTrace("Raising MetarUpdated");
        MetarUpdated?.Invoke(metar);
      }
    }
    else
      _logger.LogWarning("Failed to parse metar: {metar}", result);
  }

  private void ParseCommand(string logLine) {
    string plane = logLine[9..].Split(" ")[0];
    if (_state == ParserState.RUNNING) {
      _logger.LogDebug("Raising NewActivePlane for {callsign}", plane);
      NewActivePlane?.Invoke(plane);
    }
  }

  private void ParseClicked(string logLine) {
    string plane = logLine.Split(" ")[^1];
    if (_state == ParserState.RUNNING) {
      _logger.LogDebug("Raising NewActivePlane for {callsign}", plane);
      NewActivePlane?.Invoke(plane);
    }
  }

  private void ParseAcapelaHash(string logLine) {
    var plane = logLine.Split(" ")[^1];
    if (!string.IsNullOrEmpty(_acapelaPlane))
      _logger.LogWarning("New Acapela hash for {newPlane} before {oldPlane} was evaluated", plane, _acapelaPlane);
    else
      _logger.LogDebug("New Acapela hash for {newPlane}", plane);
    _acapelaPlane = plane;
  }

  private void ParseAcapelaTTS(string logLine) {
    _acapelaPlane = null;
  }
}
