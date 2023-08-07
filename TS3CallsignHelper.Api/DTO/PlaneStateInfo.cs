using System.Collections.Generic;
using System.Text;

namespace TS3CallsignHelper.API;
public class PlaneStateInfo {
  public PlaneState State { get; set; } = PlaneState.UNKNOWN;
  public string? Runway { get; set; }
  public string? RunwayIntersection { get; set; }
  public string? RunwayCross { get; set; }
  public string? Gate { get; set; }
  public List<string> TaxiVia { get; set; } = new();
  public string? HoldShort { get; set; }
  public (string A, string B)? TaxiIntersection { get; set; }

  public PlaneStateInfo(PlaneStateInfo? copyFrom = null) {
    if (copyFrom != null) {
      State = copyFrom.State;
      Runway = copyFrom.Runway;
      RunwayIntersection = copyFrom.RunwayIntersection;
      RunwayCross = copyFrom.RunwayCross;
      Gate = copyFrom.Gate;
      TaxiVia.AddRange(copyFrom.TaxiVia);
      HoldShort = copyFrom.HoldShort;
      TaxiIntersection = copyFrom.TaxiIntersection;
    }
  }
}
