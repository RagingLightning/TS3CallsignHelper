namespace TS3CallsignHelper.Game.Models;
public class Metar {
  public string? Airport;
  public DateTime? ReportTime;
  public bool Automatic;
  public Wind Winds;
  public int Temperature;
  public int Dewpoint;
  public Pressure Qnh;
}

public class Wind {
  public int Direction;
  public int VariableFrom;
  public int VariableTo;
  public int Magnitude;
  public int Gusts;
}

public class Pressure {
  public char Unit;
  public float Value;
}
