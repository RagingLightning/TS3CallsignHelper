namespace TS3CallsignHelper.API;
public enum ParserState {
  ERRORED = -1,
  NOT_INITIALIZED,
  INIT_START,
  INIT_CATCHUP,
  RUNNING
}

public enum ParserMode {
  NORMAL,
  GAME_START
}
