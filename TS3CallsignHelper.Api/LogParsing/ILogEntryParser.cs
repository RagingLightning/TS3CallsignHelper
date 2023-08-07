namespace TS3CallsignHelper.API.LogParsing;
public interface ILogEntryParser {
  bool CanParse(string logLine, ParserState parserState);
  void Parse(string logLine, ParserState parserState);
}
