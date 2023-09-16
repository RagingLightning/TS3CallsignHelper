using System;
using System.Threading.Tasks;
using System.Windows.Media;
using TS3CallsignHelper.API.Services;
using TS3CallsignHelper.Wpf.ViewModels;

namespace TS3CallsignHelper.Wpf.Services;
internal class GuiMessageService : IGuiMessageService {
  internal static IGuiMessageService? Instance { get; private set; }
  private MainViewModel? _viewModel;
  private Action? MessageQueue;

  public string Warning { get; private set; } = string.Empty;
  public string Error { get; private set; } = string.Empty;

  public void SetViewModel(MainViewModel viewModel) {
    _viewModel = viewModel;
    MessageQueue?.Invoke();
  }

  public GuiMessageService() { Instance = this; }

  public void ShowError(string message, TimeSpan? duration = null) {
    Error = message;
    ShowMessage(message, Brushes.Red, duration);
  }

  public void ShowWarning(string message, TimeSpan? duration = null) {
    Warning = message;
    ShowMessage(message, Brushes.OrangeRed, duration);
  }

  public void ShowInfo(string message, TimeSpan? duration = null) {
    ShowMessage(message, Brushes.Black, duration);
  }

  public void ShowMessage(string message, Brush brush, TimeSpan? duration = null) {
    if (_viewModel is null) {
      MessageQueue = () => ShowMessage(message, brush, duration);
      return;
    }
    _viewModel.StatusBrush = brush;
    _viewModel.StatusText = message;
    if (duration == null) return;
    Task.Delay(((TimeSpan) duration).Milliseconds).ContinueWith(t => ClearMessage());
  }

  public void ClearMessage() {
    if (_viewModel is null) {
      MessageQueue = null;
      return;
    }
    _viewModel.StatusText = string.Empty;
    Warning = string.Empty;
    Error = string.Empty;
  }
}
