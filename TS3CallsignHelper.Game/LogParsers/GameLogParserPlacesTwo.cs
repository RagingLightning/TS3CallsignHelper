using Microsoft.Extensions.Logging;
using TS3CallsignHelper.API;
using TS3CallsignHelper.API.Dependencies;
using TS3CallsignHelper.API.Exceptions;
using TS3CallsignHelper.API.Logging;
using TS3CallsignHelper.API.LogParsing;
using TS3CallsignHelper.API.Stores;
using TS3CallsignHelper.Game.Services;
using TS3CallsignHelper.Game.Stores;

namespace TS3CallsignHelper.Game.LogParsers;
internal class GameLogParserPlacesTwo : IGameLogParser {
  private readonly ILogger<GameLogParserPlacesTwo>? _logger;
  private readonly IInitializationProgressService _initializationProgressService;
  private readonly GameStateStore _gameStateStore;

  private ParserState _state;
  public ParserState State => _state;

  private readonly GameLogReader _reader;
  private readonly List<ILogEntryParser> _parsers = new();

  /// <summary>
  /// Requires <seealso cref="IInitializationProgressService"/>, <seealso cref="IAirportDataStore"/>
  /// </summary>
  /// <param name="dependencyStore"></param>
  internal GameLogParserPlacesTwo(IDependencyStore dependencyStore) {
    _logger = dependencyStore.TryGet<ILoggerService>()?.GetLogger<GameLogParserPlacesTwo>();
    _initializationProgressService = dependencyStore.TryGet<IInitializationProgressService>() ?? throw new MissingDependencyException(typeof(IInitializationProgressService));
    _gameStateStore = (GameStateStore) (dependencyStore.TryGet<IGameStateStore>() ?? throw new MissingDependencyException(typeof(IGameStateStore)));

    _state = ParserState.NOT_INITIALIZED;
    _reader = new GameLogReader(Parse, _initializationProgressService);
    _reader.EndOfLog += PostInit;
  }

  public void Register(ILogEntryParser logParser) {
    _parsers.Add(logParser);
  }

  public void Unregister(ILogEntryParser logParser) {
    _parsers.Remove(logParser);
  }

  internal void Init(string logFolder) {
    _logger?.LogDebug("Initializing log parser for {logFile}", Path.Combine(logFolder, "Player.log"));
    _state = ParserState.INIT_START;
    _reader.Init(logFolder);
  }

  private void PostInit() {
    if (_state != ParserState.INIT_CATCHUP) return;
    _logger?.LogDebug("Finished initial catchup");
    _reader.EndOfLog -= PostInit;
    _initializationProgressService.LogFileProgress = 1;

    _initializationProgressService.Completed = true;
    _state = ParserState.RUNNING;
  }

  internal void Start() {
    _reader.Start();
  }

  internal void Stop() {
    //throw new NotImplementedException();
  }

  private void Parse(string logLine) {
    _logger?.LogTrace("Start parsing of {line}", logLine);
    if (logLine.StartsWith("Mono config path = ") && _state == ParserState.INIT_START)
      ParseInstallationDir(logLine);
    foreach (var parser in _parsers)
      if (parser.CanParse(logLine, State)) parser.Parse(logLine, State);
  }

  private void ParseInstallationDir(string logLine) {
    string installDir = logLine[20..].Replace("/MonoBleedingEdge/etc'", string.Empty);
    _logger?.LogDebug("Raising InstallDirDetermined for {installDir}", installDir);
    _gameStateStore.SetInstallDir(installDir);
    _state = ParserState.INIT_CATCHUP;
  }
}
