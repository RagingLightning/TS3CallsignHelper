using System;
using TS3CallsignHelper.API;

namespace TS3CallsignHelper.Game.Exceptions;

/// <summary>
/// Indicates that the <seealso cref="GameInfo"/> is missing a property that is required
/// </summary>
public class IncompleteGameInfoException : Exception {
  public GameInfo? GameInfo { get; set; }
  public string Property { get; }

  /// <summary>
  /// Instantiates a new instance of this exception
  /// </summary>
  /// <param name="info">the incomplete <seealso cref="GameInfo"/> instance</param>
  /// <param name="property">the property missing</param>
  public IncompleteGameInfoException(GameInfo? info, string property) : base($"GameInfo is missing required property {property}") {
    GameInfo = info;
    Property = property;
  }
}
