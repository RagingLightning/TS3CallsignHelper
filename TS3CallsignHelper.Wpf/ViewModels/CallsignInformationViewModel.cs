using Accessibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using TS3CallsignHelper.Game.Models;
using TS3CallsignHelper.Game.Stores;

namespace TS3CallsignHelper.Wpf.ViewModels;
class CallsignInformationViewModel : ViewModelBase {
  public override string Name => "Callsign Information";
  public override Type Translation => throw new NotImplementedException();
  private readonly ILogger _logger;

  private readonly GameStateStore _gameStateStore;

  public CallsignInformationViewModel(IServiceProvider serviceProvider) {
    _logger = serviceProvider.GetRequiredService<ILogger<CallsignInformationViewModel>>();

    _logger.LogInformation("Initializing");
    _gameStateStore = serviceProvider.GetRequiredService<GameStateStore>();

    _logger.LogDebug("Registering event handlers");
    _gameStateStore.CurrentAirplaneChanged += OnCurrentAirplaneChanged;
    _logger.LogTrace("{$method} registered", nameof(OnCurrentAirplaneChanged));

    _sayname = string.Empty;
    _writename = string.Empty;
    _weightClass = string.Empty;
  }

  public override void Dispose() {
    _logger.LogDebug("Unegistering event handlers");
    _gameStateStore.CurrentAirplaneChanged -= OnCurrentAirplaneChanged;
    _logger.LogTrace("{$method} unregistered", nameof(OnCurrentAirplaneChanged));
  }

  private void OnCurrentAirplaneChanged() {
    _logger.LogDebug("Received CurrentAirplaneChanged event");
    if (_gameStateStore.CurrentPlaneIsAirline) {
      var schedule = _gameStateStore.GetCurrentScheduleEntry();
      Writename = schedule.Airline+schedule.FlightNumber;
      Sayname = _gameStateStore.GetAirline(schedule.Airline).Callsign;
      WeightClass = _gameStateStore.GetAirplaneType(schedule.AirplaneType).WeightClass.ToString();
    }
    else {
      var ga = _gameStateStore.GetCurrentGaPlane();
      Writename = ga.WriteName;
      Sayname = AirportGa.FormatSayName(ga.SayName);
      WeightClass = _gameStateStore.GetAirplaneType(ga.AirplaneType).WeightClass.ToString();
    }
    _logger.LogDebug("New callsign info: {Callsign} / {WeightClass} for {Airplane}", Sayname, WeightClass, Writename);
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
