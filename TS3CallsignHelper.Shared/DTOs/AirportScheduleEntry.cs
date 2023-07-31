namespace TS3CallsignHelper.Common.DTOs;
public class AirportScheduleEntry {
  /// <summary>
  /// Determines the airplane livery
  /// </summary>
  public AirportAirline Operator { get; set; }

  /// <summary>
  /// Determines the airplane callsign
  /// </summary>
  public AirportAirline Airline { get; set; }
  public string FlightNumber { get; set; }
  public AirportAirplane AirplaneType { get; set; }
  public string Origin { get; set; }
  public string Destination { get; set; }
  public DateTime? Arrival { get; set; }
  public DateTime? Departure { get; set; }
  public int ApproachAltitude { get; set; }
  public string Special { get; set; }
}
