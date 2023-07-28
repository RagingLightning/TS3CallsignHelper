using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS3CallsignHelper.Game.LogParsers;
internal class GameLogReader {

  internal event Action? EndOfLog;

  private Action<string> _parser;

  private string _logPath;
  private AutoResetEvent _logFileChanged;
  private FileSystemWatcher _logFileWatcher;

  private Thread _reader;

  internal GameLogReader(Action<string> parser) {
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

    var logStream = new FileStream(Path.Combine(_logPath, "Player.log"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
    using var logReader = new StreamReader(logStream);
    while (true) {
      var line = logReader.ReadLine();
      if (line != null)
        _parser(line);
      else {
        EndOfLog?.Invoke();
        _logFileChanged.WaitOne(100);
      }
    }
  }


}
