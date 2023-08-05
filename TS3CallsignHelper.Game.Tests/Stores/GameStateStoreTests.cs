using Moq;
using TS3CallsignHelper.Api;
using TS3CallsignHelper.Api.Dependencies;
using TS3CallsignHelper.Api.Events;
using TS3CallsignHelper.Api.Exceptions;
using TS3CallsignHelper.Api.Stores;
using TS3CallsignHelper.Game.Enums;
using TS3CallsignHelper.Game.Exceptions;
using TS3CallsignHelper.Game.LogParsers;
using TS3CallsignHelper.Game.Services;
using TS3CallsignHelper.Game.Stores;

namespace TS3CallsignHelper.Game.Tests.Stores;

[TestFixture]
internal class GameStateStoreTests {
  Mock<IGameLogParser> _gameLogParserMock;
  Mock<IAirportDataStore> _airportDataStoreMock; bool _airportDataStoreLoaded, _airportDataStoreUnloaded;
  Mock<IInitializationProgressService> _initializationProgressServiceMock;
  Mock<IDependencyStore> _dependencyStoreMock;

  IGameStateStore _storeToTest;

  AirplaneChangedEventArgs? _currentAirplaneChangedArgs;
  PlaneStateChangedEventArgs? _planeStateChangedArgs;
  PlayerPositionChangedEventArgs? _playerPositionChangedArgs;
  GameSessionStartedEventArgs? _gameSessionStartedArgs;
  bool _gameSessionEndedRaised = false;

  [SetUp]
  public void Setup() {
    _gameLogParserMock = new Mock<IGameLogParser>();
    _airportDataStoreMock = new Mock<IAirportDataStore>(); _airportDataStoreLoaded = false; _airportDataStoreUnloaded = false;
    _initializationProgressServiceMock = new Mock<IInitializationProgressService>();
    _dependencyStoreMock = new Mock<IDependencyStore>();


    string installationDir = "install", airport = "EDDV", database = "testDb", airplaneSet = "testAp";
    _airportDataStoreMock.Setup(s => s.Load(installationDir, airport, database, airplaneSet)).Callback(() => _airportDataStoreLoaded = true);
    _airportDataStoreMock.Setup(s => s.Unload()).Callback(() => _airportDataStoreUnloaded = true);

    _storeToTest = new GameStateStore(_dependencyStoreMock.Object);

    _currentAirplaneChangedArgs = null;
    _planeStateChangedArgs = null;
    _playerPositionChangedArgs = null;
    _gameSessionStartedArgs = null;
    _gameSessionEndedRaised = false;

    _storeToTest.CurrentAirplaneChanged += e => _currentAirplaneChangedArgs = e;
    _storeToTest.PlaneStateChanged += e => _planeStateChangedArgs = e;
    _storeToTest.ActivePositionChanged += e => _playerPositionChangedArgs = e;
    _storeToTest.GameSessionStarted += e => _gameSessionStartedArgs = e;
    _storeToTest.GameSessionEnded += () => _gameSessionEndedRaised = true;
  }

  [Test]
  public void OnInstallationDirDetermined_DuringRuntime() {
    _gameLogParserMock.Raise(s => s.InstallDirDetermined += null, "install");
    _gameLogParserMock.Raise(s => s.InstallDirDetermined += null, "install2");
  }

  [Test]
  public void OnGameSessionStart_WithFullInformation() {
    //Arrange

    string installationDir = "install", airport = "EDDV", database = "testDb", airplaneSet = "testAp", instruments = "TestUi";
    GameInfo gameInfo = new GameInfo { AirportICAO = airport, DatabaseFolder = database, AirplaneSetFolder = airplaneSet, InstrumentSetFolder = instruments };
    // Act
    _gameLogParserMock.Raise(s => s.InstallDirDetermined += null, installationDir);
    _gameLogParserMock.Raise(s => s.GameSessionSarted += null, gameInfo);

    // Assert
    Assert.Multiple(() => {
      Assert.That(_airportDataStoreLoaded);
      Assert.That(_gameSessionStartedArgs?.Info, Is.EqualTo(gameInfo));
    });
  }

