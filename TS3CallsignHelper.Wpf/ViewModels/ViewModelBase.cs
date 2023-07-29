using System.ComponentModel;

namespace TS3CallsignHelper.Wpf.ViewModels;
internal abstract class ViewModelBase : INotifyPropertyChanged {
  public abstract string Name { get; }

  public event PropertyChangedEventHandler? PropertyChanged;

  protected void OnPropertyChanged(string propertyName) {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }

  public abstract void Dispose();
}
