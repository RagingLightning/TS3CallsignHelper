using TS3CallsignHelper.Game.Enums;
using TS3CallsignHelper.Game.Models;

namespace TS3CallsignHelper.Game.LogParsers;
public class GameLogParserPlacesTwo : IGameLogParser {

  public event Action<string>? InstallDirDetermined;
  public event Action<GameInfo>? GameSessionSarted;
  public event Action? GameSessionEnded;
  public event Action<Metar>? MetarUpdated;
  public event Action<string>? NewActivePlane;
  public event Action<string, PlaneState>? NewPlaneState;

  private ParserState _state;
  private GameLogReader _reader;

  private ParserMode _mode;
  private IEntryParser _entryParser;

  private GameInfo? _currentGame;
  private Metar? _currentMetar;
  private Dictionary<string, PlaneState> _planeStates;

  public GameLogParserPlacesTwo() {
    _state = ParserState.NOT_INITIALIZED;
    _mode = ParserMode.NORMAL;
    _reader = new GameLogReader(Parse);
    _reader.EndOfLog += PostInit;

    _currentGame = null;
    _currentMetar = null;
    _planeStates = new Dictionary<string, PlaneState>();
  }

  public void Init(string logFile) {
    _state = ParserState.INIT_START;
    _reader.Init(logFile);
    _reader.Start();
  }

  public void PostInit() {
    if (_state != ParserState.INIT_CATCHUP) return;

    if (_currentGame != null)
      GameSessionSarted?.Invoke(_currentGame);
    if (_currentMetar != null)
      MetarUpdated?.Invoke(_currentMetar);
    foreach (var callsign in _planeStates.Keys) {
      NewPlaneState?.Invoke(callsign, _planeStates[callsign]);
    }

    _state = ParserState.RUNNING;
  }

  public ParserState GetState() => _state;

  public void Start() {
    //throw new NotImplementedException();
  }

  public void Stop() {
    //throw new NotImplementedException();
  }

  private void Parse(string logLine) {
    if (_mode == ParserMode.GAME_START || logLine.StartsWith("GAME START"))
      ParseGameStart(logLine);
    else if (logLine.StartsWith("Mono config path = ") && _state == ParserState.INIT_START)
      ParseInstallationDir(logLine);
    else if (logLine.StartsWith("METAR DOWNLOADED"))
      ParseMetar(logLine);
  }

  private void ParseInstallationDir(string logLine) {
    string installDir = logLine[20..].Replace("/MonoBleedingEdge/etc'", string.Empty);
    InstallDirDetermined?.Invoke(installDir);
    _state = ParserState.INIT_CATCHUP;
  }

  private void ParseGameStart(string logLine) {
    if (logLine.StartsWith("GAME START")) {
      _mode = ParserMode.GAME_START;
      _entryParser = new GameStartEntryParser();
      return;
    }
    if (_entryParser.Parse(logLine) is GameInfo info) {
      _mode = ParserMode.NORMAL;
      _currentGame = info;
      if (_state == ParserState.RUNNING)
        GameSessionSarted?.Invoke(info);
    }
  }

  private void ParseMetar(string logLine) {
    if (new MetarEntryParser().Parse(logLine) is Metar metar) {
      _currentMetar = metar;
      if (_state == ParserState.RUNNING)
        MetarUpdated?.Invoke(metar);
    }
  }
}
