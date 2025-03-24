
using System;

#nullable disable
namespace Tuni.MppOpcUaClientLib
{
  /// <summary>Holds connection parameters.</summary>
  public class ConnectionParamsHolder
  {
    private int m_connectionTimeout_ms;

    /// <summary>Constructor.</summary>
    /// <param name="url">OPC UA server URL.</param>
    public ConnectionParamsHolder(string url)
    {
      this.ServerUrl = url;
      this.m_connectionTimeout_ms = 10000;
      this.Plc1Namespace = "urn:CX-19788E:BeckhoffAutomation:Ua:PLC1";
    }

    /// <summary>OPC UA server URL.</summary>
    public string ServerUrl { get; private set; }

    /// <summary>The namespace of the PLC1 node in the OPC UA server.</summary>
    public string Plc1Namespace { get; set; }

    /// <summary>Connection timeout in milliseconds.</summary>
    public int ConnectionTimeout_ms
    {
      get => this.m_connectionTimeout_ms;
      set
      {
        this.m_connectionTimeout_ms = value >= 0 ? value : throw new ArgumentException("Invalid negative timeout given");
      }
    }
  }
}
