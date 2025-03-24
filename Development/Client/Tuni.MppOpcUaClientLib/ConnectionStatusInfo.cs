
using UnifiedAutomation.UaClient;

#nullable disable
namespace Tuni.MppOpcUaClientLib
{
  /// <summary>Connection status information.</summary>
  public class ConnectionStatusInfo
  {
    /// <summary>Constructor.</summary>
    internal ConnectionStatusInfo()
    {
      this.SimplifiedStatus = ConnectionStatusInfo.StatusType.Disconnected;
      this.FullStatusString = "";
    }

    /// <summary>Constructor.</summary>
    /// <param name="status">Status.</param>
    internal ConnectionStatusInfo(ServerConnectionStatus status)
    {
      this.FullStatusString = status.ToString();
      switch (status)
      {
        case ServerConnectionStatus.Disconnected:
        case ServerConnectionStatus.ConnectionWarningWatchdogTimeout:
        case ServerConnectionStatus.ServerShutdownInProgress:
        case ServerConnectionStatus.ServerShutdown:
        case ServerConnectionStatus.LicenseExpired:
          this.SimplifiedStatus = ConnectionStatusInfo.StatusType.Disconnected;
          break;
        case ServerConnectionStatus.Connected:
        case ServerConnectionStatus.SessionAutomaticallyRecreated:
          this.SimplifiedStatus = ConnectionStatusInfo.StatusType.Connected;
          break;
        case ServerConnectionStatus.ConnectionErrorClientReconnect:
        case ServerConnectionStatus.Connecting:
          this.SimplifiedStatus = ConnectionStatusInfo.StatusType.Connecting;
          break;
        default:
          this.SimplifiedStatus = ConnectionStatusInfo.StatusType.Unknown;
          break;
      }
    }

    /// <summary>
    /// There are various UA stack status types that are mapped to this for simplicity.
    /// </summary>
    public ConnectionStatusInfo.StatusType SimplifiedStatus { get; private set; }

    /// <summary>Full status string as received from the OPC UA stack.</summary>
    public string FullStatusString { get; private set; }

    /// <summary>
    /// There are various UA stack status types that are mapped to these for simplicity.
    /// </summary>
    public enum StatusType
    {
      /// <summary>Connection status is unknown.</summary>
      Unknown,
      /// <summary>Disconnected.</summary>
      Disconnected,
      /// <summary>Connected.</summary>
      Connected,
      /// <summary>Connecting or reconnecting.</summary>
      Connecting,
    }
  }
}
