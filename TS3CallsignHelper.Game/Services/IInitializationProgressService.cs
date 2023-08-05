using TS3CallsignHelper.Game.DTO;

namespace TS3CallsignHelper.Game.Services;
public interface IInitializationProgressService {
  float AirlineProgess { get; set; }
  float AirplaneProgress { get; set; }
  bool Completed { get; set; }
  string Details { get; set; }
  float FrequencyProgress { get; set; }
  float GaProgress { get; set; }
  float LogFileProgress { get; set; }
  float ScheduleProgress { get; set; }
  string StatusMessage { get; set; }

  event Action<Progress>? ProgressChanged;
}