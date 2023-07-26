using TS3CallsignHelper.Game.Enums;
using TS3CallsignHelper.Game.Exceptions;

namespace TS3CallsignHelper.Game.Stores;
public class GameState {

  public event Action<string> CurrentAirplaneChanged;
  public event Action<string, PlaneState> PlaneStateChanged;
  public event Action<PlayerPosition, bool> ActivePositionsChanged;
  public string currentAirplane {
    get => _currentAirplane; set {
      if (_currentAirplane == value)
        return;
      _currentAirplane = value;
      CurrentAirplaneChanged?.Invoke(value);
    }
  }

  private string _currentAirplane;
  private Dictionary<string, PlaneState> _planeStates;
  private List<PlayerPosition> _activePositions;

  public GameState() {
    _currentAirplane = "";
    _planeStates = new Dictionary<string, PlaneState>();
    _activePositions = new List<PlayerPosition>();
  }

  /**
   * Gets the current state of the airplane
   * 
   * <param name="airplane">airplane</param>
   * <returns>state of <paramref name="airplane"/> or unknown if not yet assigned</returns>
   */
  public PlaneState GetPlaneState(string airplane) {
    return _planeStates.TryGetValue(airplane, out var state) ? state : PlaneState.UNKNOWN;
  }

  /**
   * Sets the state of the airplane
   * 
   * <param name="airplane">airplane</param>
   * <param name="state">state to set</param>
   * <exception cref="InvalidPlaneStateException">If <paramref name="state"/> is not valid for <paramref name="airplane"/></exception>
   */
  public void SetPlaneState(string airplane, PlaneState state) {
    if (!ValidatePlaneState(airplane, state))
      throw new InvalidPlaneStateException(airplane, state);
    _planeStates[airplane] = state;
    PlaneStateChanged?.Invoke(airplane, state);
  }

  /**
   * <param name="position">player position</param>
   * <returns>whether <paramref name="position"/> is marked as active</returns>
   */
  public bool IsOnPosition(PlayerPosition position) {
    return _activePositions.Contains(position);
  }

  /**
   * Sets the status of a position
   * 
   * <param name="position">position to change</param>
   * <param name="active">whether the position is active or not</param>
   */
  public void SetOnPosition(PlayerPosition position, bool active) {
    if (active != _activePositions.Contains(position)) {
      if (active)
        _activePositions.Add(position);
      else
        _activePositions.Remove(position);
      ActivePositionsChanged?.Invoke(position, active);
    }
  }

  /**
   * Validates the rough validity of a plane state
   * this takes into account the active positions and restrictions for initial contact
   * 
   * <param name="airplane">airplane</param>
   * <param name="state">state to validate</param>
   * 
   * <returns><c>true</c>, if the assignment is valid, <c>false</c> otherwise</returns>
   */
  private bool ValidatePlaneState(string airplane, PlaneState state) {
    if (_activePositions.Contains(PlayerPosition.Ground) && (state & PlaneState.IS_GND) == 0) return false;
    if (_activePositions.Contains(PlayerPosition.Tower) && (state & PlaneState.IS_TWR) == 0) return false;
    if (!_planeStates.ContainsKey(airplane) && (state & PlaneState.IS_INITIAL) == 0) return false;
    return true;
  }
}
