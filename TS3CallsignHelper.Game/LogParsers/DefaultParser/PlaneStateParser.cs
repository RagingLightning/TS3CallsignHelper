using Microsoft.Extensions.Logging;
using TS3CallsignHelper.API;
using TS3CallsignHelper.API.Dependencies;
using TS3CallsignHelper.API.Exceptions;
using TS3CallsignHelper.API.Logging;
using TS3CallsignHelper.API.LogParsing;
using TS3CallsignHelper.API.Stores;
using TS3CallsignHelper.Game.Extensions;
using TS3CallsignHelper.Game.Stores;

namespace TS3CallsignHelper.Game.LogParsers.DefaultParser;
internal class PlaneStateParser : ILogEntryParser {
  private readonly ILogger<PlaneStateParser>? _logger;
  private readonly GameStateStore _gameStateStore;
  private readonly IAirportDataStore _airportDataStore;

  private string? _plane;
  private PlaneStateInfo? _planeStateInfo;

  internal PlaneStateParser(IDependencyStore dependencyStore) {
    _logger = dependencyStore.TryGet<ILoggerService>()?.GetLogger<PlaneStateParser>();
    _gameStateStore = (GameStateStore) (dependencyStore.TryGet<IGameStateStore>() ?? throw new MissingDependencyException(typeof(IGameStateStore)));
    _airportDataStore = dependencyStore.TryGet<IAirportDataStore>() ?? throw new MissingDependencyException(typeof(IAirportDataStore));
  }

  public bool CanParse(string logLine, ParserState parserState) => logLine.StartsWith("Gen TTS hash") || logLine.StartsWith("ADD TTS to Acapela: ") || logLine.StartsWith("COMMAND: ");

  public void Parse(string logLine, ParserState parserState) {
    if (logLine.StartsWith("COMMAND: "))
      ParseCommand(logLine);
    if (logLine.StartsWith("Gen TTS hash"))
      ParseHash(logLine);
    else if (logLine.StartsWith("ADD TTS to Acapela: "))
      ParseTTS(logLine, parserState);
  }
  
  private void ParseCommand(string logLine) {
    string plane = logLine[9..].Split(" ")[0];

    var line = logLine.Split($"{plane} "); // remove "COMMAND: AAL8192 "
    if (line.Length == 1) return;
    line = line[1].Split(' ');

    _planeStateInfo = new PlaneStateInfo(_gameStateStore.PlaneStates.GetValueOrDefault(plane, null));

    if (line.GetIndex("RUNWAY", "HOLD SHORT OF", "CROSS", "VACATE") is int runwayIdx) {
      _planeStateInfo.Runway = line[runwayIdx];
      _planeStateInfo.RunwayIntersection = null;
      _planeStateInfo.TaxiVia.Clear();
      _planeStateInfo.TaxiIntersection = null;
    }
    if (line.GetIndex("AT", "WIND IS ?") is int atIdx)
      _planeStateInfo.RunwayIntersection = line[atIdx];
    if (line.GetIndex("VIA") is int viaIdx)
      _planeStateInfo.TaxiVia = line[viaIdx..].ToList();
    if (line.GetIndex("INTERSECTION OF TAXIWAY") is int intersectionIdx)
      _planeStateInfo.TaxiIntersection = (line[intersectionIdx], line[intersectionIdx + 2]);
    if (line.GetIndex("TAXI TO") is int targetIdx)
      _planeStateInfo.TaxiIntersection = (line[targetIdx], string.Empty);
    if (line.GetIndex("HOLD SHORT OF") is int holdShortIdx) {
      if (line[holdShortIdx] == "TAXIWAY" || line[holdShortIdx] == "RUNWAY")
        _planeStateInfo.TaxiIntersection = (line[holdShortIdx + 1], string.Empty);
      else
        _planeStateInfo.TaxiIntersection = (line[holdShortIdx], string.Empty);
    }
  }

  private void ParseHash(string logLine) {
    var plane = logLine.Split(" ")[^1];
    if (!string.IsNullOrEmpty(_plane))
      _logger?.LogWarning("New Acapela hash for {newPlane} before {oldPlane} was evaluated", plane, _plane);
    else
      _logger?.LogDebug("New Acapela hash for {newPlane}", plane);
    _plane = plane;
  }
  
