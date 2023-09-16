using System.IO;
using System.Net;

namespace TS3CallsignHelper.Wpf.Services;
internal class UpdateCheckerService {
  public static string VERSION = "2.0.1";

  public static string? HasUpdate() {
    WebRequest UpdateRequest = WebRequest.Create("https://raw.githubusercontent.com/RagingLightning/TS3CallsignHelper/deploy/version.dat");
    string UpdateResponse = new StreamReader(UpdateRequest.GetResponse().GetResponseStream()).ReadToEnd();
    string[] NewVersion = UpdateResponse.Split('.');
    string[] CurrentVersion = VERSION.Split('.');
    bool update = false;
    for (int i = 0; i < NewVersion.Length; i++) {
      if (int.Parse(NewVersion[i]) > int.Parse(CurrentVersion[i])) {
        return UpdateResponse;
      }
    }
    return null;
  }
}
