using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TS3CallsignHelper.Wpf.ViewModels;

namespace TS3CallsignHelper.Wpf.Commands;
internal class MoveViewCommand {

  private CanvasContainerViewModel _viewModel;

  private Point? _origin;
  private Point _pos;

  public MoveViewCommand(CanvasContainerViewModel viewModel) {
    _viewModel = viewModel;
  }

  public void Start() {
    _origin = Mouse.GetPosition(null);
    _pos = new Point(_viewModel.X, _viewModel.Y);
  }

  public void Step() {
    if (_origin == null) return;
    Point pos = Mouse.GetPosition(null);
    Vector delta = (Vector) (pos - _origin);
    _viewModel.X = Math.Max(_pos.X + delta.X, 0);
    _viewModel.Y = Math.Max(_pos.Y + delta.Y, 0);
  }

  public void Stop() {
    _origin = null;
  }
}
