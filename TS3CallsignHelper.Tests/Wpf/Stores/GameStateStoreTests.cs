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

namespace TS3CallsignHelper.Tests.Wpf.Stores;

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
    _airportDataStoreMock = new Mock<IAirportDataStore>(); _airportDataStoreLoaded = false; _airportDataStoreUnloaded = false;
    _initializationProgressServiceMock = new Mock<IInitializationProgressService>();
    _dependencyStoreMock = new Mock<IDependencyStore>();
    _dependencyStoreMock.Setup(s => s.TryGet<IInitializationProgressService>()).Returns(_initializationProgressServiceMock.Object);
    _dependencyStoreMock.Setup(s => s.TryGet<IAirportDataStore>()).Returns(_airportDataStoreMock.Object);

    string installationDir = "install";
    _airportDataStoreMock.Setup(s => s.Load(installationDir, It.IsAny<GameInfo>())).Callback(() => _airportDataStoreLoaded = true);
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
  public void SetInstallDir() {
    _storeToTest.InstallDir = "install";

    Assert.That(_storeToTest.InstallDir, Is.EqualTo("install"));
  }

  [Test]
  public void SetInstallDir_DuringRuntime() {
    _storeToTest.InstallDir = "install";
    _storeToTest.InstallDir = "installchange";

    Assert.That(_storeToTest.InstallDir, Is.EqualTo("install"));
  }

  [Test]
  public void OnGameSessionStart_WithFullInformation() {
    //Arrange

    string installationDir = "install", airport = "EDDV", database = "testDb", airplaneSet = "testAp", instruments = "TestUi";
    GameInfo gameInfo = new GameInfo { AirportICAO = airport, DatabaseFolder = database, AirplaneSetFolder = airplaneSet, InstrumentSetFolder = instruments };
    // Act
    _storeToTest.InstallDir = installationDir;
    _storeToTest.StartGame(gameInfo);

    // Assert
    Assert.Multiple(() => {
      Assert.That(_storeToTest.InstallDir, Is.EqualTo(installationDir));
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
    _storeToTest.InstallDir = installationDir;
    IncompleteGameInfoException exception = Assert.Throws<IncompleteGameInfoException>(() => _storeToTest.StartGame(gameInfo));

    // Assert
    Assert.Multiple(() => {
      Assert.That(_storeToTest.InstallDir, Is.EqualTo(installationDir));
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
    _storeToTest.InstallDir = installationDir;
    IncompleteGameInfoException exception = Assert.Throws<IncompleteGameInfoException>(() => _storeToTest.StartGame(gameInfo));

    // Assert
    Assert.Multiple(() => {
      Assert.That(_storeToTest.InstallDir, Is.EqualTo(installationDir));
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
    _storeToTest.InstallDir = installationDir;
    IncompleteGameInfoException exception = Assert.Throws<IncompleteGameInfoException>(() => _storeToTest.StartGame(gameInfo));

    // Assert
    Assert.Multiple(() => {
      Assert.That(_storeToTest.InstallDir, Is.EqualTo(installationDir));
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
    _storeToTest.InstallDir = installationDir;
    _storeToTest.StartGame(gameInfo);

    // Assert
    Assert.Multiple(() => {
      Assert.That(_storeToTest.InstallDir, Is.EqualTo(installationDir));
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
    IncompleteGameInfoException exception = Assert.Throws<IncompleteGameInfoException>(() => _storeToTest.StartGame(gameInfo));

    // Assert
    Assert.Multiple(() => {
      Assert.That(_storeToTest.InstallDir, Is.EqualTo(string.Empty));
      Assert.That(_airportDataStoreLoaded, Is.False);
      Assert.That(_airportDataStoreUnloaded, Is.False);
      Assert.That(exception.Property, Is.EqualTo(nameof(_storeToTest.InstallDir)));
    });
  }

  [Test]
  public void OnGameSessionEnded() {
    string installationDir = "install", airport = "EDDV", database = "testDb", airplaneSet = "testAp", instruments = "TestUi";
    GameInfo gameInfo = new GameInfo { AirportICAO = airport, DatabaseFolder = database, AirplaneSetFolder = airplaneSet, InstrumentSetFolder = instruments };
    _storeToTest.InstallDir = installationDir;
    _storeToTest.StartGame(gameInfo);

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
    _storeToTest.SetPlayerPosition(pos, false);
    Assert.That(_storeToTest.PlayerPositions.Contains(pos), Is.False);
  }

  [Test]
  public void ForcePlaneState_Succeeds([Values]PlayerPosition pos, [Values]PlaneState state) {
    _storeToTest.SetPlayerPosition(pos, true);

    _storeToTest.ForcePlaneState("DLH8192", new PlaneStateInfo() { State = state });

    Assert.That(_storeToTest.PlaneStates["DLH8192"].State, Is.EqualTo(state));
  }

  [Test]
  public void SetPlaneState_SucceedsForInitials([Values] PlayerPosition pos, [Values] PlaneState state) {
    _storeToTest.SetPlayerPosition(pos, true);

    _storeToTest.SetPlaneState("DLH8192", new PlaneStateInfo() { State = state });

    if (state.IsInitial(pos) || state == PlaneState.UNKNOWN)
      Assert.That(_storeToTest.PlaneStates["DLH8192"].State, Is.EqualTo(state));
    else
      Assert.That(_storeToTest.PlaneStates.ContainsKey("DLH8192"), Is.False);
  }

  [Test]
  public void SetPlaneState_SucceedsForGroundStates([Values] PlaneState state) {
    _storeToTest.SetPlayerPosition(PlayerPosition.Ground, true);
    _storeToTest.SetPlaneState("DLH8192", new PlaneStateInfo { State = PlaneState.OUT_PUSHBACK_REQUEST });

    _storeToTest.SetPlaneState("DLH8192", new PlaneStateInfo() { State = state });

    if (state.Is(PlayerPosition.Ground))
      Assert.That(_storeToTest.PlaneStates["DLH8192"].State, Is.EqualTo(state));
    else
      Assert.That(_storeToTest.PlaneStates["DLH8192"].State, Is.EqualTo(PlaneState.OUT_PUSHBACK_REQUEST));
  }

  [Test]
  public void SetPlaneState_SucceedsForTowerStates([Values] PlaneState state) {
    _storeToTest.SetPlayerPosition(PlayerPosition.Tower, true);
    _storeToTest.SetPlaneState("DLH8192", new PlaneStateInfo { State = PlaneState.IN_RWY_APPROACH });

    _storeToTest.SetPlaneState("DLH8192", new PlaneStateInfo() { State = state });

    if (state.Is(PlayerPosition.Tower))
      Assert.That(_storeToTest.PlaneStates["DLH8192"].State, Is.EqualTo(state));
    else
      Assert.That(_storeToTest.PlaneStates["DLH8192"].State, Is.EqualTo(PlaneState.IN_RWY_APPROACH));
  }
}
