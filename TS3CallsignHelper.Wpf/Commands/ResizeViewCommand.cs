using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TS3CallsignHelper.Wpf.ViewModels;

namespace TS3CallsignHelper.Wpf.Commands;
public class ResizeViewCommand {

  private CanvasContainerViewModel _viewModel;

  private Point? _origin;
  private Point _size;

  public ResizeViewCommand(CanvasContainerViewModel viewModel) {
    _viewModel = viewModel;
  }

  public void Start() {
    _origin = Mouse.GetPosition(null);
    _size = new Point(_viewModel.Width, _viewModel.Height);
  }

  public void Step() {
    if (_origin == null) return;
    Point pos = Mouse.GetPosition(null);
    Vector delta = (Vector) (pos - _origin);
    _viewModel.Width = _size.X + delta.X;
    _viewModel.Height = _size.Y + delta.Y;
  }

  public void Stop() {
    _origin = null;
  }
}
