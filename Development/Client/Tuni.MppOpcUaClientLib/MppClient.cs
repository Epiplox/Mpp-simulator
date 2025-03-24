
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnifiedAutomation.UaBase;
using UnifiedAutomation.UaClient;

#nullable disable
namespace Tuni.MppOpcUaClientLib
{
  /// <summary>
  /// Implements an OPC UA client for the mini pulp process (MPP).
  /// </summary>
  public class MppClient : IDisposable
  {
    private const string NodeIdPrefix = "eq_states.EQ_";
    private readonly string m_serverUrl;
    private readonly string m_plc1Namespace;
    private readonly int m_connectionTimeoutMs;
    private readonly MppClient.StatefulSynchroniser m_synchroniser = new MppClient.StatefulSynchroniser();

    /// <summary>An event raised when an item changes in the process.</summary>
    public event MppClient.ProcessItemsChangedEventHandler ProcessItemsChanged;

    /// <summary>An event raised when the connection status changes.</summary>
    public event MppClient.ConnectionStatusEventHandler ConnectionStatus;

    /// <summary>Constructor.</summary>
    /// <param name="connParams">Connection parameter.</param>
    public MppClient(ConnectionParamsHolder connParams)
    {
      this.m_serverUrl = connParams.ServerUrl;
      this.m_plc1Namespace = connParams.Plc1Namespace;
      this.m_connectionTimeoutMs = connParams.ConnectionTimeout_ms;
    }

    /// <summary>Disposes the object.</summary>
    public void Dispose()
    {
      try
      {
        new Task((Action) (() => this.m_synchroniser.DisposeUaConnection())).Start();
      }
      catch
      {
      }
    }

    /// <summary>
    /// Initializes the object. This function may only be called once for each object.
    /// </summary>
    /// <exception cref="T:System.InvalidOperationException">Thrown if the object state does not permit the action or if another failure occurs.</exception>
    /// <exception cref="T:System.TimeoutException">Thrown if the connection times out.</exception>
    public void Init()
    {
      try
      {
        this.m_synchroniser.StartInit();
        Session session = this.InitSession();
        ushort nsIndex = this.ResolveNamespaceIndex(session);
        Subscription subscriptionObject = this.CreateSubscriptionObject(session);
        this.m_synchroniser.FinishInit(session, subscriptionObject, nsIndex);
      }
      catch (InvalidOperationException ex)
      {
        throw new InvalidOperationException("Init function can only be called once; to reconnect, create another client object", (Exception) ex);
      }
    }

    /// <summary>
    /// Adds a process item to the subscription list so notifications will be received about its value changes.
    /// </summary>
    /// <param name="name">Item name.</param>
    /// <exception cref="T:System.ArgumentException">Thrown if an invalid argument has been given.</exception>
    /// <exception cref="T:System.InvalidOperationException">Thrown if the object state does not permit the action.</exception>
    public void AddToSubscription(string name)
    {
      name = this.ReplacePlusAndMinusInNames_toServer(name);
      List<StatusCode> monitoredItems = this.m_synchroniser.Subscription.CreateMonitoredItems((IList<MonitoredItem>) new List<MonitoredItem>()
      {
        (MonitoredItem) new DataMonitoredItem(new NodeId("eq_states.EQ_" + name, this.m_synchroniser.NamespaceIndex))
      });
      this.CheckOperationStatusCode(name, monitoredItems[0]);
    }

    /// <summary>Sets the status of an on/off item.</summary>
    /// <param name="name">Item name.</param>
    /// <param name="value">True for on, otherwise false.</param>
    /// <exception cref="T:System.ArgumentException">Thrown if an invalid argument has been given.</exception>
    /// <exception cref="T:System.InvalidOperationException">Thrown if the object state does not permit the action.</exception>
    public void SetOnOffItem(string name, bool value)
    {
      UnifiedAutomation.UaBase.Variant variant = new UnifiedAutomation.UaBase.Variant(value);
      this.SetValueAtServer(name, variant);
    }

    /// <summary>Sets the opening of a control valve.</summary>
    /// <param name="name">Valve name.</param>
    /// <param name="opening">The opening to be set. 0 is fully closed, 100 is fully open.</param>
    /// <exception cref="T:System.ArgumentException">Thrown if an invalid argument has been given.</exception>
    /// <exception cref="T:System.InvalidOperationException">Thrown if the object state does not permit the action.</exception>
    public void SetValveOpening(string name, int opening)
    {
      UnifiedAutomation.UaBase.Variant variant = new UnifiedAutomation.UaBase.Variant(this.ConvertIntAndCheckRange_0_to_100(name, opening));
      this.SetValueAtServer(name, variant);
    }

    /// <summary>Sets pump status.</summary>
    /// <param name="name">Pump name.</param>
    /// <param name="power">Power to be set. 0 for off, 100 for maximum.</param>
    /// <exception cref="T:System.ArgumentException">Thrown if an invalid argument has been given.</exception>
    /// <exception cref="T:System.InvalidOperationException">Thrown if the object state does not permit the action.</exception>
    public void SetPumpControl(string name, int power)
    {
      UnifiedAutomation.UaBase.Variant variant = new UnifiedAutomation.UaBase.Variant(this.ConvertIntAndCheckRange_0_to_100(name, power));
      this.SetValueAtServer(name, variant);
    }

