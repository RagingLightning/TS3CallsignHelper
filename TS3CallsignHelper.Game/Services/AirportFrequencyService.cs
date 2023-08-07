using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using TS3CallsignHelper.API;
using TS3CallsignHelper.API.Dependencies;
using TS3CallsignHelper.API.Exceptions;
using TS3CallsignHelper.API.Logging;
using TS3CallsignHelper.Game.Exceptions;

namespace TS3CallsignHelper.Game.Services;
public partial class AirportFrequencyService : IAirportFrequencyService {
  [GeneratedRegex("^(?<q0>\"?)(?<frequency>[0-9.]+?)\\k<q0>,(?<q1>\"?)(?<writename>.+?)\\k<q1>,(?<q2>\"?)(?<sayname>.+?)\\k<q2>,(?<q3>\"?)(?<readback>.+?)\\k<q3>,(?<q4>\"?)(?<controlarea>.+?)\\k<q4>$")]
  private static partial Regex Parser();
  private readonly ILogger<AirportFrequencyService>? _logger;
  private readonly IInitializationProgressService _initializationProgressService;

  /// <summary>
  /// Requires <seealso cref="IInitializationProgressService"/>
  /// </summary>
  /// <param name="dependencyStore"></param>
  public AirportFrequencyService(IDependencyStore dependencyStore) {
    _logger = dependencyStore.TryGet<ILoggerService>()?.GetLogger<AirportFrequencyService>();
    _initializationProgressService = dependencyStore.TryGet<IInitializationProgressService>() ?? throw new MissingDependencyException(typeof(IInitializationProgressService));
  }
  public ImmutableDictionary<string, AirportFrequency> Load(string installation, GameInfo info) {

    var frequencies = new Dictionary<string, AirportFrequency>();

    _initializationProgressService.StatusMessage = "Loading frequencies...";

    var airport = info.AirportICAO ?? throw new IncompleteGameInfoException(info, nameof(info.AirportICAO));
    var database = info.DatabaseFolder ?? throw new IncompleteGameInfoException(info, nameof(info.DatabaseFolder));

    var configFile = Path.Combine(installation, "Airports", airport, "databases", database, "freq.csv");
    _logger.LogDebug("Loading frequencies from {Config}", configFile);
    var stream = File.Open(configFile, FileMode.Open, FileAccess.Read, FileShare.Read);
    using var reader = new StreamReader(stream);
    reader.ReadLine(); // first line contains headers
    while (reader.ReadLine() is string line) {
      _logger.LogTrace("Loading frequency from {Line}", line);
      var groups = Parser().Match(line).Groups;
      if (groups.Count == 1) throw new FrequencyDefinitionFormatException(line);

      if (!MakeFrequency(groups, out var frequency)) continue;

      frequencies.Add(frequency.Frequency, frequency);
      _logger.LogDebug("Added frequency {@Frequency}", frequency);
      _initializationProgressService.FrequencyProgress = ((float) stream.Position) / stream.Length;
    }
    _initializationProgressService.FrequencyProgress = 1;

    return frequencies.ToImmutableDictionary();
  }

  private bool MakeFrequency(GroupCollection groups, out AirportFrequency result) {
    result = new AirportFrequency();
    var frequency = groups["frequency"].Value;
    var writename = groups["writename"].Value;
    var sayname = groups["sayname"].Value;
    var readback = groups["readback"].Value;
    var controlArea = groups["controlarea"].Value;

    var typeCheck = writename.ToUpper();
    AirportFrequencyType type;
    if (typeCheck.Contains("DEPARTURE") || typeCheck.Contains("CENTER"))
      type = AirportFrequencyType.DEPARTURE;
    else if (typeCheck.Contains("TOWER"))
      type = AirportFrequencyType.TOWER;
    else if (typeCheck.Contains("GROUND") || typeCheck.Contains("APRON"))
      type = AirportFrequencyType.GROUND;
    else {
      _logger.LogWarning("Could not determine frequency type: {Frequency}", writename);
      return false;
    }

    result.Type = type;
    result.Frequency = frequency;
    result.Writename = writename;
    result.Sayname = sayname;
    result.Readback = readback;
    result.ControlArea = controlArea;
    return true;
  }

}
