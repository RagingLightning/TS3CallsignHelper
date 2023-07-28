using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS3CallsignHelper.Game.Enums;
public enum ParserState {
  ERRORED = -1,
  NOT_INITIALIZED,
  INIT_START,
  INIT_CATCHUP,
  RUNNING,
  PAUSED,
  STOPPED
}

public enum ParserMode {
  NORMAL,
  GAME_START
}
