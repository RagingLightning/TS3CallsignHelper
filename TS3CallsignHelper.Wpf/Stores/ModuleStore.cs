using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Windows;
using TS3CallsignHelper.Api;
using TS3CallsignHelper.Api.Dependencies;
using TS3CallsignHelper.Api.Logging;
using TS3CallsignHelper.Modules.CallsignInfo;
using TS3CallsignHelper.Wpf.Services;

namespace TS3CallsignHelper.Wpf.Stores;
internal class ModuleStore {
  private IDependencyStore _dependencyStore;
  private ILogger<ModuleStore>? _logger;

  private bool _active;

  public ModuleStore(IDependencyStore dependencyStore) {
    _dependencyStore = dependencyStore;
    _logger = dependencyStore.TryGet<ILoggerService>()?.GetLogger<ModuleStore>();

    var catalog = new AggregateCatalog();

    //Adds all the parts found in the same assembly as the Program class
    catalog.Catalogs.Add(new AssemblyCatalog(GetType().Assembly));
    catalog.Catalogs.Add(new AssemblyCatalog(typeof(CallsignInfoModule).Assembly));
    if (Directory.Exists("Modules"))
      catalog.Catalogs.Add(new DirectoryCatalog("Modules"));

    //Create the CompositionContainer with the parts in the catalog
    var container = new CompositionContainer(catalog);

    //Fill the imports of this object
    container.ComposeParts(this);
  }

  public void LoadModules() {
    foreach (var module in modules) {
      _logger?.LogInformation("Loading {Module}", module.Metadata.Name);
      try {
        module.Value.Load(_dependencyStore);
      }
      catch (Exception ex) {
        _logger?.LogWarning(ex, "Failed to load {Module}", module.Metadata.Name);
      }
    }
  }

  [ImportMany(typeof(ICallsignHelperModule))]
  IEnumerable<Lazy<ICallsignHelperModule, ICallsignHelperModuleData>> modules;
}
