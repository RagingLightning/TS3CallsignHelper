﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TS3CallsignHelper.Game.Exceptions;

namespace TS3CallsignHelper.Game.Models;
public partial class AirportGaConfig {
  [GeneratedRegex("^(?<q0>\"?)(?<writename>.+?)\\k<q0>,(?<q1>\"?)(?<sayname>.+?)\\k<q1>,(?<q2>\"?)(?<airplanetype>.+?)\\k<q2>,(?<q3>\"?)(?<from>\\p{Lu}{4})\\k<q3>,(?<q4>\"?)(?<to>\\p{Lu}{4})\\k<q4>,(?<q5>\"?)(?<arrival>[0-9:]*?)\\k<q5>,(?<q6>\"?)(?<departure>[0-9:]*?)\\k<q6>,(?<q7>\"?)(?<approachaltitude>[0-9]*?)\\k<q7>,(?<q8>\"?)(?<stopandgo>\\d)\\k<q8>,(?<q9>\"?)(?<touchandgo>\\d)\\k<q9>,(?<q10>\"?)(?<lowapproach>\\d)\\k<q10>,(?<q11>\"?)(?<color>.+?)\\k<q11>,(?<q12>\"?)(?<special>.*?)\\k<q12>$")]
  private static partial Regex Parser();

  public IEnumerable<AirportGa> GaPlanes => _gaPlanes;

  private List<AirportGa> _gaPlanes;

  public AirportGaConfig(string configPath) {
    _gaPlanes = new List<AirportGa>();

    var configFile = File.Open(configPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    using var reader = new StreamReader(configFile);
    reader.ReadLine(); // first line contains headers
    while (reader.ReadLine() is string line) {
      var groups = Parser().Match(line).Groups;
      if (groups.Count == 1) throw new GaDefinitionFormatException(line);

      _gaPlanes.Add(new AirportGa(groups["writename"].Value,
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
                                  groups["color"].Value,
                                  groups["special"].Value));
    }
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
  public string Color { get; }
  public string Special { get; }

  internal AirportGa(string writeName, string sayName, string airplaneType, string from, string to, string arrival, string departure,
                   string approachAltitude, string stopAndGo, string touchAndGo, string lowApproach, string color, string special) : 
                   this(writeName, sayName, airplaneType, from, to, null, null, 0, false, false, false, color, special) {
    Arrival = DateTime.TryParse(arrival, out var res0) ? res0 : null;
    Departure = DateTime.TryParse(departure, out var res1) ? res1 : null;
    ApproachAltitude = int.TryParse(approachAltitude, out var res2) ? res2 : 0;
    StopAndGo = stopAndGo == "1";
    TouchAndGo = touchAndGo == "1";
    LowApproach = lowApproach == "1";
  }

  public AirportGa(string writeName, string sayName, string airplaneType, string from, string to, DateTime? arrival, DateTime? departure,
                   int approachAltitude, bool stopAndGo, bool touchAndGo, bool lowApproach, string color, string special) {
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
    Color = color;
    Special = special;
  }
}