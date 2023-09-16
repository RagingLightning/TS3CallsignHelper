using TS3CallsignHelper.API;

namespace TS3CallsignHelper.Modules.FrequencyInformation.Models;
public class FrequencyModel {
  public IViewModel ViewModel { get; }
  public string Frequency { get; }
  public string Name { get; }
  public string Area { get; }

  public FrequencyModel(IViewModel viewModel, AirportFrequency frequency, bool useSayNames) {
    if (frequency.Position == PlayerPosition.Unknown) return;
    ViewModel = viewModel;
    var leading = frequency.Frequency.Split('.')[0].PadLeft(3,'!');
    var trailing = frequency.Frequency.Split('.')[1].PadRight(3,'!');
    Frequency = leading + " . " + trailing;
    Name = useSayNames ? frequency.Sayname : frequency.Writename;
    Area = frequency.ControlArea;
  }
}
