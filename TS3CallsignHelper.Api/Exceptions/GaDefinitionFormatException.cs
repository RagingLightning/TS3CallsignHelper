using System;

namespace TS3CallsignHelper.Game.Exceptions;

/// <summary>
/// Indicates that a general aviation entry has an incorrect format
/// </summary>
public class GaDefinitionFormatException : FormatException {

  /// <summary>
  /// Instantiates a new instance of this exception
  /// </summary>
  /// <param name="message">the message to be passed to the <seealso cref="FormatException"/> constructor</param>
  public GaDefinitionFormatException(string? message) : base(message) {
  }
}
