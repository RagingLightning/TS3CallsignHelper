using System.Windows.Controls;
using TS3CallsignHelper.Wpf.Commands;

namespace TS3CallsignHelper.Wpf.ViewModels;

internal class CanvasContainerViewModel : ViewModelBase {
  public override string Name => "Container";

  public ViewModelBase CurrentViewModel { get; }
  public ResizeViewCommand ResizeCommand { get; }
  public CloseCanvasContainerCommand CloseCommand { get; }
  public string ViewName => CurrentViewModel.Name;
  public double Width {
    get => _width; set {
      _width = value;
      OnPropertyChanged(nameof(Width));
    }
  }
  private double _width;

  public double Height {
    get => _height; set {
      _height = value;
      OnPropertyChanged(nameof(Height));
    }
  }
  private double _height;

  public CanvasContainerViewModel(MainViewModel mainModel, ViewModelBase viewModel, Canvas canvas) {
    CurrentViewModel = viewModel;
    CloseCommand = new CloseCanvasContainerCommand(mainModel, this);
    ResizeCommand = new ResizeViewCommand(this, canvas);
    Width = 450;
    Height = 150;
  }
}
