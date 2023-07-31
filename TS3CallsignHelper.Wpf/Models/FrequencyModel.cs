using TS3CallsignHelper.Common.DTOs;

namespace TS3CallsignHelper.Wpf.Models;
public class FrequencyModel {
  public string Frequency { get; }
  public string Name { get; }
  public string Area { get; }

  public FrequencyModel(string frequency, string name, string area) {
    Frequency = frequency;
    Name = name;
    Area = area;
  }

  public FrequencyModel(AirportFrequency frequency, bool useSayNames) {
    Frequency = frequency.Frequency;
    Name = useSayNames ? frequency.Sayname : frequency.Writename;
    Area = frequency.ControlArea;
  }
}
