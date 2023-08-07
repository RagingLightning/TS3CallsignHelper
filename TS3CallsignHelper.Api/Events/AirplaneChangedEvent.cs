namespace TS3CallsignHelper.API.Events;

public delegate void AirplaneChangedEvent(AirplaneChangedEventArgs args);
public class AirplaneChangedEventArgs {
  public string Callsign { get; }

  public AirplaneChangedEventArgs(string callsign) {
    Callsign = callsign;
  }
}
