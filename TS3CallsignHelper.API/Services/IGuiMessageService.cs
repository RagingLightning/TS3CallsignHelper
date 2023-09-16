using System;

namespace TS3CallsignHelper.API.Services;
public interface IGuiMessageService {
  public string Warning { get; }
  public string Error { get; }
  void ShowInfo(string message, TimeSpan? duration = null);
  void ShowWarning(string message, TimeSpan? duration = null);
  void ShowError(string message, TimeSpan? duration = null);
  void ClearMessage();
}
