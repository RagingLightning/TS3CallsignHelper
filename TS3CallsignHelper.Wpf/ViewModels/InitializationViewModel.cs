using Microsoft.Extensions.Logging;
using System;
using TS3CallsignHelper.Common.DTOs;
using TS3CallsignHelper.Common.Services;
using TS3CallsignHelper.Game.Services;
using TS3CallsignHelper.Game.Stores;
using TS3CallsignHelper.Wpf.Stores;

namespace TS3CallsignHelper.Wpf.ViewModels;
internal class InitializationViewModel : ViewModelBase {
	private readonly ILogger<InitializationViewModel>? _logger;
	private readonly NavigationStore _navigationStore;
	private readonly InitializationProgressService _initializationProgressService;
	private readonly GameStateStore _gameStateStore;
  public override Type Translation => throw new NotImplementedException();
  public override double InitialWidth => throw new NotImplementedException();
  public override double InitialHeight => throw new NotImplementedException();

	public InitializationViewModel(NavigationStore navigationStore, InitializationProgressService initializationProgressService, GameStateStore gameStateStore) {
		_logger = LoggingService.GetLogger<InitializationViewModel>();
		_navigationStore = navigationStore;
		_initializationProgressService = initializationProgressService;
		_gameStateStore = gameStateStore;

		_logger?.LogInformation("{LoadingState}", "Loading...");
		Progress = "0%";
		Status = "Loading...";

		_initializationProgressService.ProgressChanged += OnProgressChanged;
	}

  public override void Dispose() {
		_initializationProgressService.ProgressChanged -= OnProgressChanged;
	}

	private void OnProgressChanged(Progress progress) {
		if (progress.Completed) {
			_logger?.LogInformation("{LoadingState}", "Loading complete");
			_navigationStore.RootContent = new MainViewModel(_gameStateStore, _navigationStore);
			Dispose();
		}

		Progress = $"{progress.Value * 100:0.00}%";
		if (progress.Status != Status)
			_logger?.LogInformation("{LoadingState}", progress.Status);
		Status = progress.Status;
		Details = progress.Details;
		
	}

	private string? _progress;
	public string Progress {
		get {
			return _progress ?? string.Empty;
		}
		set {
			_progress = value;
			OnPropertyChanged(nameof(Progress));
		}
	}

	private string? _status;
	public string Status {
		get {
			return _status ?? string.Empty;
		}
		set {
			_status = value;
			OnPropertyChanged(nameof(Status));
		}
	}

	private string? _details;
	public string Details {
		get {
			return _details ?? string.Empty;
		}
		set {
			_details = value;
			OnPropertyChanged(nameof(Details));
		}
	}
}
