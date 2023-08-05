using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using TS3CallsignHelper.Api.Dependencies;
using TS3CallsignHelper.Wpf.Services;

namespace TS3CallsignHelper.Wpf.Stores;
internal class OptionsStore {
  private ILogger<OptionsStore>? _logger;

  private Entries _entries;
  private string _filePath;
  private bool _skipAutosave = false;

  public OptionsStore(string path, IDependencyStore dependencyStore) {
    _logger = dependencyStore.TryGet<LoggerService>()?.GetLogger<OptionsStore>();
    _filePath = path;

    if (File.Exists(_filePath)) {
      _entries = JsonConvert.DeserializeObject<Entries>(File.ReadAllText(_filePath));
    }
    else {
      _entries = new();
    }
  }

  public bool SkipAutosave {
    set { _skipAutosave = value; if (!value) Save(); }
  }

  public int BackupLogFiles {
    get => _entries.BackupLogFiles;
    set { _entries.BackupLogFiles = value; if (!_skipAutosave) Save(); }
  }

  internal void Save() {
    File.WriteAllText(_filePath, JsonConvert.SerializeObject(_entries, new JsonSerializerSettings { Formatting = Formatting.Indented }));
  }

  internal void ApplyDefaults() {
    _entries.ApplyDefaults();
    Save();
  }

  internal class Entries {
    public static readonly Dictionary<string, object> DEFAULTS = new();

    public int BackupLogFiles = (int) DEFAULTS[nameof(BackupLogFiles)];

    static Entries() {
      DEFAULTS.Add(nameof(BackupLogFiles), 3);
    }

    internal void ApplyDefaults() {
      BackupLogFiles = (int) DEFAULTS[nameof(BackupLogFiles)];
    }
  }

}

