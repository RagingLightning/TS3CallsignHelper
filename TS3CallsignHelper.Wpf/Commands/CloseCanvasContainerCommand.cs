using TS3CallsignHelper.Wpf.ViewModels;

namespace TS3CallsignHelper.Wpf.Commands;
public class CloseCanvasContainerCommand : CommandBase {

  private MainViewModel _mainModel;
  private CanvasContainerViewModel _viewModel;

  public CloseCanvasContainerCommand(MainViewModel mainModel, CanvasContainerViewModel viewModel) {
    _mainModel = mainModel;
    _viewModel = viewModel;
  }
  public override void Execute(object? parameter) {
    _viewModel.Dispose();
    _mainModel.RemoveView(_viewModel);
  }
}
