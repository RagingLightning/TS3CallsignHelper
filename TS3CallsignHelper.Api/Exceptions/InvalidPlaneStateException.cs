using System;

namespace TS3CallsignHelper.Api.Exceptions;

/// <summary>
/// Indicates that
/// </summary>
public class InvalidPlaneStateException : Exception {
  public InvalidPlaneStateException(string airplane, PlaneState state) : base($"The assignment of state {state} to {airplane} is not valid!") {
    
  }
}
