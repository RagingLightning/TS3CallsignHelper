using TS3CallsignHelper.Game.Models;
using TS3CallsignHelper.Game.Stores;

namespace TS3CallsignHelper.Wpf.ViewModels;
class CallsignInformationViewModel : ViewModelBase {
  public override string Name => "Callsign Information";

  private readonly GameStateStore _gameStateStore;

  public CallsignInformationViewModel(GameStateStore gameStateStore) {
    _gameStateStore = gameStateStore;
    gameStateStore.CurrentAirplaneChanged += OnCurrentAirplaneChanged;
  }

  public override void Dispose() {
    base.Dispose();
    
    _gameStateStore.CurrentAirplaneChanged -= OnCurrentAirplaneChanged;
  }

  private void OnCurrentAirplaneChanged() {
    if (_gameStateStore.CurrentAirline == "GA") {
      var ga = _gameStateStore.GetCurrentGaPlane();
      AirlineCode = "GA ";
      FlightNumber = ga.WriteName;
      Callsign = AirportGa.FormatSayName(ga.SayName);
    }
    else {
      var airline = _gameStateStore.GetCurrentAirline();
      AirlineCode = airline.Code;
      FlightNumber = _gameStateStore.CurrentFlight;
      Callsign = airline.Callsign;
    }
  }

  private string _callsign;
  public string Callsign {
    get {
      return _callsign;
    }
    set {
      _callsign = value;
      OnPropertyChanged(nameof(Callsign));
    }
  }

  private string _airlineCode;
  public string AirlineCode {
    get {
      return _airlineCode;
    }
    set {
      _airlineCode = value;
      OnPropertyChanged(nameof(AirlineCode));
    }
  }

  private string _flightNumber;
  public string FlightNumber {
    get {
      return _flightNumber;
    }
    set {
      _flightNumber = value;
      OnPropertyChanged(nameof(FlightNumber));
    }
  }

}
