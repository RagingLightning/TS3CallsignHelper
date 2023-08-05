using System;
namespace TS3CallsignHelper.Game.Exceptions;
public class UnknownFrequencyTypeException : Exception {
  public UnknownFrequencyTypeException(string? message) : base(message) {
  }
}
