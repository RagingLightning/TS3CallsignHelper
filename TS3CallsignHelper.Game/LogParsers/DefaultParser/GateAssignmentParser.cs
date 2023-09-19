using Microsoft.Extensions.Logging;
using TS3CallsignHelper.API;
using TS3CallsignHelper.API.Dependencies;
using TS3CallsignHelper.API.Exceptions;
using TS3CallsignHelper.API.Logging;
using TS3CallsignHelper.API.LogParsing;
using TS3CallsignHelper.API.Stores;

namespace TS3CallsignHelper.Game.LogParsers.DefaultParser;
internal class GateAssignmentParser : ILogEntryParser {
  private readonly ILogger<GateAssignmentParser>? _logger;
  private readonly IGameStateStore _gameStateStore;

  private readonly Dictionary<string, string> _restarters = new();

  internal GateAssignmentParser(IDependencyStore dependencyStore) {
    _logger = dependencyStore.TryGet<ILoggerService>()?.GetLogger<GateAssignmentParser>();
    _gameStateStore = dependencyStore.TryGet<IGameStateStore>() ?? throw new MissingDependencyException(typeof(IGameStateStore));
  }

  public bool CanParse(string logLine, ParserState parserState) => logLine.StartsWith("Restarter airplanes /") || logLine.Contains(" => Terminal locked: ") || logLine.EndsWith(" => CLIENT AP STATE CHANGE");

  public void Parse(string logLine, ParserState parserState) {
    if (logLine.StartsWith("Restarter airplanes /")) { //
      var parent = logLine.Split("incoming: ")[1].Split(' ')[0];
      var child = logLine.Split("outgoing: ")[1].Split(' ')[0];
      _restarters[parent] = child;
      _logger?.LogInformation("Restarter: {ParentAirplane} -> {ChildAirplane}", parent, child);
    }
    else if (logLine.Contains(" => Terminal locked: ")) { // SERVER only
      var plane = logLine.Split(" => Terminal locked: ")[0].Split(' ')[^1];
      var gate = logLine.Split(" => Terminal locked: ")[1];

      var planeState = _gameStateStore.PlaneStates.GetValueOrDefault(plane, new PlaneStateInfo());
      planeState.Gate = gate.Replace("gate_", "Gate ");
      _logger?.LogInformation("Gate of {Airplane} is {Gate}", plane, gate);
      _gameStateStore.SetPlaneState(plane, planeState);

      if (_restarters.TryGetValue(plane, out var child)) {
        var childState = _gameStateStore.PlaneStates.GetValueOrDefault(child, new PlaneStateInfo());
        childState.Gate = gate.Replace("gate_", "Gate ");
        _logger?.LogInformation("Gate of {Airplane} is {Gate}", child, gate);
        _gameStateStore.SetPlaneState(child, childState);
      }
    }
    else if (logLine.EndsWith(" => CLIENT AP STATE CHANGE")) { // CLIENT only
      if (!logLine.Contains(" te: ")) return;
      var plane = logLine.Split(" * ")[1].Split(" => ")[0];
      var gate = logLine.Split(" te: ")[1].Split(" * ")[0];

      var planeState = _gameStateStore.PlaneStates.GetValueOrDefault(plane, new PlaneStateInfo());
      planeState.Gate = gate.Replace("gate_", "Gate ");
      _logger?.LogInformation("Gate of {Airplane} is {Gate}", plane, gate);
      _gameStateStore.SetPlaneState(plane, planeState);
    }
  }
}
