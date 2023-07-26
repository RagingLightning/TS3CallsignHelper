using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS3CallsignHelper.Game.Stores;
public class GameState {

  public event Action<string> CurrentAirplaneChanged;
  public string currentAirplane {
    get => _currentAirplane; set {
      if (_currentAirplane == value)
        return;
      _currentAirplane = value;
      CurrentAirplaneChanged?.Invoke(value);
    }
  }

  private string _currentAirplane;
}