  [Test]
  public void OnGameSessionStart_MissingAirportCode() {
    // Arrange
    string installationDir = "install", airport = "EDDV", database = "testDb", airplaneSet = "testAp", instruments = "TestUi";
    GameInfo gameInfo = new GameInfo { AirportICAO = null, DatabaseFolder = database, AirplaneSetFolder = airplaneSet, InstrumentSetFolder = instruments };

    // Act
    _gameLogParserMock.Raise(s => s.InstallDirDetermined += null, installationDir);
    IncompleteGameInfoException exception = Assert.Throws<IncompleteGameInfoException>(() => _gameLogParserMock.Raise(s => s.GameSessionSarted += null, gameInfo));
    
    // Assert
    Assert.Multiple(() => {
      Assert.That(_airportDataStoreLoaded, Is.False);
      Assert.That(_airportDataStoreUnloaded, Is.False);
      Assert.That(exception.Property, Is.EqualTo(nameof(GameInfo.AirportICAO)));
    });
  }

  [Test]
  public void OnGameSessionStart_MissingDatabase() {
    // Arrange
    string installationDir = "install", airport = "EDDV", database = "testDb", airplaneSet = "testAp", instruments = "TestUi";
    GameInfo gameInfo = new GameInfo { AirportICAO = airport, DatabaseFolder = null, AirplaneSetFolder = airplaneSet, InstrumentSetFolder = instruments };

    // Act
    _gameLogParserMock.Raise(s => s.InstallDirDetermined += null, installationDir);
    IncompleteGameInfoException exception = Assert.Throws<IncompleteGameInfoException>(() => _gameLogParserMock.Raise(s => s.GameSessionSarted += null, gameInfo));

    // Assert
    Assert.Multiple(() => {
      Assert.That(_airportDataStoreLoaded, Is.False);
      Assert.That(_airportDataStoreUnloaded, Is.False);
      Assert.That(exception.Property, Is.EqualTo(nameof(GameInfo.DatabaseFolder)));
    });
  }

  [Test]
  public void OnGameSessionStart_MissingAirplaneSet() {
    // Arrange
    string installationDir = "install", airport = "EDDV", database = "testDb", airplaneSet = "testAp", instruments = "TestUi";
    GameInfo gameInfo = new GameInfo { AirportICAO = airport, DatabaseFolder = database, AirplaneSetFolder = null, InstrumentSetFolder = instruments };

    // Act
    _gameLogParserMock.Raise(s => s.InstallDirDetermined += null, installationDir);
    IncompleteGameInfoException exception = Assert.Throws<IncompleteGameInfoException>(() => _gameLogParserMock.Raise(s => s.GameSessionSarted += null, gameInfo));

    // Assert
    Assert.Multiple(() => {
      Assert.That(_airportDataStoreLoaded, Is.False);
      Assert.That(_airportDataStoreUnloaded, Is.False);
      Assert.That(exception.Property, Is.EqualTo(nameof(GameInfo.AirplaneSetFolder)));
    });
  }

  [Test]
  public void OnGameSessionStart_MissingInstrumentSet() {
    // Arrange
    string installationDir = "install", airport = "EDDV", database = "testDb", airplaneSet = "testAp", instruments = "TestUi";
    GameInfo gameInfo = new GameInfo { AirportICAO = airport, DatabaseFolder = database, AirplaneSetFolder = airplaneSet, InstrumentSetFolder = null };

    // Act
    _gameLogParserMock.Raise(s => s.InstallDirDetermined += null, installationDir);
    _gameLogParserMock.Raise(s => s.GameSessionSarted += null, gameInfo);

    // Assert
    Assert.Multiple(() => {
      Assert.That(_airportDataStoreLoaded);
      Assert.That(_airportDataStoreUnloaded, Is.False);
    });
  }

  [Test]
  public void OnGameSessionStart_MissingInstallation() {
    // Arrange
    string installationDir = "install", airport = "EDDV", database = "testDb", airplaneSet = "testAp", instruments = "TestUi";
    GameInfo gameInfo = new GameInfo { AirportICAO = airport, DatabaseFolder = database, AirplaneSetFolder = airplaneSet, InstrumentSetFolder = instruments };

    // Act
    IncompleteGameInfoException exception = Assert.Throws<IncompleteGameInfoException>(() => _gameLogParserMock.Raise(s => s.GameSessionSarted += null, gameInfo));

    // Assert
    Assert.Multiple(() => {
      Assert.That(_airportDataStoreLoaded, Is.False);
      Assert.That(_airportDataStoreUnloaded, Is.False);
      Assert.That(exception.Property, Is.EqualTo("Installation"));
    });
  }

