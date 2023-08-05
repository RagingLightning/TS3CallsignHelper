using System;
namespace TS3CallsignHelper.Game.Exceptions;
public class ScheduleDefinitionFormatException : FormatException {
  public ScheduleDefinitionFormatException(string? message) : base(message) {
  }
}
