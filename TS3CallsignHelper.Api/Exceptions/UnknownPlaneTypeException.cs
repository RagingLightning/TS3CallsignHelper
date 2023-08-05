using System;

namespace TS3CallsignHelper.Game.Exceptions;
public class UnknownPlaneTypeException : Exception {
  public UnknownPlaneTypeException(string? message) : base(message) {
  }
}
