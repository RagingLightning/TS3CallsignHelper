namespace TS3CallsignHelper.Api.Events;

public delegate void PlayerPositionChangedEvent(PlayerPositionChangedEventArgs args);
public class PlayerPositionChangedEventArgs {
  public PlayerPosition Position { get; }
  public bool IsActive { get; }

  public PlayerPositionChangedEventArgs(PlayerPosition position, bool isActive) {
    Position = position;
    IsActive = isActive;
  }
}
