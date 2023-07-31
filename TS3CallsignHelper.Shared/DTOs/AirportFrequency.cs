namespace TS3CallsignHelper.Common.DTOs;
public class AirportFrequency {
  public AirportFrequencyType Type { get; set; }
  public string Frequency { get; set; }
  public string Writename { get; set; }
  public string Sayname { get; set; }
  public string Readback { get; set; }
  public string ControlArea { get; set; }
}

public enum AirportFrequencyType {
  GROUND,
  TOWER,
  DEPARTURE
}
