using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TS3CallsignHelper.Wpf.ViewModels;

namespace TS3CallsignHelper.Wpf.Commands;
internal class ResizeViewCommand {

  private CanvasContainerViewModel _viewModel;

  private Point? _origin;
  private Point _size;

  private Canvas _canvas;

  public ResizeViewCommand(CanvasContainerViewModel viewModel, Canvas canvas) {
    _viewModel = viewModel;
    _canvas = canvas;
  }

  public void Start() {
    _origin = Mouse.GetPosition(_canvas);
    _size = new Point(_viewModel.Width, _viewModel.Height);
  }

  public void Step() {
    if (_origin == null) return;
    Point pos = Mouse.GetPosition(_canvas);
    Vector delta = (Vector) (pos - _origin);
    _viewModel.Width = _size.X + delta.X;
    _viewModel.Height = _size.Y + delta.Y;
  }

  public void Stop() {
    _origin = null;
  }
}
