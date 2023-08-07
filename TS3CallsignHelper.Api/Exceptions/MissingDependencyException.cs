using System;

namespace TS3CallsignHelper.API.Exceptions;

/// <summary>
/// Indicates that a required dependency was not available
/// </summary>
public class MissingDependencyException : Exception {

  public Type DependencyType { get; }

  public MissingDependencyException(Type dependencyType) : base($"Dependency of type {dependencyType} not available") {
    DependencyType = dependencyType;
  }
}
