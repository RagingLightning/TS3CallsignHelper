using System;
using System.ComponentModel;

namespace TS3CallsignHelper.Wpf.ViewModels;
public abstract class ViewModelBase : INotifyPropertyChanged {
  public abstract Type Translation { get; }

  public string? TranslationAssembly => Translation.Assembly.FullName;
  public string? TranslationDictionary => Translation.Name;

  public event PropertyChangedEventHandler? PropertyChanged;

  protected void OnPropertyChanged(string propertyName) {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }

  public abstract void Dispose();
}
