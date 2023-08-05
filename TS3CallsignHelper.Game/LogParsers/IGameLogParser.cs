using TS3CallsignHelper.Api;
using TS3CallsignHelper.Api.Events;
using TS3CallsignHelper.Game.Enums;

namespace TS3CallsignHelper.Game.LogParsers;
public interface IGameLogParser {

  public event LogLineReadEventHandler? LogLineRead;
  public event Action<string>? InstallDirDetermined;
  public event Action<GameInfo>? GameSessionSarted;
  public event Action? GameSessionEnded;
  public event Action<Metar>? MetarUpdated;
  public event Action<string>? NewActivePlane;
  public event Action<string, PlaneState>? NewPlaneState;
  public ParserState State { get; }

  public void Init(string logFolder);
  public void Start();
  public void Stop();



}
