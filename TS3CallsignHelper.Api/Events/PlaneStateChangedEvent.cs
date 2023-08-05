namespace TS3CallsignHelper.Api.Events;

public delegate void PlaneStateChangedEvent(PlaneStateChangedEventArgs args);
public class PlaneStateChangedEventArgs {
  public string Callsign { get; }
  public PlaneState State { get; }

  public PlaneStateChangedEventArgs(string callsign, PlaneState state) {
    Callsign = callsign;
    State = state;
  }
}
