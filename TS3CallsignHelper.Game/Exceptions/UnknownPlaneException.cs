using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS3CallsignHelper.Game.Exceptions;
internal class UnknownPlaneException : Exception {
  public UnknownPlaneException(string? message) : base(message) {
  }
}
