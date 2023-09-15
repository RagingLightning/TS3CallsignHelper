using System;
using System.Runtime.CompilerServices;

namespace TS3CallsignHelper.Modules.CommandSuggestion.Models;
public class SuggestedCommand {
  public string Text { get; }
  public string[] Segments { get; }

  private SuggestedCommand(string text, string[] segments) {
    Text = text;
    Segments = segments;
  }

  internal static readonly SuggestedCommand PULL_BACK = new("PULL BACK TO THE GATE", new[] { "PULL BACK TO THE GATE" });
  internal static readonly SuggestedCommand HOLD_POSITION = new("HOLD POSITION", new[] { "HOLD POSITION" });
  internal static readonly SuggestedCommand TAXI_HOLD_INTERSECTION = new("TAXI AND HOLD AT THE INTERSECTION OF TAXIWAY ? AND ? [VIA ?]", new[] { "TAXI AND HOLD AT THE INTERSECTION OF TAXIWAY ", " AND ", " VIA " });
  internal static readonly SuggestedCommand HOLD_SHORT_TAXIWAY = new("HOLD SHORT OF TAXIWAY ?", new[] { "HOLD SHORT OF TAXIWAY " });
  internal static readonly SuggestedCommand CONTINUE_TAXI = new("CONTINUE TAXI", new[] { "CONTINUE TAXI" });
  internal static readonly SuggestedCommand CROSS_RUNWAY = new("CROSS RUNWAY ??", new[] { "CROSS RUNWAY " });
  internal static readonly SuggestedCommand CHANGE_RUNWAY = new("CHANGE TO RUNWAY ??", new[] { "CHANGE TO RUNWAY " });
  internal static readonly SuggestedCommand GO_AROUND = new("GO AROUND", new[] { "GO AROUND" });
  internal static readonly SuggestedCommand EXIT_RUNWAY_AT = new("TAKE NEXT AVAILABLE EXIT ON [left|right]", new[] { "TAKE NEXT AVAILABLE EXIT ON " });
  internal static readonly SuggestedCommand EXIT_RUNWAY_ON = new("EXIT AT TAXIWAY ?", new[] { "EXIT AT TAXIWAY " });
  internal static readonly SuggestedCommand EXIT_RUNWAY_AT_ON = new("VACATE RUNWAY [left|right] ONTO TAXIWAY ?", new[] { "VACATE RUNWAY ", " ONTO TAXIWAY " });
  internal static readonly SuggestedCommand TAXI_TERMINAL = new("TAXI TO TERMINAL [VIA ?]", new[] { "TAXI TO TERMINAL", " VIA " });

  internal static SuggestedCommand StartupApproved(string? runway = null) {
    if (runway is null)
      return new("APPROVED, EXPECT RUNWAY ??", new[] { "APPROVED, EXPECT RUNWAY " });
    return new($"APPROVED, EXPECT RUNWAY {runway}", new[] { $"APPROVED, EXPECT RUNWAY {runway}" });
  }
  internal static SuggestedCommand PushbackApproved(string? runway = null) {
    if (runway is null)
      return new("PUSHBACK APPROVED, EXPECT RUNWAY ??", new[] { "PUSHBACK APPROVED, EXPECT RUNWAY " });
    return new($"PUSHBACK APPROVED, EXPECT RUNWAY {runway}", new[] { $"PUSHBACK APPROVED, EXPECT RUNWAY {runway}" });
  }

  internal static SuggestedCommand RunwayVia(string? runway = null) {
    if (runway is null)
      return new("RUNWAY ?? [VIA ? ? ?]", new[] { "RUNWAY ", " VIA " });
    return new($"RUNWAY {runway} [VIA ? ? ?]", new[] { $"RUNWAY {runway}", " VIA " });
  }

  internal static SuggestedCommand RunwayAtVia(string? runway = null, string? intersection = null) {
    if (runway is null)
      return new("RUNWAY ?? AT ? [VIA ? ?]", new[] { "RUNWAY ", " AT ", " VIA " });
    if (intersection is null)
      return new($"RUNWAY {runway} AT ? [VIA ? ?]", new[] { $"RUNWAY {runway}", " AT ", " VIA " });
    return new($"RUNWAY {runway} AT {intersection} [VIA ? ?]", new[] { $"RUNWAY {runway}", $" AT {intersection}", " VIA " });
  }

  internal static SuggestedCommand LineUp(string? runway = null) {
    if (runway is null)
      return new("RUNWAY ?? LINE UP AND WAIT [BEHIND NEXT LANDING AIRCRAFT]", new[] { "RUNWAY ", " LINE UP AND WAIT", " BEHIND NEXT LANDING AIRCRAFT" });
    return new($"RUNWAY {runway} LINE UP AND WAIT [BEHIND NEXT LANDING AIRCRAFT]", new[] { $"RUNWAY {runway}", " LINE UP AND WAIT", " BEHIND NEXT LANDING AIRCRAFT" });
  }

  internal static SuggestedCommand ClearedTakeoff(string? runway = null) {
    if (runway is null)
      return new("RUNWAY ?? CLEARED FOR TAKEOFF", new[] { "RUNWAY ", " CLEARED FOR TAKEOFF" });
    return new($"RUNWAY {runway} CLEARED FOR TAKEOFF", new[] { $"RUNWAY {runway}", " CLEARED FOR TAKEOFF" });
  }

  internal static SuggestedCommand ClearedImmediateTakeoff(string? runway = null) {
    if (runway is null)
      return new("RUNWAY ?? CLEARED FOR IMMEDIATE TAKEOFF", new[] { "RUNWAY ", " CLEARED FOR IMMEDIATE TAKEOFF" });
    return new($"RUNWAY {runway} CLEARED FOR IMMEDIATE TAKEOFF", new[] { $"RUNWAY {runway}", " CLEARED FOR IMMEDIATE TAKEOFF" });
  }

  internal static SuggestedCommand ClearedLand(string? runway = null) {
    if (runway is null)
      return new("RUNWAY ?? CLEARED TO LAND", new[] { "RUNWAY ", " CLEARED TO LAND" });
    return new($"RUNWAY {runway} CLEARED TO LAND", new[] { $"RUNWAY {runway}", " CLEARED TO LAND" });
  }

  internal static SuggestedCommand Contact(string? freq) {
    return new SuggestedCommand($"CONTACT {freq}", new[] { $"CONTACT {freq}" });
  }
}
