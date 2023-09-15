using System.Diagnostics;
using System.Windows;

namespace TS3CallsignHelper.Wpf.Commands;
internal class DownloadUpdateCommand : CommandBase {
  public override void Execute(object? parameter) {
    Process.Start(new ProcessStartInfo {
      FileName = $"https://www.github.com/RagingLightning/TS3CallsignHelper/releases/latest/",
      UseShellExecute = true
    });
    Application.Current.Shutdown();
  }
}
