namespace TS3CallsignHelper.Api.Events;

public delegate void LogLineReadEventHandler(LogLineReadEventArgs args);
public class LogLineReadEventArgs {
  public string LogLine { get; }

  public LogLineReadEventArgs(string logLine) {
    LogLine = logLine;
  }
}
