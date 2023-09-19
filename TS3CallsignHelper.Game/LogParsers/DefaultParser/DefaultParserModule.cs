using System.ComponentModel.Composition;
using TS3CallsignHelper.API;
using TS3CallsignHelper.API.Dependencies;
using TS3CallsignHelper.API.Exceptions;
using TS3CallsignHelper.API.LogParsing;

namespace TS3CallsignHelper.Game.LogParsers.DefaultParser;

[Export(typeof(ICallsignHelperModule))]
[ExportMetadata("Name", "Default log parsers")]
public class DefaultParserModule : ICallsignHelperModule {

  public void Load(IDependencyStore dependencyStore) {
    var gameLogParser = dependencyStore.TryGet<IGameLogParser>() ?? throw new MissingDependencyException(typeof(IGameLogParser));
    gameLogParser.Register(new GameSessionParser(dependencyStore));
    gameLogParser.Register(new GateAssignmentParser(dependencyStore));
    //gameLogParser.Register(new MetarParser(dependencyStore));
    gameLogParser.Register(new PlaneSelectionParser(dependencyStore));
    gameLogParser.Register(new PlaneStateParser(dependencyStore));
  }
}