  [Test]
  public void OnGameSessionEnded() {
    OnGameSessionStart_WithFullInformation();

    _gameLogParserMock.Raise(s => s.GameSessionEnded += null);

    Assert.That(_storeToTest.CurrentGameInfo, Is.Null);
    Assert.That(_gameSessionEndedRaised);
  }

  [Test]
  public void OnNewActivePlane() {
    _gameLogParserMock.Raise(s => s.NewActivePlane += null, "airplane");

    Assert.That(_storeToTest.CurrentAirplane, Is.EqualTo("airplane"));
  }

  [Test]
  [TestCase(PlayerPosition.Ground)]
  [TestCase(PlayerPosition.Tower)]
  public void SetPlayerPosition(PlayerPosition pos) {
    _storeToTest.SetPlayerPosition(pos, true);

    Assert.That(_storeToTest.PlayerPositions.Contains(pos));

    _storeToTest.SetPlayerPosition(pos, true);

    Assert.That(_storeToTest.PlayerPositions.Contains(pos));

    _storeToTest.SetPlayerPosition(pos, false);

    Assert.That(_storeToTest.PlayerPositions.Contains(pos), Is.False);

    _storeToTest.SetPlayerPosition(pos, false);

    Assert.That(_storeToTest.PlayerPositions.Contains(pos), Is.False);
  }

  [Test]
  [TestCase(PlayerPosition.Ground, PlaneState.OUT_STARTUP_REQUEST)] // valid initial for Ground
  [TestCase(PlayerPosition.Ground, PlaneState.OUT_RWY_LINE_UP)] // invalid initial for Ground
  [TestCase(PlayerPosition.Tower, PlaneState.IN_RWY_APPROACH)] // valid initial for Tower
  [TestCase(PlayerPosition.Tower, PlaneState.OUT_STARTUP_REQUEST)] // invalid initial for Tower
  public void SetPlaneState_DuringCatchup_Succeeds(PlayerPosition pos, PlaneState state) {
    _gameLogParserMock.Setup(s => s.State).Returns(ParserState.INIT_CATCHUP);
    _storeToTest.SetPlayerPosition(pos, true);

    _gameLogParserMock.Raise(s => s.NewPlaneState += null, "callsign", state);

    Assert.That(_storeToTest.PlaneStates["callsign"], Is.EqualTo(state));
  }

  [Test]
  [TestCase(PlayerPosition.Ground, PlaneState.OUT_STARTUP_REQUEST)] // valid initial for Ground
  public void SetPlaneState_OutsideCatchup_Succeeds(PlayerPosition pos, PlaneState state) {
    _gameLogParserMock.Setup(s => s.State).Returns(ParserState.RUNNING);
    _storeToTest.SetPlayerPosition(pos, true);

    _gameLogParserMock.Raise(s => s.NewPlaneState += null, "callsign", state);

    Assert.That(_storeToTest.PlaneStates["callsign"], Is.EqualTo(state));
  }

  [Test]
  [TestCase(PlayerPosition.Ground, PlaneState.OUT_TAXI_PROGRESS)] // invalid initial for Ground
  public void SetPlaneState_OutsideCatchup_Fails(PlayerPosition pos, PlaneState state) {
    _gameLogParserMock.Setup(s => s.State).Returns(ParserState.RUNNING);
    _storeToTest.SetPlayerPosition(pos, true);

    Assert.Throws<InvalidPlaneStateException>(() =>_gameLogParserMock.Raise(s => s.NewPlaneState += null, "callsign", state));

    Assert.That(_storeToTest.PlaneStates.ContainsKey("callsign"), Is.False);
  }

  [Test]
  public void Dispose() {
    _storeToTest.Dispose();

    _gameLogParserMock.Raise(s => s.InstallDirDetermined += null, "install");
    _gameLogParserMock.Raise(s => s.NewActivePlane += null, "callsign");
    _gameLogParserMock.Raise(s => s.MetarUpdated += null, new Metar());
    _gameLogParserMock.Raise(s => s.GameSessionEnded += null);
    _gameLogParserMock.Raise(s => s.GameSessionSarted += null, new GameInfo());
    _gameLogParserMock.Raise(s => s.NewPlaneState += null, "callsign", PlaneState.UNKNOWN);

    Assert.Multiple(() => {
      Assert.That(_airportDataStoreUnloaded);
      Assert.That(_gameSessionEndedRaised, Is.False);
      Assert.That(_gameSessionStartedArgs, Is.Null);
      Assert.That(_planeStateChangedArgs, Is.Null);
    });
  }
}
