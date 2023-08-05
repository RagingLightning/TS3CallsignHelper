using System;
using System.ComponentModel;
using TS3CallsignHelper.Api.Dependencies;

namespace TS3CallsignHelper.Api;
public abstract class IViewModel : INotifyPropertyChanged {
  public abstract Type Translation { get; }
	public abstract Type View { get; }
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

	public virtual IViewModel Initialize(IDependencyStore dependencyStore) { return this; }

	public virtual void Dispose() { }
}
