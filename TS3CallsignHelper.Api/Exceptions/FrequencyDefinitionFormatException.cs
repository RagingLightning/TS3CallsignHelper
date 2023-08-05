using System;

namespace TS3CallsignHelper.Game.Exceptions;

/// <summary>
/// Indicates that the definition of a frequency has an incorrect format
/// </summary>
public class FrequencyDefinitionFormatException : FormatException {

  /// <summary>
  /// Instantiates a new instance of this exception
  /// </summary>
  /// <param name="message">the message to be passed to the <seealso cref="FormatException"/> constructor</param>
  public FrequencyDefinitionFormatException(string? message) : base(message) {
  }
}
