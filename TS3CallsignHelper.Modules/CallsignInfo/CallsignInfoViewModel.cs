using Microsoft.Extensions.Logging;
using System;
using TS3CallsignHelper.API;
using TS3CallsignHelper.API.Dependencies;
using TS3CallsignHelper.API.Events;
using TS3CallsignHelper.API.Exceptions;
using TS3CallsignHelper.API.Logging;
using TS3CallsignHelper.API.Stores;

namespace TS3CallsignHelper.Modules.CallsignInfo;
class CallsignInfoViewModel : IViewModel
{
    private readonly ILogger? _logger;
    private readonly IGameStateStore _gameStateStore;
    private readonly IAirportDataStore _airportDataStore;
    public override Type Translation => typeof(Translation.CallsignInfoModule);
    public override Type View => typeof(CallsignInfoView);
    public override double InitialWidth => 450;
    public override double InitialHeight => 150;

    public CallsignInfoViewModel(IDependencyStore dependencyStore)
    {
        _logger = dependencyStore.TryGet<ILoggerService>()?.GetLogger<CallsignInfoViewModel>();

        _logger?.LogInformation("Initializing");
        _gameStateStore = dependencyStore.TryGet<IGameStateStore>() ?? throw new MissingDependencyException(typeof(IGameStateStore));
        _airportDataStore = dependencyStore.TryGet<IAirportDataStore>() ?? throw new MissingDependencyException(typeof(IAirportDataStore));

        _logger?.LogDebug("Registering event handlers");
        _gameStateStore.CurrentAirplaneChanged += OnCurrentAirplaneChanged;
        _logger?.LogTrace("{Method} registered", nameof(OnCurrentAirplaneChanged));

        _sayname = string.Empty;
        _writename = string.Empty;
        _weightClass = string.Empty;
    }

    public override void Dispose()
    {
        _logger?.LogDebug("Unegistering event handlers");
        _gameStateStore.CurrentAirplaneChanged -= OnCurrentAirplaneChanged;
        _logger?.LogTrace("{Method} unregistered", nameof(OnCurrentAirplaneChanged));
    }

    private void OnCurrentAirplaneChanged(AirplaneChangedEventArgs args)
    {
        _logger?.LogDebug("Received CurrentAirplaneChanged event");
        if (args.Callsign == string.Empty)
        {
            Writename = string.Empty;
            Sayname = string.Empty;
            WeightClass = string.Empty;
            return;
        }
        if (_airportDataStore.Schedule?.TryGetValue(args.Callsign, out var schedule) == true)
        {
            Writename = schedule.Airline?.Code + schedule.FlightNumber;
            Sayname = schedule.Airline?.Callsign ?? string.Empty;
            WeightClass = schedule.AirplaneType?.WeightClass.ToString() ?? string.Empty;
        }
        else if (_airportDataStore.GaPlanes?.TryGetValue(args.Callsign, out var ga) == true)
        {
            Writename = ga.Writename;
            Sayname = ga.FormatSayname();
            WeightClass = ga.AirplaneType.WeightClass.ToString();
        }
        _logger?.LogDebug("New callsign info: {Callsign} / {WeightClass} for {Airplane}", Sayname, WeightClass, Writename);
    }

    private string _sayname;
    public string Sayname
    {
        get
        {
            return _sayname;
        }
        set
        {
            _sayname = value;
            OnPropertyChanged(nameof(Sayname));
        }
    }

    private string _writename;
    public string Writename
    {
        get
        {
            return _writename;
        }
        set
        {
            _writename = value;
            OnPropertyChanged(nameof(Writename));
        }
    }

    private string _weightClass;
    public string WeightClass
    {
        get
        {
            return _weightClass;
        }
        set
        {
            _weightClass = value;
            OnPropertyChanged(nameof(WeightClass));
        }
    }

}
