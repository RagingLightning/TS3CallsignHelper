namespace TS3CallsignHelper.API.LogParsing;
public interface IGameLogParser {
  ParserState State { get; }
  void Register(ILogEntryParser parser);
  void Unregister(ILogEntryParser parser);
}
