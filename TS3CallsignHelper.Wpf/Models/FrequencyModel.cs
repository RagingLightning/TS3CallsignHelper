using TS3CallsignHelper.Common.DTOs;
using TS3CallsignHelper.Wpf.ViewModels;

namespace TS3CallsignHelper.Wpf.Models;
public class FrequencyModel {
  public ViewModelBase ViewModel { get; }
  public string Frequency { get; }
  public string Name { get; }
  public string Area { get; }

  public FrequencyModel(ViewModelBase viewModel, AirportFrequency frequency, bool useSayNames) {
    ViewModel = viewModel;
    var leading = frequency.Frequency.Split('.')[0].PadLeft(3,'!');
    var trailing = frequency.Frequency.Split('.')[1].PadRight(3,'!');
    Frequency = leading + " . " + trailing;
    Name = useSayNames ? frequency.Sayname : frequency.Writename;
    Area = frequency.ControlArea;
  }
}
