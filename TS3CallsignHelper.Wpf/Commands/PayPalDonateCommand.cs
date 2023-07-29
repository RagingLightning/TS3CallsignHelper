using System.Diagnostics;

namespace TS3CallsignHelper.Wpf.Commands;
internal class PayPalDonateCommand : CommandBase {

  private const string _url = "https://www.paypal.com/donate/?hosted_button_id=DM5BH83828KCL";

  public override void Execute(object? parameter) {
    Process.Start(new ProcessStartInfo() { FileName = _url, UseShellExecute = true });
  }
}
