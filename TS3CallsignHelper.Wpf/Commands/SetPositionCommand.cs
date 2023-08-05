using TS3CallsignHelper.Api;
using TS3CallsignHelper.Api.Stores;
using TS3CallsignHelper.Game.Stores;

namespace TS3CallsignHelper.Wpf.Commands;
internal class SetPositionCommand : CommandBase {
  private IGameStateStore _gameStateStore;
  private PlayerPosition _position;

  public SetPositionCommand(IGameStateStore gameStateStore, PlayerPosition position) {
    _gameStateStore = gameStateStore;
    _position = position;
  }

  public override void Execute(object? parameter) {
    if (parameter is not bool active) return;
    _gameStateStore.SetPlayerPosition(_position, active);
  }
}
