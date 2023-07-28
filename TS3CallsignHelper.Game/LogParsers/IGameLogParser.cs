using TS3CallsignHelper.Game.Enums;
using TS3CallsignHelper.Game.Models;

namespace TS3CallsignHelper.Game.LogParsers;
public interface IGameLogParser {

  public event Action<string>? InstallDirDetermined;
  public event Action<GameInfo>? GameSessionSarted;
  public event Action? GameSessionEnded;
  public event Action<Metar>? MetarUpdated;
  public event Action<string>? NewActivePlane;
  public event Action<string, PlaneState>? NewPlaneState;

  public void Init(string logFile);
  public ParserState GetState();
  public void Start();
  public void Stop();



}
