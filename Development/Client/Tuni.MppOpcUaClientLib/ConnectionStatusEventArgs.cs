
using System;

#nullable disable
namespace Tuni.MppOpcUaClientLib
{
  /// <summary>
  /// Used to notify about connection status changes. Inheritance from the EventArgs base class is used because of the C# events convention.
  /// </summary>
  public class ConnectionStatusEventArgs : EventArgs
  {
    /// <summary>Constructor.</summary>
    /// <param name="st"></param>
    internal ConnectionStatusEventArgs(ConnectionStatusInfo st) => this.StatusInfo = st;

    /// <summary>Status information.</summary>
    public ConnectionStatusInfo StatusInfo { get; private set; }
  }
}
