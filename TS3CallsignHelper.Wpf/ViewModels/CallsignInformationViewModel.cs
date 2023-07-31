using Microsoft.Extensions.Logging;
using System;
using TS3CallsignHelper.Common.DTOs;
using TS3CallsignHelper.Common.Services;
using TS3CallsignHelper.Game.Models;
using TS3CallsignHelper.Game.Stores;

namespace TS3CallsignHelper.Wpf.ViewModels;
class CallsignInformationViewModel : ViewModelBase {
  private readonly ILogger? _logger;
  private readonly GameStateStore _gameStateStore;
  public override Type Translation => throw new NotImplementedException();
  public override double InitialWidth => 450;
  public override double InitialHeight => 150;

  public CallsignInformationViewModel(GameStateStore gameStateStore) {
    _logger = LoggingService.GetLogger<CallsignInformationViewModel>();

    _logger?.LogInformation("Initializing");
    _gameStateStore = gameStateStore;

    _logger?.LogDebug("Registering event handlers");
    _gameStateStore.CurrentAirplaneChanged += OnCurrentAirplaneChanged;
    _logger?.LogTrace("{Method} registered", nameof(OnCurrentAirplaneChanged));

    _sayname = string.Empty;
    _writename = string.Empty;
    _weightClass = string.Empty;
  }

  public override void Dispose() {
    _logger?.LogDebug("Unegistering event handlers");
    _gameStateStore.CurrentAirplaneChanged -= OnCurrentAirplaneChanged;
    _logger?.LogTrace("{Method} unregistered", nameof(OnCurrentAirplaneChanged));
  }

  private void OnCurrentAirplaneChanged(string callsign) {
    _logger?.LogDebug("Received CurrentAirplaneChanged event");
    if (_gameStateStore.ScheduleConfig?.TryGet(callsign, out var schedule) == true) {
      Writename = schedule.Airline+schedule.FlightNumber;
      Sayname = schedule.Airline.Callsign;
      WeightClass = schedule.AirplaneType.WeightClass.ToString();
    }
    else if (_gameStateStore.GaConfig?.TryGet(callsign, out var ga) == true) {
      Writename = ga.Writename;
      Sayname = ga.FormatSayname();
      WeightClass = ga.AirplaneType.WeightClass.ToString();
    }
    _logger?.LogDebug("New callsign info: {Callsign} / {WeightClass} for {Airplane}", Sayname, WeightClass, Writename);
  }

  private string _sayname;
  public string Sayname {
    get {
      return _sayname;
    }
    set {
      _sayname = value;
      OnPropertyChanged(nameof(Sayname));
    }
  }

  private string _writename;
  public string Writename {
    get {
      return _writename;
    }
    set {
      _writename = value;
      OnPropertyChanged(nameof(Writename));
    }
  }

  private string _weightClass;
  public string WeightClass {
    get {
      return _weightClass;
    }
    set {
      _weightClass = value;
      OnPropertyChanged(nameof(WeightClass));
    }
  }

}
