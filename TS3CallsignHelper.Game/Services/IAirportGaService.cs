﻿using System.Collections.Immutable;
using TS3CallsignHelper.Api;

namespace TS3CallsignHelper.Game.Services;
public interface IAirportGaService {
  public abstract ImmutableDictionary<string, AirportGa> Load(string installation, string airport, string database,
    ImmutableDictionary<string, AirportAirplane> airplanes);
}