    private void CheckOperationStatusCode(string name, StatusCode status)
    {
      switch (status.Code)
      {
        case 0:
          break;
        case 2150891520:
          throw new ArgumentException(string.Format("{0}: bad node ID ({1} {2})", (object) name, (object) status.Code, (object) status.Message));
        case 2155085824:
          throw new ArgumentException(string.Format("{0}: bad type ({1} {2})", (object) name, (object) status.Code, (object) status.Message));
        default:
          throw new InvalidOperationException(string.Format("{0}: operation failed ({1} {2})", (object) name, (object) status.Code, (object) status.Message));
      }
    }

    private string ReplacePlusAndMinusInNames_toServer(string input)
    {
      input = input.Replace("+", "_PLUS_");
      input = input.Replace("-", "_MINUS_");
      return input;
    }

    private string ReplacePlusAndMinusInNames_fromServer(string input)
    {
      input = input.Replace("_PLUS_", "+");
      input = input.Replace("_MINUS_", "-");
      return input;
    }

    private Session InitSession()
    {
      ApplicationInstanceBase app = ApplicationInstanceBase.Default;
      app.AutoCreateCertificate = true;
      Session session = new Session(app)
      {
        UseDnsNameAndPortFromDiscoveryUrl = false,
        DisableEndpointValidation = true
      };
      session.ConnectionStatusUpdate += new ServerConnectionStatusUpdateEventHandler(this.M_session_ConnectionStatusUpdate);
      RequestSettings settings = new RequestSettings()
      {
        OperationTimeout = this.m_connectionTimeoutMs
      };
      try
      {
        session.Connect(this.m_serverUrl, SecuritySelection.None, settings);
      }
      catch (StatusException ex)
      {
        throw new InvalidOperationException("Failed to connect", (Exception) ex);
      }
      return session;
    }

    private ushort ResolveNamespaceIndex(Session uaSession)
    {
      for (int index = 0; index < uaSession.NamespaceUris.Count; ++index)
      {
        if (uaSession.NamespaceUris[index] == this.m_plc1Namespace)
          return (ushort) index;
      }
      throw new InvalidOperationException("Cannot find the expected namespace in the server: " + this.m_plc1Namespace);
    }

    private Subscription CreateSubscriptionObject(Session session)
    {
      Subscription subscriptionObject = new Subscription(session);
      subscriptionObject.PublishingInterval = 50.0;
      subscriptionObject.DataChanged += new DataChangedEventHandler(this.Subscription_DataChanged);
      subscriptionObject.Create();
      return subscriptionObject;
    }

    private void M_session_ConnectionStatusUpdate(
      Session sender,
      ServerConnectionStatusUpdateEventArgs args)
    {
      ConnectionStatusInfo st = new ConnectionStatusInfo(args.Status);
      try
      {
        this.ConnectionStatus((object) this, new ConnectionStatusEventArgs(st));
      }
      catch
      {
      }
    }

    private void Subscription_DataChanged(Subscription subscription, DataChangedEventArgs args)
    {
      try
      {
        Dictionary<string, MppValue> items = new Dictionary<string, MppValue>();
        foreach (DataChange dataChange in args.DataChanges)
        {
          try
          {
            string identifier = (string) dataChange.MonitoredItem.NodeId.Identifier;
            if (identifier.StartsWith("eq_states.EQ_"))
            {
              string key = this.ReplacePlusAndMinusInNames_fromServer(identifier.Substring("eq_states.EQ_".Length));
              switch (dataChange.Value.Value)
              {
                case ushort num:
                  items[key] = (MppValue) new MppValueInt(Convert.ToInt32(num));
                  continue;
                case bool v2:
                  items[key] = (MppValue) new MppValueBool(v2);
                  continue;
                case double v3:
                    items[key] = (MppValue) new MppValueDouble(v3);
                    continue;
                default:
                  continue;
              }
            }
          }
          catch (Exception ex)
          {
          }
        }
        this.ProcessItemsChanged((object) this, new ProcessItemChangedEventArgs(items));
      }
      catch (Exception ex)
      {
      }
    }

    private ushort ConvertIntAndCheckRange_0_to_100(string name, int input)
    {
      return input >= 0 && input <= 100 ? (ushort) input : throw new ArgumentException("\"" + name + "\" is out of range: " + (object) input);
    }

