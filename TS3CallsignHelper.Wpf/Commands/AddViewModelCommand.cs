﻿using System;
using TS3CallsignHelper.Wpf.ViewModels;

namespace TS3CallsignHelper.Wpf.Commands;
public class AddViewModelCommand : CommandBase {

  private readonly MainViewModel _mainViewModel;
  private Func<ViewModelBase> _creator;
  public AddViewModelCommand(MainViewModel mainViewModel, Func<ViewModelBase> creator) {
    _mainViewModel = mainViewModel;
    _creator = creator;
  }
  public override void Execute(object? parameter) {
    _mainViewModel.AddView(_creator());
    _mainViewModel.ViewSelectorOpen = false;
  }
}
