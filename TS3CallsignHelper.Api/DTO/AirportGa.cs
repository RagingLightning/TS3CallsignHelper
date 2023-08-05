using System;

namespace TS3CallsignHelper.Api;
public class AirportGa {
  public string Writename { get; set; }
  public string Sayname { get; set; }
  public AirportAirplane AirplaneType { get; set; }
  public string Origin { get; set; }
  public string Destination { get; set; }
  public DateTime? Arrival { get; set; }
  public DateTime? Departure { get; set; }
  public int ApproachAltitude { get; set; }
  public bool StopAndGo { get; set; }
  public bool TouchAndGo { get; set; }
  public bool LowApproach { get; set; }
  public string Special { get; set; }

  public string FormatSayname() {
    return Sayname.Replace("ONE", "1")
        .Replace("TWO", "2")
        .Replace("THREE", "3")
        .Replace("TREE", "3")
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