    private void SetValueAtServer(string name, UnifiedAutomation.UaBase.Variant variant)
    {
      string id = "eq_states.EQ_" + name;
      WriteValueCollection writeValueCollection = new WriteValueCollection();
      ((List<WriteValue>) writeValueCollection).Add(new WriteValue()
      {
        NodeId = new NodeId(id, this.m_synchroniser.NamespaceIndex),
        AttributeId = 13U,
        Value = new DataValue(variant)
        {
          ServerTimestamp = new DateTime(0L),
          SourceTimestamp = new DateTime(0L)
        }
      });
      List<StatusCode> statusCodeList = this.m_synchroniser.Session.Write((IList<WriteValue>) writeValueCollection);
      this.CheckOperationStatusCode(name, statusCodeList[0]);
    }

    /// <summary>Event handler delegate for ProcessItemChanged.</summary>
    /// <param name="source">Event source.</param>
    /// <param name="args">Event arguments.</param>
    public delegate void ProcessItemsChangedEventHandler(
      object source,
      ProcessItemChangedEventArgs args);

    /// <summary>Event handler delegate for ConnectionStatus.</summary>
    /// <param name="source">Source.</param>
    /// <param name="args">Event arguments.</param>
    public delegate void ConnectionStatusEventHandler(object source, ConnectionStatusEventArgs args);

    /// <summary>
    /// Ensures that items are synchronised between threads. Also maintains state information.
    /// </summary>
    private class StatefulSynchroniser
    {
      private readonly object m_lockObject = new object();
      private MppClient.StatefulSynchroniser.StateType m_stateValue;
      private Subscription m_uaSubscription;
      private Session m_uaSession;
      private ushort m_uaNamespaceIndex = ushort.MaxValue;

      /// <summary>Sets the state to "initialising".</summary>
      /// <exception cref="T:System.InvalidOperationException">Thrown if the object state does not allow the operation.</exception>
      public void StartInit()
      {
        lock (this.m_lockObject)
        {
          this.AssertStateValue(MppClient.StatefulSynchroniser.StateType.Created);
          this.m_stateValue = MppClient.StatefulSynchroniser.StateType.Initializing;
        }
      }

      /// <summary>Sets the state to "initialised".</summary>
      /// <param name="sess">Session.</param>
      /// <param name="subs">Subscription.</param>
      /// <param name="nsIndex">Namespace index.</param>
      /// <exception cref="T:System.InvalidOperationException">Thrown if the object state does not allow the operation.</exception>
      public void FinishInit(Session sess, Subscription subs, ushort nsIndex)
      {
        lock (this.m_lockObject)
        {
          this.AssertStateValue(MppClient.StatefulSynchroniser.StateType.Initializing);
          this.m_stateValue = MppClient.StatefulSynchroniser.StateType.Initialized;
          this.m_uaSession = sess;
          this.m_uaSubscription = subs;
          this.m_uaNamespaceIndex = nsIndex;
        }
      }

      /// <summary>Sets the state to "disposed".</summary>
      public void DisposeUaConnection()
      {
        bool flag1 = false;
        bool flag2 = false;
        lock (this.m_lockObject)
        {
          this.m_stateValue = MppClient.StatefulSynchroniser.StateType.Disposed;
          flag1 = this.m_uaSubscription == null;
          flag2 = this.m_uaSession == null;
        }
        if (!flag1)
        {
          try
          {
            this.m_uaSubscription.Delete();
            lock (this.m_lockObject)
              this.m_uaSubscription = (Subscription) null;
          }
          catch
          {
          }
        }
        if (flag2)
          return;
        try
        {
          this.m_uaSession.Disconnect();
          this.m_uaSession.Dispose();
          lock (this.m_lockObject)
            this.m_uaSession = (Session) null;
        }
        catch
        {
        }
      }

      /// <summary>Session object.</summary>
      /// <exception cref="T:System.InvalidOperationException">Thrown if the state does not allow the member to be utilised.</exception>
      public Session Session
      {
        get
        {
          lock (this.m_lockObject)
          {
            this.AssertStateValue(MppClient.StatefulSynchroniser.StateType.Initialized);
            return this.m_uaSession;
          }
        }
      }

      /// <summary>Subscription object.</summary>
      /// <exception cref="T:System.InvalidOperationException">Thrown if the state does not allow the member to be utilised.</exception>
      public Subscription Subscription
      {
        get
        {
          lock (this.m_lockObject)
          {
            this.AssertStateValue(MppClient.StatefulSynchroniser.StateType.Initialized);
            return this.m_uaSubscription;
          }
        }
      }

      /// <summary>The relevant namespace index in the UA server.</summary>
      /// <exception cref="T:System.InvalidOperationException">Thrown if the state does not allow the member to be utilised.</exception>
      public ushort NamespaceIndex
      {
        get
        {
          lock (this.m_lockObject)
          {
            this.AssertStateValue(MppClient.StatefulSynchroniser.StateType.Initialized);
            return this.m_uaNamespaceIndex;
          }
        }
      }

      private void AssertStateValue(MppClient.StatefulSynchroniser.StateType expected)
      {
        if (this.m_stateValue != expected)
          throw new InvalidOperationException("Unexpected state of client object");
      }

      /// <summary>Indicates client state.</summary>
      public enum StateType
      {
        Created,
        Initializing,
        Initialized,
        Disposed,
      }
    }
  }
}
