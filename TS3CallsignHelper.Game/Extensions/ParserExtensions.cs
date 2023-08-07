namespace TS3CallsignHelper.Game.Extensions;
public static class ParserExtensions {
  public static int? GetIndex(this string[] haystack, string needle, params string[] exclude) {
    List<int> indices = new();
    var needleSegments = needle.Split(' ');
    for (int i = 0; i < haystack.Length-needleSegments.Length; i++) {
      bool isMatch = true;
      for (int j = 0; j < needleSegments.Length; j++) {
        if (haystack[i+j] != needleSegments[j] && needleSegments[j] != "?") {
          isMatch = false;
          break;
        }
      }
      if (isMatch)
        indices.Add(i);
    }

    foreach (var index in indices) {
      bool shouldInclude = true;
      foreach (var exclusion in exclude) {
        var exclusionSegments = exclusion.Split(' ');
        if (index - exclusionSegments.Length < 0) break;
        bool failed = false;
        for (int i = 0; i < exclusionSegments.Length; i++) {
          if (haystack[index - exclusionSegments.Length + i] != exclusionSegments[i] && exclusionSegments[i] != "?") {
            failed = true;
            break;
          }
        }
        if (!failed) {
          shouldInclude = false;
          break;
        }
      }
      if (shouldInclude)
        return index+needleSegments.Length;
    }

    return null;
  }
}
