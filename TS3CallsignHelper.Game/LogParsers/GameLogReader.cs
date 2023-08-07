using TS3CallsignHelper.Game.Services;

namespace TS3CallsignHelper.Game.LogParsers;
internal class GameLogReader {
  private readonly IInitializationProgressService _initializationProgress;

  internal event Action? EndOfLog;

  private Action<string> _parser;

  private string _logPath;
  private AutoResetEvent _logFileChanged;
  private FileSystemWatcher _logFileWatcher;

  private Thread _reader;

  internal GameLogReader(Action<string> parser, IInitializationProgressService initializationProgress) {
    _initializationProgress = initializationProgress;

    _parser = parser;
    _logFileChanged = new AutoResetEvent(false);
  }

  internal void Init(string logPath) {
    _logPath = logPath;
    _logFileWatcher = new FileSystemWatcher(logPath) {
      Filter = "Player.log",
      EnableRaisingEvents = true
    };
    _logFileWatcher.Changed += (s, e) => _logFileChanged.Set();

    _reader = new Thread(Run);
  }

  internal void Start() => _reader.Start();

  private void Run() {
    Thread.CurrentThread.Name = "Log Parsing Thread";
    Thread.CurrentThread.IsBackground = true;
    _initializationProgress.StatusMessage = "State_LogFile";

    var logStream = new FileStream(Path.Combine(_logPath, "Player.log"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
    using var logReader = new StreamReader(logStream);
    while (true) {
      var line = logReader.ReadLine();
      if (line != null) {
        _parser(line);
        if (!_initializationProgress.Completed)
          _initializationProgress.LogFileProgress = ((float) logStream.Position) / logStream.Length;
      }
      else {
        EndOfLog?.Invoke();
        _logFileChanged.WaitOne(100);
      }
    }
  }


}
