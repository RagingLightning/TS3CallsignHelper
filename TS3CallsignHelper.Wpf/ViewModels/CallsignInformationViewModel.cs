using Accessibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Numerics;
using TS3CallsignHelper.Game.Models;
using TS3CallsignHelper.Game.Stores;

namespace TS3CallsignHelper.Wpf.ViewModels;
class CallsignInformationViewModel : ViewModelBase {
  public override Type Translation => throw new NotImplementedException();
  public override double InitialWidth => 450;
  public override double InitialHeight => 150;
  private readonly ILogger _logger;

  private readonly GameStateStore _gameStateStore;

  public CallsignInformationViewModel(GameStateStore gameStateStore, IServiceProvider serviceProvider) {
    _logger = serviceProvider.GetRequiredService<ILogger<CallsignInformationViewModel>>();

    _logger.LogInformation("Initializing");
    _gameStateStore = gameStateStore;

    _logger.LogDebug("Registering event handlers");
    _gameStateStore.CurrentAirplaneChanged += OnCurrentAirplaneChanged;
    _logger.LogTrace("{Method} registered", nameof(OnCurrentAirplaneChanged));

    _sayname = string.Empty;
    _writename = string.Empty;
    _weightClass = string.Empty;
  }

  public override void Dispose() {
    _logger.LogDebug("Unegistering event handlers");
    _gameStateStore.CurrentAirplaneChanged -= OnCurrentAirplaneChanged;
    _logger.LogTrace("{Method} unregistered", nameof(OnCurrentAirplaneChanged));
  }

  private void OnCurrentAirplaneChanged(string callsign) {
    _logger.LogDebug("Received CurrentAirplaneChanged event");
    if (_gameStateStore.CurrentPlaneIsAirline) {
      var schedule = _gameStateStore.Schedule?[callsign];
      Writename = schedule.Airline+schedule.FlightNumber;
      Sayname = _gameStateStore.Airlines[schedule.Airline].Callsign;
      WeightClass = _gameStateStore.Airplanes[schedule.AirplaneType].WeightClass.ToString();
    }
    else {
      var ga = _gameStateStore.GaPlanes[callsign];
      Writename = ga.WriteName;
      Sayname = AirportGa.FormatSayName(ga.SayName);
      WeightClass = _gameStateStore.Airplanes[ga.AirplaneType].WeightClass.ToString();
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
