namespace TS3CallsignHelper.Api.Events;

public delegate void GameSessionStartedEventHandler(GameSessionStartedEventArgs args);
public class GameSessionStartedEventArgs {
  public GameInfo Info { get; }

  public GameSessionStartedEventArgs(GameInfo info) {
    Info = info;
  }
}
