using Moq;
using TS3CallsignHelper.API;
using TS3CallsignHelper.API.Dependencies;
using TS3CallsignHelper.API.Events;
using TS3CallsignHelper.API.Exceptions;
using TS3CallsignHelper.API.LogParsing;
using TS3CallsignHelper.API.Stores;
using TS3CallsignHelper.Game.Exceptions;
using TS3CallsignHelper.Game.Services;
using TS3CallsignHelper.Game.Stores;

namespace TS3CallsignHelper.Game.Tests.Stores;

[TestFixture]
internal class GameStateStoreTests {
  Mock<IGameLogParser> _gameLogParserMock;
  Mock<IAirportDataStore> _airportDataStoreMock; bool _airportDataStoreLoaded, _airportDataStoreUnloaded;
  Mock<IInitializationProgressService> _initializationProgressServiceMock;
  Mock<IDependencyStore> _dependencyStoreMock;

  GameStateStore _storeToTest;

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

    string installationDir = "install";
    GameInfo gameInfo = new GameInfo { AirportICAO = "EDDV", DatabaseFolder = "testDb", AirplaneSetFolder = "testAp", InstrumentSetFolder="testIs", StartHour=12};
    _airportDataStoreMock.Setup(s => s.Load(installationDir, gameInfo)).Callback(() => _airportDataStoreLoaded = true);
    _airportDataStoreMock.Setup(s => s.Unload()).Callback(() => _airportDataStoreUnloaded = true);

    _storeToTest = new GameStateStore(_dependencyStoreMock.Object);
    _storeToTest.SetInstallDir(installationDir);

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
    _storeToTest.SetInstallDir("install2");
  }

  [Test]
  public void OnGameSessionStart_WithFullInformation() {
    //Arrange

    string installationDir = "install", airport = "EDDV", database = "testDb", airplaneSet = "testAp", instruments = "TestUi";
    GameInfo gameInfo = new GameInfo { AirportICAO = airport, DatabaseFolder = database, AirplaneSetFolder = airplaneSet, InstrumentSetFolder = instruments };
    // Act
    _storeToTest.StartGame(gameInfo);

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
    IncompleteGameInfoException exception = Assert.Throws<IncompleteGameInfoException>(() => _storeToTest.StartGame(gameInfo));
    
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
    IncompleteGameInfoException exception = Assert.Throws<IncompleteGameInfoException>(() => _storeToTest.StartGame(gameInfo));

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
    IncompleteGameInfoException exception = Assert.Throws<IncompleteGameInfoException>(() => _storeToTest.StartGame(gameInfo));

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
    _storeToTest.StartGame(gameInfo);

    // Assert
    Assert.Multiple(() => {
      Assert.That(_airportDataStoreLoaded);
      Assert.That(_airportDataStoreUnloaded, Is.False);
    });
  }

  [Test]
  public void OnGameSessionEnded() {
    OnGameSessionStart_WithFullInformation();

    _storeToTest.EndGame();

    Assert.That(_storeToTest.CurrentGameInfo, Is.Null);
    Assert.That(_gameSessionEndedRaised);
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

    _storeToTest.SetPlaneState("callsign", new PlaneStateInfo() { State = state });

    Assert.That(_storeToTest.PlaneStates["callsign"].State, Is.EqualTo(state));
  }

  [Test]
  [TestCase(PlayerPosition.Ground, PlaneState.OUT_STARTUP_REQUEST)] // valid initial for Ground
  public void SetPlaneState_OutsideCatchup_Succeeds(PlayerPosition pos, PlaneState state) {
    _gameLogParserMock.Setup(s => s.State).Returns(ParserState.RUNNING);
    _storeToTest.SetPlayerPosition(pos, true);

    _storeToTest.SetPlaneState("callsign", new PlaneStateInfo() { State = state });

    Assert.That(_storeToTest.PlaneStates["callsign"].State, Is.EqualTo(state));
  }

  [Test]
  [TestCase(PlayerPosition.Ground, PlaneState.OUT_TAXI_PROGRESS)] // invalid initial for Ground
  public void SetPlaneState_OutsideCatchup_Fails(PlayerPosition pos, PlaneState state) {
    _gameLogParserMock.Setup(s => s.State).Returns(ParserState.RUNNING);
    _storeToTest.SetPlayerPosition(pos, true);

    _storeToTest.SetPlaneState("callsign", new PlaneStateInfo() { State = state });

    Assert.That(_storeToTest.PlaneStates["callsign"].State, Is.EqualTo(state));
  }
}
