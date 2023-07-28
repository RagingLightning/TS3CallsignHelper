using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TS3CallsignHelper.Game.Models;

namespace TS3CallsignHelper.Game.LogParsers;
internal partial class MetarEntryParser : IEntryParser {
  [GeneratedRegex(@"^(?<icao>[A-Z]{4}) (?<time>\d{6})[zZ] (?<auto>AUTO )?(?<wind>[0-9G])KT (?<var>[0-9V] )?(?<vis>.+) (?<temp>[\dM]+?)/(?<dew>[\dM]+?) (?<qnh>[AQ][\d.]+)$")]
  private partial Regex Metar();
  public object? Parse(string logLine) {
    logLine = logLine[18..].Trim();

    var groups = Metar().Match(logLine).Groups;
    if (groups.Count == 1) return null;

    return new Metar {
      Airport = groups["icao"].Value,
      ReportTime = ParseReportTime(groups["time"].Value),
      Automatic = !string.IsNullOrEmpty(groups["auto"].Value),
      Winds = ParseWind(groups["dir"].Value, groups["mag"].Value.Trim()),
      Temperature = ParseTemperature(groups["temp"].Value),
      Dewpoint = ParseTemperature(groups["dew"].Value),
      Qnh = ParsePressure(groups["qnh"].Value)
    };
  }

  private DateTime? ParseReportTime(string value) {
    int hour = int.Parse(value[2..2]);
    int minute = int.Parse(value[4..2]);
    return DateTime.Parse($"{hour}:{minute}:00", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
  }

  private Wind ParseWind(string winds, string variable) {
    Wind wind = new Wind();

    if (!string.IsNullOrEmpty(variable)) {
      wind.VariableFrom = int.Parse(variable[..3]);
      wind.VariableTo = int.Parse(variable[4..]);
    }
    wind.Direction = int.Parse(winds[..3]);
    wind.Magnitude = int.Parse(winds[3..2]);
    if (winds.Contains('G'))
      wind.Gusts = int.Parse(winds[6..2]);

    return wind;
  }

  private int ParseTemperature(string value) {
    throw new NotImplementedException();
  }

  private Pressure ParsePressure(string value) {
    throw new NotImplementedException();
  }
}
