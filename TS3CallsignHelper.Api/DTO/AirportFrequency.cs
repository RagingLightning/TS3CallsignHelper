namespace TS3CallsignHelper.Api;
public class AirportFrequency {
  public AirportFrequencyType Type { get; set; }
  public string Frequency { get; set; } = string.Empty;
  public string Writename { get; set; } = string.Empty;
  public string Sayname { get; set; } = string.Empty;
  public string Readback { get; set; } = string.Empty;
  public string ControlArea { get; set; } = string.Empty;
}

public enum AirportFrequencyType {
  GROUND,
  TOWER,
  DEPARTURE
}
