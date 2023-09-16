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
    ShowMessage(message, Brushes.Black, Brushes.OrangeRed, duration ?? TimeSpan.FromSeconds(10));
  }

  public void ShowWarning(string message, TimeSpan? duration = null) {
    Warning = message;
    ShowMessage(message, Brushes.Black, Brushes.Orange, duration ?? TimeSpan.FromSeconds(5));
  }

  public void ShowInfo(string message, TimeSpan? duration = null) {
    ShowMessage(message, Brushes.Black, Brushes.Transparent, duration ?? TimeSpan.FromSeconds(5));
  }

  public void ShowMessage(string message, Brush foreground, Brush background, TimeSpan duration) {
    if (_viewModel is null) {
      MessageQueue = () => ShowMessage(message, foreground, background, duration);
      return;
    }
    _viewModel.StatusFg = foreground;
    _viewModel.StatusBg = background;
    _viewModel.StatusText = message;
    Task.Delay(duration).ContinueWith(t => ClearMessage());
  }

  public void ClearMessage() {
    if (_viewModel is null) {
      MessageQueue = null;
      return;
    }
    _viewModel.StatusText = string.Empty;
    _viewModel.StatusBg = Brushes.Transparent;
    Warning = string.Empty;
    Error = string.Empty;
  }
}
