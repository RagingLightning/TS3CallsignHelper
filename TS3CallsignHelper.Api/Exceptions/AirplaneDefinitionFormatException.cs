using System;
namespace TS3CallsignHelper.Api.Exceptions;

/// <summary>
/// Indicates that the definition of an airplane type has an incorrect format
/// </summary>
public class AirplaneDefinitionFormatException : FormatException {

  /// <summary>
  /// Instantiates a new instance of this exception
  /// </summary>
  /// <param name="message">the message to be passed to the <seealso cref="FormatException"/> constructor</param>
  public AirplaneDefinitionFormatException(string? message) : base(message) {
  }
}
