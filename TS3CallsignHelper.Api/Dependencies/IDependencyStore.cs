namespace TS3CallsignHelper.Api.Dependencies;

/// <summary>
/// Helper class to wrap all available Dependencies
/// </summary>
public interface IDependencyStore {

  /// <summary>
  /// Adds a dependency
  /// </summary>
  /// <typeparam name="T">Type of dependency</typeparam>
  /// <param name="instance">Instance of dependency</param>
  /// <returns>the instance passed in</returns>
  public T Add<T>(T instance);

  public T? TryGet<T>();

}
