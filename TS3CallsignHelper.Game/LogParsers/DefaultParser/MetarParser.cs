using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.RegularExpressions;
using TS3CallsignHelper.API;
using TS3CallsignHelper.API.Dependencies;
using TS3CallsignHelper.API.Exceptions;
using TS3CallsignHelper.API.Logging;
using TS3CallsignHelper.API.LogParsing;
using TS3CallsignHelper.API.Stores;

namespace TS3CallsignHelper.Game.LogParsers;
internal partial class MetarParser : ILogEntryParser {
  [GeneratedRegex(@"^(?<icao>[A-Z]{4}) (?<time>\d{6})[zZ] (?<auto>AUTO )?(?<wind>[0-9G]+?)KT (?<var>[0-9V] )?(?<vis>.+) (?<temp>[\dM]+?)/(?<dew>[\dM]+?) (?<qnh>[AQ][\d.]+)(?<tempo>.*)$")]
  private partial Regex Metar();
  private readonly ILogger<MetarParser>? _logger;
  private readonly IGameStateStore _gameStateStore;

  internal MetarParser(IDependencyStore dependencyStore) {
    _logger = dependencyStore.TryGet<ILoggerService>()?.GetLogger<MetarParser>();
    _gameStateStore = dependencyStore.TryGet<IGameStateStore>() ?? throw new MissingDependencyException(typeof(IGameStateStore));
  }

  public bool CanParse(string logLine, ParserState parserState) => logLine.StartsWith("METAR DOWNLOADED: ");

  public void Parse(string logLine, ParserState state) {
    logLine = logLine[18..].Trim();

    var groups = Metar().Match(logLine).Groups;
    if (groups.Count == 1) {
      _logger?.LogWarning("Failed to load Metar: {Metar}", logLine);
      return;
    }

    var airport = groups["icao"].Value;
    var reportTime = ParseReportTime(groups["time"].Value);
    var automatic = groups["auto"].Value != string.Empty;
    var winds = ParseWind(groups["wind"].Value, groups["var"].Value.Trim());
    var temperature = ParseTemperature(groups["temp"].Value);
    var dewpoint = ParseTemperature(groups["dew"].Value);
    var qnh = ParsePressure(groups["qnh"].Value);

    /*_gameStateStore.CurrentMetar = new Metar {
      Airport = airport,
      ReportTime = reportTime,
      Automatic = automatic,
      Winds = winds,
      Temperature = temperature,
      Dewpoint = dewpoint,
      Qnh = qnh
    };*/
  }

  private DateTime? ParseReportTime(string value) {
    value += DateTime.UtcNow.ToString("MMyyyy") + "00";
    return DateTime.ParseExact(value, "ddHHmmMMyyyyss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
  }

  private Wind ParseWind(string winds, string variable) {
    Wind wind = new Wind();

    if (!string.IsNullOrEmpty(variable)) {
      wind.VariableFrom = int.Parse(variable[..3]);
      wind.VariableTo = int.Parse(variable[4..]);
    }
    wind.Direction = int.Parse(winds[..3]);
    wind.Magnitude = int.Parse(winds[3..5]);
    if (winds.Contains('G'))
      wind.Gusts = int.Parse(winds[6..8]);

    return wind;
  }

  private int ParseTemperature(string value) {
    return int.Parse(value);
  }

  private Pressure ParsePressure(string value) {
    return new Pressure { Unit = value.ToCharArray()[0], Value = double.Parse(value[1..]) };
  }
}
