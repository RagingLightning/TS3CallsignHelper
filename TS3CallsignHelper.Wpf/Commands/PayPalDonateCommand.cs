using System.Diagnostics;

namespace TS3CallsignHelper.Wpf.Commands;
internal class PayPalDonateCommand : CommandBase {

  private readonly string _url;

  public PayPalDonateCommand(string url) {
    _url = url;
  }

  public override void Execute(object? parameter) {
    Process.Start(new ProcessStartInfo() { FileName = _url, UseShellExecute = true });
  }
}
