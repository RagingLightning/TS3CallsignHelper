namespace TS3CallsignHelper.API;
public static class PlaneStates {

  public static bool Is(this PlaneState state, PlayerPosition position) {
    switch (position) {
      case PlayerPosition.Ground: return ((uint) state & IS_GND) != 0;
      case PlayerPosition.Tower: return ((uint) state & IS_TWR) != 0;
      case PlayerPosition.Departure: return ((uint) state & IS_DEP) != 0;
    }
    return false;
  }
  public static bool IsInitial(this PlaneState state, PlayerPosition position) {
    switch (position) {
      case PlayerPosition.Ground: return ((uint) state & IS_GND_INIT) != 0;
      case PlayerPosition.Tower: return ((uint) state & IS_TWR_INIT) != 0;
      case PlayerPosition.Departure: return ((uint) state & IS_DEP_INIT) != 0;
    }
    return false;
  }

  public static bool IsIncoming(this PlaneState state) => ((uint) state & IS_INCOMING) != 0;

  internal const uint IS_GND = 0x01;
  internal const uint IS_TWR = 0x02;
  internal const uint IS_DEP = 0x04;

  internal const uint IS_GND_INIT = 0x10;
  internal const uint IS_TWR_INIT = 0x20;
  internal const uint IS_DEP_INIT = 0x40;

  internal const uint IS_INCOMING = 0x08;
}

public enum PlaneState : uint {
  UNKNOWN = 0x00,

  OUT_STARTUP_REQUEST = 0x100 | PlaneStates.IS_GND | PlaneStates.IS_GND_INIT,
  OUT_PUSHBACK_REQUEST = 0x200 | PlaneStates.IS_GND | PlaneStates.IS_GND_INIT,
  OUT_STARTUP_PROGRESS = 0x300 | PlaneStates.IS_GND,
  OUT_PUSHBACK_PROGRESS = 0x400 | PlaneStates.IS_GND,
  OUT_TAXI_REQUEST = 0x500 | PlaneStates.IS_GND,
  OUT_TAXI_PROGRESS = 0x600 | PlaneStates.IS_GND,
  OUT_RWY_WAITING = 0x700 | PlaneStates.IS_TWR | PlaneStates.IS_TWR_INIT,
  OUT_RWY_LINE_UP = 0x800 | PlaneStates.IS_TWR,
  OUT_RWY_LINE_UP_BEHIND = 0x900 | PlaneStates.IS_TWR,
  OUT_RWY_TAKEOFF = 0xa00 | PlaneStates.IS_TWR,
  OUT_DEPARTURE = 0xb00 | PlaneStates.IS_DEP | PlaneStates.IS_DEP_INIT,

  IN_RWY_APPROACH = 0xc00 | PlaneStates.IS_INCOMING | PlaneStates.IS_TWR | PlaneStates.IS_TWR_INIT,
  IN_RWY_CLR_LAND = 0xd00 | PlaneStates.IS_INCOMING | PlaneStates.IS_TWR,
  IN_RWY_CLR_LAHS = 0xe00 | PlaneStates.IS_INCOMING | PlaneStates.IS_TWR,
  IN_RWY_CLR_LAPP = 0xf00 | PlaneStates.IS_INCOMING | PlaneStates.IS_TWR,
  IN_RWY_CLR_TAGO = 0x1000 | PlaneStates.IS_INCOMING | PlaneStates.IS_TWR,
  IN_RWY_CLR_SAGO = 0x1100 | PlaneStates.IS_INCOMING | PlaneStates.IS_TWR,
  IN_RWY_GO_AROUND = 0x1200 | PlaneStates.IS_INCOMING | PlaneStates.IS_TWR,
  IN_RWY_WAITING = 0x1300 | PlaneStates.IS_INCOMING | PlaneStates.IS_TWR,
  IN_TAXI_REQUEST = 0x1400 | PlaneStates.IS_INCOMING | PlaneStates.IS_GND | PlaneStates.IS_GND_INIT,
  IN_TAXI_PROGRESS = 0x1500 | PlaneStates.IS_INCOMING | PlaneStates.IS_GND,
  IN_AT_TERMINAL = 0x1600 | PlaneStates.IS_INCOMING | PlaneStates.IS_GND,

  OUT_RAD_LEAVE = 0x0 | PlaneStates.IS_GND | PlaneStates.IS_TWR | PlaneStates.IS_DEP,
  IN_RAD_LEAVE = 0x0 | PlaneStates.IS_INCOMING | PlaneStates.IS_GND | PlaneStates.IS_TWR | PlaneStates.IS_DEP,
}
