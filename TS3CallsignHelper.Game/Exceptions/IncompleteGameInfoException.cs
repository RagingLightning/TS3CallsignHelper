namespace TS3CallsignHelper.Game.Exceptions;
internal class IncompleteGameInfoException : Exception {
  public IncompleteGameInfoException(string field) : base($"GameInfo.{field} was not set, but is required") {
  }

}
