using Microsoft.Extensions.DependencyInjection;
using System;
using TS3CallsignHelper.Game.DTOs;
using TS3CallsignHelper.Game.Services;
using TS3CallsignHelper.Wpf.Stores;

namespace TS3CallsignHelper.Wpf.ViewModels;
internal class InitializationViewModel : ViewModelBase {
  public override string Name => "Initialization";
  public override Type Translation => throw new NotImplementedException();
  private readonly IServiceProvider _serviceProvider;
	private readonly NavigationStore _navigationStore;
	private readonly InitializationProgressService _initializationProgressService;

	public InitializationViewModel(IServiceProvider serviceProvider) {
		_serviceProvider = serviceProvider;
		_navigationStore = serviceProvider.GetRequiredService<NavigationStore>();
		_initializationProgressService = serviceProvider.GetRequiredService<InitializationProgressService>();
		Progress = "0%";
		Status = "Loading...";

		_initializationProgressService.ProgressChanged += OnProgressChanged;
	}

  public override void Dispose() {
		_initializationProgressService.ProgressChanged -= OnProgressChanged;
	}

	private void OnProgressChanged(Progress progress) {
		if (progress.Completed) {
			_navigationStore.RootContent = new MainViewModel(_serviceProvider);
			Dispose();
		}

		Progress = $"{progress.Value * 100:0.00}%";
		Status = progress.Status;
		Details = progress.Details;
		
	}

	private string _progress;
	public string Progress {
		get {
			return _progress;
		}
		set {
			_progress = value;
			OnPropertyChanged(nameof(Progress));
		}
	}

	private string _status;
	public string Status {
		get {
			return _status;
		}
		set {
			_status = value;
			OnPropertyChanged(nameof(Status));
		}
	}

	private string _details;
	public string Details {
		get {
			return _details;
		}
		set {
			_details = value;
			OnPropertyChanged(nameof(Details));
		}
	}
}
