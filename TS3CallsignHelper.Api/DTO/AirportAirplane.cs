using System.Globalization;
using System.IO;

namespace TS3CallsignHelper.API;
public class AirportAirplane {
  public string Code { get; private set; } = string.Empty;
  public string Name { get; private set; } = string.Empty;
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

  public void SetProperty(string key, string value) {
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
      default: throw new InvalidDataException(key);
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
