using TS3CallsignHelper.Common.DTOs;

namespace TS3CallsignHelper.Game.Services;
public class InitializationProgressService {

  public event Action<Progress>? ProgressChanged;

  public string StatusMessage {
    get => _statusMessage;
    set {
      _statusMessage = value;
      OnProgressChanged();
    }
  }

  public string Details {
    get => _details;
    set {
      _details = value;
      OnProgressChanged();
    }
  }

  public bool Completed {
    get => _completed;
    set {
      _completed = value;
      OnProgressChanged();
    }
  }

  public float LogFileProgress {
    get => _logFileProgress;
    set {
      _logFileProgress = value;
      OnProgressChanged();
    }
  }

  public float AirlineProgess {
    get => _airlineProgress;
    set {
      _airlineProgress = value;
      OnProgressChanged();
    }
  }

  public float FrequencyProgress {
    get => _frequencyProgress;
    set {
      _frequencyProgress = value;
      OnProgressChanged();
    }
  }

  public float GaProgress {
    get => _gaProgress;
    set {
      _gaProgress = value;
      OnProgressChanged();
    }
  }

  public float ScheduleProgress {
    get => _scheduleProgress;
    set {
      _scheduleProgress = value;
      OnProgressChanged();
    }
  }

  public float AirplaneProgress {
    get => _airplaneProgress;
    set {
      _airplaneProgress = value;
      OnProgressChanged();
    }
  }

  private string _statusMessage;
  private string _details;
  private bool _completed;
  private float _logFileProgress;
  private float _airlineProgress;
  private float _frequencyProgress;
  private float _gaProgress;
  private float _scheduleProgress;
  private float _airplaneProgress;

  private void OnProgressChanged() {
    ProgressChanged?.Invoke(new Progress { Status = _statusMessage, Details = _details, Value = (_logFileProgress + _airlineProgress + _frequencyProgress + _gaProgress + _scheduleProgress + _airplaneProgress) / 6, Completed = _completed });
  }
}
