using System;

namespace TS3CallsignHelper.Api;
public class AirportScheduleEntry {
  /// <summary>
  /// Determines the airplane livery
  /// </summary>
  public AirportAirline? Operator { get; set; }

  /// <summary>
  /// Determines the airplane callsign
  /// </summary>
  public AirportAirline? Airline { get; set; }
  public string FlightNumber { get; set; } = string.Empty;
  public AirportAirplane? AirplaneType { get; set; }
  public string Origin { get; set; } = string.Empty;
  public string Destination { get; set; } = string.Empty;
  public DateTime? Arrival { get; set; }
  public DateTime? Departure { get; set; }
  public int ApproachAltitude { get; set; }
  public string Special { get; set; } = string.Empty;
}
