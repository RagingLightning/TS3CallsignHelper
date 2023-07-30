using System;
using TS3CallsignHelper.Wpf.Commands;

namespace TS3CallsignHelper.Wpf.ViewModels;

public class CanvasContainerViewModel : ViewModelBase {
  public override Type Translation => typeof(Translation.CanvasContainerView);

  public ViewModelBase CurrentViewModel { get; }
  public ResizeViewCommand ResizeCommand { get; }
  public MoveViewCommand MoveCommand { get; }
  public CommandBase CloseCommand { get; }
  public CommandBase DecreaseScaleCommand => new CallFunctionCommand(() => {
    CurrentViewModel.Scale -= 0.1;
    OnPropertyChanged(nameof(ViewScale));
    });
  public CommandBase IncreaseScaleCommand => new CallFunctionCommand(() => {
    CurrentViewModel.Scale += 0.1;
    OnPropertyChanged(nameof(ViewScale));
  });
  public CommandBase DecreaseZIndexCommand => new CallFunctionCommand(() => ZIndex = Math.Min(0, ZIndex-1));
  public CommandBase IncreaseZIndexCommand => new CallFunctionCommand(() => ZIndex += 1);
  public string ViewName {
    get {
      var name = CurrentViewModel.GetType().Name;
      return $"{name.Remove(name.Length-5)}:Name";
    }
  }
  public double X {
    get => _x; set {
      _x = value;
      OnPropertyChanged(nameof(X));
    }
  }
  private double _x;

  public double Y {
    get => _y; set {
      _y = value;
      OnPropertyChanged(nameof(Y));
    }
  }
  private double _y;
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

  private int _zindex;
  public int ZIndex {
    get {
      return _zindex;
    }
    set {
      _zindex = value;
      OnPropertyChanged(nameof(ZIndex));
    }
  }

  public string ViewScale => $"{CurrentViewModel.Scale*100:.}%";

  public CanvasContainerViewModel(MainViewModel mainModel, ViewModelBase viewModel) {
    CurrentViewModel = viewModel;
    CloseCommand = new CloseCanvasContainerCommand(mainModel, this);
    ResizeCommand = new ResizeViewCommand(this);
    MoveCommand = new MoveViewCommand(this);
    Width = 450;
    Height = 150;
    X = 0;
    Y = 0;
  }

  public override void Dispose() {
    CurrentViewModel.Dispose();
  }
}
