using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TS3CallsignHelper.API;
using TS3CallsignHelper.API.Dependencies;
using TS3CallsignHelper.API.Exceptions;
using TS3CallsignHelper.API.LogParsing;
using TS3CallsignHelper.API.Stores;

namespace TS3CallsignHelper.Game.LogParsers.DefaultParser;
internal class PlaneSelectionParser : ILogEntryParser {
  private readonly IGameStateStore _gameStateStore;
  private readonly IAirportDataStore _airportDataStore;

  internal PlaneSelectionParser(IDependencyStore dependencyStore) {
    _gameStateStore = dependencyStore.TryGet<IGameStateStore>() ?? throw new MissingDependencyException(typeof(IGameStateStore));
    _airportDataStore = dependencyStore.TryGet<IAirportDataStore>() ?? throw new MissingDependencyException(typeof(IAirportDataStore));
  }

  public bool CanParse(string logLine, ParserState parserState)
    => parserState == ParserState.RUNNING && (logLine.StartsWith("COMMAND: ") || logLine.StartsWith("SET PlANE: "));

  public void Parse(string logLine, ParserState parserState) {
    string callsign = string.Empty;
    if (logLine.StartsWith("COMMAND: "))
      callsign = logLine[9..].Split(" ")[0];
    else if (logLine.StartsWith("SET PlANE: "))
      callsign = logLine.Split(" ")[^1];

    if (callsign == string.Empty) return;

    if (_airportDataStore.Schedule?.ContainsKey(callsign) == true || _airportDataStore.GaPlanes?.ContainsKey(callsign) == true)
      _gameStateStore.CurrentAirplane = callsign;
  }
}
