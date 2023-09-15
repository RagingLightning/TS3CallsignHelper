using TS3CallsignHelper.API;
using TS3CallsignHelper.API.Services;
using TS3CallsignHelper.Wpf.Stores;

namespace TS3CallsignHelper.Wpf.Services;
internal class NavigationService : INavigationService {
  private readonly NavigationStore _navigationStore;

  internal NavigationService(NavigationStore navigationStore) {
    _navigationStore = navigationStore;
  }

  public void Navigate(IViewModel viewModel) {
    _navigationStore.RootContent = viewModel;
  }
}
