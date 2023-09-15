using System;
using TS3CallsignHelper.API;
using TS3CallsignHelper.API.Dependencies;
using TS3CallsignHelper.API.Exceptions;
using TS3CallsignHelper.API.Services;
using TS3CallsignHelper.Wpf.Commands;
using TS3CallsignHelper.Wpf.Services;

namespace TS3CallsignHelper.Wpf.ViewModels;
internal class UpdateNotificationViewModel : IViewModel {
  public override Type Translation => typeof(Translation.UpdateNotificationView);

  public override Type View => typeof(Views.UpdateNotificationView);

  public override double InitialWidth => throw new NotImplementedException();

  public override double InitialHeight => throw new NotImplementedException();

	private readonly IDependencyStore _dependencyStore;
	private readonly INavigationService _navigationService;
	private readonly GuiMessageService _messageService;

	internal UpdateNotificationViewModel(string currentVersion, string newVersion, IDependencyStore dependencyStore) {
		_currentVersion = currentVersion;
		_newVersion = newVersion;
		_dependencyStore = dependencyStore;
		_navigationService = dependencyStore.TryGet<INavigationService>() ?? throw new MissingDependencyException(typeof(INavigationService));
		_messageService = (GuiMessageService) (dependencyStore.TryGet<IGuiMessageService>() ?? throw new MissingDependencyException(typeof(IGuiMessageService)));
	}

	private string _currentVersion;
	public string CurrentVersion {
		get {
			return _currentVersion;
		}
		set {
			_currentVersion = value;
			OnPropertyChanged(nameof(CurrentVersion));
		}
	}

	private string _newVersion;
	public string NewVersion {
		get {
			return _newVersion;
		}
		set {
			_newVersion = value;
			OnPropertyChanged(nameof(NewVersion));
		}
	}

	public CommandBase UpdateCommand => new DownloadUpdateCommand();

  public CommandBase SkipCommand => new NavigateCommand(() => new MainViewModel(_dependencyStore, _messageService), _navigationService);
}
