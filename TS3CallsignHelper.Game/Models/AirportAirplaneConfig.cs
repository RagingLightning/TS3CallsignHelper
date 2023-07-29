using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using TS3CallsignHelper.Game.Exceptions;
using TS3CallsignHelper.Game.Services;

namespace TS3CallsignHelper.Game.Models;
public partial class AirportAirplaneConfig {
  [GeneratedRegex("^(?<q0>\"?)(?<property>.+?)\\k<q0>,(?<q1>\"?)(?<value>.+?)\\k<q1>,(?<information>.+)$")]
  private static partial Regex Parser();
  private readonly ILogger<AirportAirplaneConfig> _logger;

  public IEnumerable<AirportAirplane> Airplanes => _airplanes.Values;

  private readonly Dictionary<string, AirportAirplane> _airplanes;
  public AirportAirplaneConfig(string configPath, IServiceProvider serviceProvider, InitializationProgressService initializationProgress) {
    _logger = serviceProvider.GetRequiredService<ILogger<AirportAirplaneConfig>>();

    _airplanes = new Dictionary<string, AirportAirplane>();

    initializationProgress.StatusMessage = "Loading airplanes...";
    _logger.LogDebug("Loading airplane set from {Config}", configPath);
    var airplaneCount = new DirectoryInfo(configPath).EnumerateDirectories().Count();
    foreach (var di in new DirectoryInfo(configPath).EnumerateDirectories()) {
      var airplaneConfig = Path.Combine(configPath, di.Name, di.Name + ".csv");
      AirportAirplane airplane = new AirportAirplane();

      _logger.LogTrace("Loading airplane type {AirplaneType} from {Config}", di.Name, airplaneConfig);
      var configFile = File.Open(airplaneConfig, FileMode.Open, FileAccess.Read, FileShare.Read);
      using var reader = new StreamReader(configFile);
      reader.ReadLine(); // first line contains headers
      while (reader.ReadLine() is string line) {
        var groups = Parser().Match(line).Groups;
        if (groups.Count == 1) throw new AirplaneDefinitionFormatException(line);

        airplane.SetProperty(groups["property"].Value, groups["value"].Value);
      }
      _airplanes.Add(airplane.Code, airplane);
      _logger.LogDebug("Added airplane type {@AirplaneType}", airplane);
      initializationProgress.AirplaneProgress += 1.0f / airplaneCount;
    }
  }

  public bool Contains(string code) => _airplanes.ContainsKey(code);

  public bool TryGet(string code, out AirportAirplane airplane) {
    return _airplanes.TryGetValue(code, out airplane);
  }
}

public class AirportAirplane {

  public string Code { get; private set; }
  public string Name { get; private set; }
  public AirplaneWeightClass WeightClass { get; private set; }
  public AirplaneApproachClass ApproachSpeedClass { get; private set; }
  public AirplaneCategory Category { get; private set; }
  public double Length { get; private set; }
  public double LandingSpeed { get; private set; }
  public double ApproachAttitude { get; private set; }
  public double GlideslopeDescentRate { get; private set; }
  public double FlareAltitudeStart { get; private set; }
  public double FlareAltitude { get; private set; }
  public double FlareAttitude { get; private set; }
  public double LandingRate { get; private set; }
  public double TakeoffSpeed { get; private set; }
  public double LandingLength { get; private set; }
  public double TakeoffLength { get; private set; }
  public double RateOfDescent { get; private set; }
  public double RateOfClimb { get; private set; }
  public double MinClimbSpeed { get; private set; }
  public double ClimbSpeed { get; private set; }
  public double MaxClimbSpeed { get; private set; }
  public double RateOfUpperClimb { get; private set; }
  public double MinCruiseSpeed { get; private set; }
  public double CruiseSpeed { get; private set; }
  public double MaxCruiseSpeed { get; private set; }
  public double CruiseDescentRate { get; private set; }
  public double CruiseClimbRate { get; private set; }
  public double CruiseAltitude { get; private set; }

