using System;

namespace TS3CallsignHelper.API.Exceptions;

/// <summary>
/// Indicates that the definition of an airline has an incorrect format
/// </summary>
public class AirlineDefinitionFormatException : FormatException {

  /// <summary>
  /// Instantiates a new instance of this exception
  /// </summary>
  /// <param name="message">the message to be passed to the <seealso cref="FormatException"/> constructor</param>
  public AirlineDefinitionFormatException(string? message) : base(message) {
  }
}
