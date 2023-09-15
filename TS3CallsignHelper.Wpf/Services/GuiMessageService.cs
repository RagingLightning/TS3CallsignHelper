﻿using System;
using System.Drawing;
using System.Threading.Tasks;
using TS3CallsignHelper.API.Services;
using TS3CallsignHelper.Wpf.ViewModels;

namespace TS3CallsignHelper.Wpf.Services;
internal class GuiMessageService : IGuiMessageService {
  internal MainViewModel? ViewModel { private get; set; }

  public void ShowError(string message, TimeSpan? duration = null) {
    ShowMessage(message, Brushes.Red, duration);
  }

  public void ShowWarning(string message, TimeSpan? duration = null) {
    ShowMessage(message, Brushes.OrangeRed, duration);
  }

  public void ShowInfo(string message, TimeSpan? duration = null) {
    ShowMessage(message, Brushes.Black, duration);
  }

  public void ShowMessage(string message, Brush brush, TimeSpan? duration = null) {
    if (ViewModel == null) return;
    ViewModel.StatusBrush = brush;
    ViewModel.StatusText = message;
    if (duration == null) return;
    Task.Delay(((TimeSpan) duration).Milliseconds).ContinueWith(t => ClearMessage());
  }

  public void ClearMessage() {
    if (ViewModel == null) return;
    ViewModel.StatusText = string.Empty;
  }
}