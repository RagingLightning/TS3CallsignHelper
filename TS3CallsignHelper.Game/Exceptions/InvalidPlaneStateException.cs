using TS3CallsignHelper.Game.Enums;

namespace TS3CallsignHelper.Game.Exceptions;
public class InvalidPlaneStateException : Exception {
  public InvalidPlaneStateException(string airplane, PlaneState state) : base($"The assignment of state {state} to {airplane} is not valid!") {
    
  }
}
