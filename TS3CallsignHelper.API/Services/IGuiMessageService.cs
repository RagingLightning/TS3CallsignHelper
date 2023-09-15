using System;
using System.Drawing;

namespace TS3CallsignHelper.API.Services;
public interface IGuiMessageService {
  void ShowInfo(string message, TimeSpan? duration = null);
  void ShowWarning(string message, TimeSpan? duration = null);
  void ShowError(string message, TimeSpan? duration = null);
  void ClearMessage();
}
