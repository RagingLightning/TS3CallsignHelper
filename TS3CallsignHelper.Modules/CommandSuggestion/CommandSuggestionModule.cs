using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TS3_Callsign_Helper.Lib;
using TS3CallsignHelper.API;
using TS3CallsignHelper.API.Dependencies;
using TS3CallsignHelper.API.Exceptions;

namespace TS3CallsignHelper.Modules.CommandSuggestion;

[Export(typeof(ICallsignHelperModule))]
[ExportMetadata("Name", "Command Suggestion View")]
public class CommandSuggestionModule : ICallsignHelperModule {
  private ILogger<CommandSuggestionModule>? _logger;
  private static HotKeyHelper? _hotkeyHelper;
  private uint hk_CopySegment;

  internal static readonly Queue<string> CopySegments = new();

  [ImportingConstructor]
  public CommandSuggestionModule() {
  }

  public void Load(IDependencyStore dependencyStore) {
    var viewStore = dependencyStore.TryGet<IViewStore>() ?? throw new MissingDependencyException(typeof(IViewStore));
    viewStore.Register<CommandSuggestionView, CommandSuggestionViewModel, Translation.CommandSuggestionModule>();

    if (_hotkeyHelper is null && dependencyStore.TryGet<Window>() is Window window) {
      _hotkeyHelper = new HotKeyHelper(window, HandleHotkey);
      hk_CopySegment = _hotkeyHelper.ListenForHotKey(Hotkeys.V, HotKeyModifiers.Control);
    }
  }

  private void HandleHotkey(int hotkey) {
    if (hotkey != hk_CopySegment) return;
    
    if (CopySegments.TryPeek(out string? s) && s is string segment) {
      try {
        Clipboard.SetText(segment);
        _logger?.LogDebug("Copied {Segment}", segment);
        CopySegments.Dequeue();
      }
      catch (Exception ex) {
        _logger?.LogError(ex, "Failed to access clipboard");
      }
    }

    _hotkeyHelper?.SendBlockedKey();
  }
}
