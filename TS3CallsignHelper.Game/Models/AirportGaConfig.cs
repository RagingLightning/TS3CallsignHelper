using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.RegularExpressions;
using TS3CallsignHelper.Game.Exceptions;
using TS3CallsignHelper.Game.Services;

namespace TS3CallsignHelper.Game.Models;
public partial class AirportGaConfig {
  [GeneratedRegex("^(?<q0>\"?)(?<writename>.+?)\\k<q0>,(?<q1>\"?)(?<sayname>.+?)\\k<q1>,(?<q2>\"?)(?<airplanetype>.+?)\\k<q2>,(?<q3>\"?)(?<from>[A-Z]{4})\\k<q3>,(?<q4>\"?)(?<to>[A-Z]{4})\\k<q4>,(?<q5>\"?)(?<arrival>[0-9:]*?)\\k<q5>,(?<q6>\"?)(?<departure>[0-9:]*?)\\k<q6>,(?<q7>\"?)(?<approachaltitude>[0-9]*?)\\k<q7>,(?<q8>\"?)(?<stopandgo>\\d)\\k<q8>,(?<q9>\"?)(?<touchandgo>\\d)\\k<q9>,(?<q10>\"?)(?<lowapproach>\\d)\\k<q10>,(?<q11>\"?)(?<special>.*?)\\k<q11>$")]
  private static partial Regex Parser();
  private readonly ILogger<AirportGaConfig> _logger;

  public IEnumerable<AirportGa> GaPlanes => _gaPlanes.Values;

  private Dictionary<string, AirportGa> _gaPlanes;

  public AirportGaConfig(string configPath, IServiceProvider serviceProvider, InitializationProgressService initializationProgress) {
    _logger = serviceProvider.GetRequiredService<ILogger<AirportGaConfig>>();

    _gaPlanes = new Dictionary<string, AirportGa>();

    initializationProgress.StatusMessage = "Loading GA schedule...";
    _logger.LogDebug("Loading ga schedule from {Config}", configPath);
    var stream = File.Open(configPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    using var reader = new StreamReader(stream);
    reader.ReadLine(); // first line contains headers
    while (reader.ReadLine() is string line) {
      _logger.LogTrace("Loading ga flight from {Line}", line);
      var groups = Parser().Match(line).Groups;
      if (groups.Count == 1) throw new GaDefinitionFormatException(line);

      var callsign = groups["writename"].Value;
      var airplane = new AirportGa(groups["writename"].Value,
                                  groups["sayname"].Value,
                                  groups["airplanetype"].Value,
                                  groups["from"].Value,
                                  groups["to"].Value,
                                  groups["arrival"].Value,
                                  groups["departure"].Value,
                                  groups["approachaltitude"].Value,
                                  groups["stopandgo"].Value,
                                  groups["touchandgo"].Value,
                                  groups["lowapproach"].Value,
                                  groups["special"].Value);
      _gaPlanes.Add(callsign, airplane);
      _logger.LogDebug("Added ga schedule entry {@GaSchedule}", airplane);
      initializationProgress.GaProgress = ((float) stream.Position) / stream.Length;
    }
    initializationProgress.GaProgress = 1;
  }

  public bool TryGet(string code, out AirportGa ga) {
    return _gaPlanes.TryGetValue(code, out ga);
  }

}

public class AirportGa {
  public string WriteName { get; }
  public string SayName { get; }
  public string AirplaneType { get; }
  public string From { get; }
  public string To { get; }
  public DateTime? Arrival { get; }
  public DateTime? Departure { get; }
  public int ApproachAltitude { get; }
  public bool StopAndGo { get; }
  public bool TouchAndGo { get; }
  public bool LowApproach { get; }
  public string Special { get; }

  internal AirportGa(string writeName, string sayName, string airplaneType, string from, string to, string arrival, string departure,
                   string approachAltitude, string stopAndGo, string touchAndGo, string lowApproach, string special) : 
                   this(writeName, sayName, airplaneType, from, to, null, null, 0, false, false, false, special) {
    Arrival = DateTime.TryParse(arrival, out var res0) ? res0 : null;
    Departure = DateTime.TryParse(departure, out var res1) ? res1 : null;
    ApproachAltitude = int.TryParse(approachAltitude, out var res2) ? res2 : 0;
    StopAndGo = stopAndGo == "1";
    TouchAndGo = touchAndGo == "1";
    LowApproach = lowApproach == "1";
  }

  public AirportGa(string writeName, string sayName, string airplaneType, string from, string to, DateTime? arrival, DateTime? departure,
                   int approachAltitude, bool stopAndGo, bool touchAndGo, bool lowApproach, string special) {
    WriteName = writeName;
    SayName = sayName;
    AirplaneType = airplaneType;
    From = from;
    To = to;
    Arrival = arrival;
    Departure = departure;
    ApproachAltitude = approachAltitude;
    StopAndGo = stopAndGo;
    TouchAndGo = touchAndGo;
    LowApproach = lowApproach;
    Special = special;
  }

  public static string FormatSayName(string name) {
    return name.Replace("ONE", "1")
        .Replace("TWO", "2")
        .Replace("THREE", "3")
        .Replace("FOUR", "4")
        .Replace("FIVE", "5")
        .Replace("FIFE", "5")
        .Replace("SIX", "6")
        .Replace("SEVEN", "7")
        .Replace("EIGHT", "8")
        .Replace("NINER", "9")
        .Replace("NYNER", "9")
        .Replace("ZERO", "0")
        .Replace("ALFA", "A")
        .Replace("ALPHA", "A")
        .Replace("BRAVO", "B")
        .Replace("CHARLIE", "C")
        .Replace("DELTA", "D")
        .Replace("ECHO", "E")
        .Replace("FOXTROT", "F")
        .Replace("GOLF", "G")
        .Replace("HOTEL", "H")
        .Replace("INDIA", "I")
        .Replace("JULIET", "J")
        .Replace("JULIETT", "J")
        .Replace("KILO", "K")
        .Replace("LIMA", "L")
        .Replace("MIKE", "M")
        .Replace("NOVEMBER", "N")
        .Replace("OSCAR", "O")
        .Replace("PAPA", "P")
        .Replace("QUEBEC", "Q")
        .Replace("ROMEO", "R")
        .Replace("SIERRA", "S")
        .Replace("TANGO", "T")
        .Replace("UNIFORM", "U")
        .Replace("VICTOR", "V")
        .Replace("WHISKEY", "W")
        .Replace("XRAY", "X")
        .Replace("YANKEE", "Y")
        .Replace("ZULU", "Z");
  }
}