using Hjson;
using TS3CallsignHelper.Game.DTOs;

namespace TS3CallsignHelper.Game.LogParsers;
internal partial class GameStartEntryParser : IEntryParser {

  private string json = string.Empty;

  public object? Parse(string logLine) {
    if (logLine.StartsWith("GAME START"))
      return null;
    if (logLine.StartsWith('-'))
      return CreateInfo();

    json += logLine.Trim();

    return null;
  }

  private GameInfo CreateInfo() {
    var gameInfo = new GameInfo();
    var jObject = JsonValue.Parse(json).Qo();

    gameInfo.AirportICAO = jObject["icao"].Qs();
    gameInfo.DatabaseFolder = jObject["path_databases"].Qs();
    gameInfo.AirplaneSetFolder = jObject["path_airplanes"].Qs();
    gameInfo.InstrumentSetFolder = jObject["path_instruments"].Qs();

    return gameInfo;
  }
}