  internal void SetProperty(string key, string value) {
    switch (key) {
      case "icao": Code = value; break;
      case "name": Name = value; break;
      case "weight class":
        switch (value) {
          case "L": WeightClass = AirplaneWeightClass.LIGHT; break;
          case "M": WeightClass = AirplaneWeightClass.MEDIUM; break;
          case "H": WeightClass = AirplaneWeightClass.HEAVY; break;
          case "J": WeightClass = AirplaneWeightClass.SUPER; break;
        }
        break;
      case "approach speed class":
        switch (value) {
          case "A": ApproachSpeedClass = AirplaneApproachClass.A; break;
          case "B": ApproachSpeedClass = AirplaneApproachClass.B; break;
          case "C": ApproachSpeedClass = AirplaneApproachClass.C; break;
          case "D": ApproachSpeedClass = AirplaneApproachClass.D; break;
          case "E": ApproachSpeedClass = AirplaneApproachClass.E; break;
          case "F": ApproachSpeedClass = AirplaneApproachClass.F; break;
          case "G": ApproachSpeedClass = AirplaneApproachClass.G; break;
        }
        break;
      case "category":
        switch (value) {
          case "PROP": Category = AirplaneCategory.PROP; break;
          case "TURBOPROP": Category = AirplaneCategory.TURBOPROP; break;
          case "BUSINESS JET": Category = AirplaneCategory.BUSINESS; break;
          case "REGIONAL JET": Category = AirplaneCategory.REGIONAL; break;
          case "NARROW BODY JET": Category = AirplaneCategory.NARROW_BODY; break;
          case "WIDE BODY JET": Category = AirplaneCategory.WIDE_BODY; break;
        }
        break;
      case "length": Length = double.Parse(value, CultureInfo.InvariantCulture); break;
      case "landing speed": LandingSpeed = double.Parse(value, CultureInfo.InvariantCulture); break;
      case "landing attitude approach": ApproachAttitude = double.Parse(value, CultureInfo.InvariantCulture); break;
      case "glideslope after break decent rate": GlideslopeDescentRate = double.Parse(value, CultureInfo.InvariantCulture); break;
      case "landing attitude break": FlareAltitudeStart = double.Parse(value, CultureInfo.InvariantCulture); break;
      case "flare altitude": FlareAltitude = double.Parse(value, CultureInfo.InvariantCulture); break;
      case "landing attitude": FlareAttitude = double.Parse(value, CultureInfo.InvariantCulture); break;
      case "landing rate": LandingRate = double.Parse(value, CultureInfo.InvariantCulture); break;
      case "takeoff speed": TakeoffSpeed = double.Parse(value, CultureInfo.InvariantCulture); break;
      case "landing length": LandingLength = double.Parse(value, CultureInfo.InvariantCulture); break;
      case "takeoff length": TakeoffLength = double.Parse(value, CultureInfo.InvariantCulture); break;
      case "rate of decent": RateOfDescent = double.Parse(value, CultureInfo.InvariantCulture); break;
      case "rate of climb": RateOfClimb = double.Parse(value, CultureInfo.InvariantCulture); break;
      case "min speed 10k": MinClimbSpeed = double.Parse(value, CultureInfo.InvariantCulture); break;
      case "speed 10k": ClimbSpeed = double.Parse(value, CultureInfo.InvariantCulture); break;
      case "max speed 10k": MaxClimbSpeed = double.Parse(value, CultureInfo.InvariantCulture); break;
      case "rate of climb 10k": RateOfUpperClimb = double.Parse(value, CultureInfo.InvariantCulture); break;
      case "min cruse speed": MinCruiseSpeed = double.Parse(value, CultureInfo.InvariantCulture); break;
      case "cruse speed": CruiseSpeed = double.Parse(value, CultureInfo.InvariantCulture); break;
      case "max cruse speed": MaxCruiseSpeed = double.Parse(value, CultureInfo.InvariantCulture); break;
      case "cruse decent rate": CruiseDescentRate = double.Parse(value, CultureInfo.InvariantCulture); break;
      case "cruse climb rate": CruiseClimbRate = double.Parse(value, CultureInfo.InvariantCulture); break;
      case "optimal altitude": CruiseAltitude = double.Parse(value, CultureInfo.InvariantCulture); break;

    }
  }
}

public enum AirplaneWeightClass {
  LIGHT, MEDIUM, HEAVY, SUPER
}

public enum AirplaneApproachClass {
  A, B, C, D, E, F, G
}

public enum AirplaneCategory {
  PROP, TURBOPROP, BUSINESS, REGIONAL, NARROW_BODY, WIDE_BODY
}