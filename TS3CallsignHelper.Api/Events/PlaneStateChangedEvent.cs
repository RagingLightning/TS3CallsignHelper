namespace TS3CallsignHelper.API.Events;

public delegate void PlaneStateChangedEvent(PlaneStateChangedEventArgs args);
public class PlaneStateChangedEventArgs {
  public string Callsign { get; }
  public PlaneStateInfo State { get; }

  public PlaneStateChangedEventArgs(string callsign, PlaneStateInfo state) {
    Callsign = callsign;
    State = state;
  }
}