  private void ParseTTS(string logLine, ParserState parserState) {
    if (string.IsNullOrEmpty(_plane)) {
      _logger?.LogWarning("Tried to parse Acapela TTS without known callsign");
      return;
    }

    logLine = logLine.ToUpper();

    var planeState = PlaneState.UNKNOWN;
    if (logLine.Contains("APPROVED")) {
      if (logLine.Contains("PUSHBACK"))
        planeState = PlaneState.OUT_PUSHBACK_PROGRESS;
      else
        planeState = PlaneState.OUT_STARTUP_PROGRESS;
    }
    else if (logLine.Contains("ON FINAL"))
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
      else
        planeState = _gameStateStore.PlaneStates[_plane].State.IsIncoming() ? PlaneState.IN_TAXI_PROGRESS : PlaneState.OUT_TAXI_PROGRESS;
    }
    else if (logLine.Contains("READY")) {
      if (logLine.Contains("TAXI"))
        planeState = PlaneState.OUT_TAXI_REQUEST;
      else if (logLine.Contains("START"))
        planeState = PlaneState.OUT_STARTUP_REQUEST;
    }
    else if (logLine.Contains("TAXI")) {
      if (logLine.Contains("TO"))
        planeState = _gameStateStore.PlaneStates[_plane].State.IsIncoming() ? PlaneState.IN_TAXI_PROGRESS : PlaneState.OUT_TAXI_PROGRESS;
      else if (logLine.Contains("AND HOLD AT"))
        planeState = _gameStateStore.PlaneStates[_plane].State.IsIncoming() ? PlaneState.IN_TAXI_PROGRESS : PlaneState.OUT_TAXI_PROGRESS;
    }
    else if (logLine.Contains("REQUESTING")) {
      if (logLine.Contains("PUSH"))
        planeState = PlaneState.OUT_PUSHBACK_REQUEST;
    }
    else if (logLine.Contains("GOING AROUND"))
      planeState = PlaneState.IN_RWY_GO_AROUND;

    if (planeState == PlaneState.UNKNOWN && _airportDataStore.DepartureFrequencies != null)
      foreach (var freq in _airportDataStore.DepartureFrequencies) {
        if (logLine.Contains(freq.Value.Readback.ToUpper())) {
          planeState = _gameStateStore.PlaneStates[_plane].State.IsIncoming() ? PlaneState.IN_RAD_LEAVE : PlaneState.OUT_DEPARTURE;
          break;
        }
      }

    if (planeState == PlaneState.UNKNOWN && _airportDataStore.TowerFrequencies != null)
      foreach (var freq in _airportDataStore.TowerFrequencies) {
        if (logLine.Contains(freq.Value.Readback.ToUpper())) {
          planeState = _gameStateStore.PlaneStates[_plane].State.IsIncoming() ? PlaneState.IN_RAD_LEAVE : PlaneState.OUT_RAD_LEAVE;
          break;
        }
      }

    if (planeState == PlaneState.UNKNOWN && _airportDataStore.GroundFrequencies != null)
      foreach (var freq in _airportDataStore.GroundFrequencies) {
        if (logLine.Contains(freq.Value.Readback.ToUpper())) {
          planeState = _gameStateStore.PlaneStates[_plane].State.IsIncoming() ? PlaneState.IN_RAD_LEAVE : PlaneState.OUT_RAD_LEAVE;
          break;
        }
      }

    if (planeState == PlaneState.UNKNOWN) {
      _logger?.LogWarning("Failed to identify state for {Callsign} from {Readback}", _plane, logLine);
      return;
    }
    var planeStateInfo = _planeStateInfo ?? _gameStateStore.PlaneStates.GetValueOrDefault(_plane, new PlaneStateInfo());
    planeStateInfo.State = planeState;
    if (parserState == ParserState.INIT_CATCHUP)
      _gameStateStore.ForcePlaneState(_plane, planeStateInfo);
    else 
      _gameStateStore.SetPlaneState(_plane, planeStateInfo);
    _planeStateInfo = null;
    _plane = null;
  }
}
