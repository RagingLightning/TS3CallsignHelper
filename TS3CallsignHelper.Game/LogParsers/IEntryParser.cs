namespace TS3CallsignHelper.Game.LogParsers;
internal interface IEntryParser {

  public object? Parse(string logLine);

}
