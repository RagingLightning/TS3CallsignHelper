using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using TS3CallsignHelper.Game.Exceptions;
using TS3CallsignHelper.Game.Services;

namespace TS3CallsignHelper.Game.Models;
public partial class AirportAirlineConfig {
  [GeneratedRegex("^(?<q0>\"?)(?<airline>[A-Z]{3})\\k<q0>,(?<q1>\"?)(?<callsign>.+?)\\k<q1>,(?<q2>\"?)(?<name>.+?)\\k<q2>,(?<q3>\"?)(?<country>.+?)\\k<q3>$")]
  private static partial Regex Parser();
  private readonly ILogger<AirportAirlineConfig> _logger;
  private readonly InitializationProgressService _initializationProgress;

  public IEnumerable<AirportAirline> Airlines => _airlines.Values;

  private Dictionary<string, AirportAirline> _airlines;

  public AirportAirlineConfig(string configPath, IServiceProvider serviceProvider, InitializationProgressService initializationProgress) {
    _logger = serviceProvider.GetRequiredService<ILogger<AirportAirlineConfig>>();

    _airlines = new Dictionary<string, AirportAirline>();

    initializationProgress.StatusMessage = "Loading airlines...";
    _logger.LogDebug("Loading airlines from {Config}", configPath);
    var stream = File.Open(configPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    using var reader = new StreamReader(stream);
    reader.ReadLine(); // first line contains headers
    while (reader.ReadLine() is string line) {
      _logger.LogTrace("Loading airline from {Line}", line);
      var groups = Parser().Match(line).Groups;
      if (groups.Count == 1) throw new AirlineDefinitionFormatException(line);

      var code = groups["airline"].Value;
      var airline = new AirportAirline(groups["airline"].Value, groups["callsign"].Value, groups["name"].Value, groups["country"].Value);
      _airlines.Add(code, airline);
      _logger.LogDebug("Added Airline {@Airline}", airline);
      initializationProgress.AirlineProgess = ((float) stream.Position) / stream.Length;
    }
    initializationProgress.AirlineProgess = 1;
  }

  public bool Contains(string code) => _airlines.ContainsKey(code);

  public bool TryGet(string code, out AirportAirline airline) {
    return _airlines.TryGetValue(code, out airline);
  }
}

public class AirportAirline {

  public string Code { get; }
  public string Callsign { get; }
  public string Name { get; }
  public string Country { get; }

  public AirportAirline(string airline, string callsign, string name, string country) {
    Code = airline;
    Callsign = callsign;
    Name = name;
    Country = country;
  }
}
