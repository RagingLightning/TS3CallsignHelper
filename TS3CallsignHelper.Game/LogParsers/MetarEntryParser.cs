using System.Globalization;
using System.Text.RegularExpressions;
using TS3CallsignHelper.Common.DTOs;

namespace TS3CallsignHelper.Game.LogParsers;
internal partial class MetarEntryParser : IEntryParser {
  [GeneratedRegex(@"^(?<icao>[A-Z]{4}) (?<time>\d{6})[zZ] (?<auto>AUTO )?(?<wind>[0-9G]+?)KT (?<var>[0-9V] )?(?<vis>.+) (?<temp>[\dM]+?)/(?<dew>[\dM]+?) (?<qnh>[AQ][\d.]+)$")]
  private partial Regex Metar();

  public object? Parse(string logLine) {
    logLine = logLine[18..].Trim();

    var groups = Metar().Match(logLine).Groups;
    if (groups.Count == 1) return logLine;

    var airport = groups["icao"].Value;
    var reportTime = ParseReportTime(groups["time"].Value);
    var automatic = groups["auto"].Value != string.Empty;
    var winds = ParseWind(groups["wind"].Value, groups["var"].Value.Trim());
    var temperature = ParseTemperature(groups["temp"].Value);
    var dewpoint = ParseTemperature(groups["dew"].Value);
    var qnh = ParsePressure(groups["qnh"].Value);
    return new Metar {
      Airport = airport,
      ReportTime = reportTime,
      Automatic = automatic,
      Winds = winds,
      Temperature = temperature,
      Dewpoint = dewpoint,
      Qnh = qnh
    };
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
