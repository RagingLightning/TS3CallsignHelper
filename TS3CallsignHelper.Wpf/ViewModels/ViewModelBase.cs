using System;
using System.ComponentModel;
using System.Numerics;

namespace TS3CallsignHelper.Wpf.ViewModels;
public abstract class ViewModelBase : INotifyPropertyChanged {
  public abstract Type Translation { get; }
	public abstract double InitialWidth { get; }
  public abstract double InitialHeight { get; }
  public string? TranslationAssembly => Translation.Assembly.FullName;
  public string? TranslationDictionary => Translation.Name;

	private double _scale = 1;
	public double Scale {
		get {
			return _scale;
		}
		set {
			_scale = value;
			OnPropertyChanged(nameof(Scale));
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;

  protected void OnPropertyChanged(string propertyName) {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }

  public abstract void Dispose();
}
