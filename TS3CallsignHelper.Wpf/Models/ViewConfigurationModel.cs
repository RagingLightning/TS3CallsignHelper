﻿using TS3CallsignHelper.Wpf.Commands;

namespace TS3CallsignHelper.Wpf.Models;
internal class ViewConfigurationModel {
  public string Name { get; }
  public AddViewModelCommand Creator { get; }
  public ViewConfigurationModel(string name, AddViewModelCommand creator) {
    Name = name;
    Creator = creator;
  }
}
