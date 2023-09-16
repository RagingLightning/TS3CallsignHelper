using Hjson;
using Microsoft.Extensions.Logging;
using TS3CallsignHelper.API;
using TS3CallsignHelper.API.Dependencies;
using TS3CallsignHelper.API.Exceptions;
using TS3CallsignHelper.API.Logging;
using TS3CallsignHelper.API.LogParsing;
using TS3CallsignHelper.API.Stores;

namespace TS3CallsignHelper.Game.LogParsers.DefaultParser;
internal class GameSessionParser : ILogEntryParser {
  private readonly ILogger<GameSessionParser>? _logger;
  private readonly IGameStateStore _gameStateStore;

  private bool _parsingGameStart = false;
  private string _json = string.Empty;

  internal GameSessionParser(IDependencyStore dependencyStore) {
    _logger = dependencyStore.TryGet<ILoggerService>()?.GetLogger<GameSessionParser>();
    _gameStateStore = dependencyStore.TryGet<IGameStateStore>() ?? throw new MissingDependencyException(typeof(IGameStateStore));
  }

  public bool CanParse(string logLine, ParserState parserState) => _parsingGameStart || logLine.StartsWith("UnloadTime: ") || logLine.StartsWith("GAME START");

  public void Parse(string logLine, ParserState parserState) {
    if (_parsingGameStart || logLine.StartsWith("GAME START"))
      ParseGameStart(logLine, parserState);
    else if (logLine.StartsWith("UnloadTime: ") && parserState >= ParserState.INIT_CATCHUP)
      _gameStateStore.EndGame();
  }

  private void ParseGameStart(string logLine, ParserState parserState) {
    if (logLine.StartsWith("GAME START")) {
      _logger?.LogDebug("Begin parsing of game information");
      _parsingGameStart = true;
      _json = string.Empty;
      return;
    }
    if (logLine.StartsWith('-') && parserState >= ParserState.INIT_CATCHUP) {
      _logger?.LogDebug("Parsing of game information finished");
      _parsingGameStart = false;
      _gameStateStore.StartGame(CreateInfo());
      return;
    }
    _json += logLine.Trim();
  }

  private GameInfo CreateInfo() {
    var gameInfo = new GameInfo();
    var jObject = JsonValue.Parse(_json).Qo();

    gameInfo.AirportICAO = jObject["icao"].Qs();
    gameInfo.DatabaseFolder = jObject["path_databases"].Qs();
    gameInfo.AirplaneSetFolder = jObject["path_airplanes"].Qs();
    gameInfo.InstrumentSetFolder = jObject["path_instruments"].Qs();
    gameInfo.StartHour = jObject["time"].Qi();

    return gameInfo;
  }
}
