
using System;
using System.Collections.Generic;

#nullable disable
namespace Tuni.MppOpcUaClientLib
{
  /// <summary>Event arguments for ProcessItemChanged.</summary>
  public class ProcessItemChangedEventArgs : EventArgs
  {
    /// <summary>Constructor.</summary>
    /// <param name="items">The items that have changed in the process and their values.</param>
    internal ProcessItemChangedEventArgs(Dictionary<string, MppValue> items)
    {
      this.ChangedItems = items;
    }

    /// <summary>
    /// The items that have changed in the process and their values.
    /// Note that this *may* or *may not* contain all the items currently in the subscription.
    /// </summary>
    public Dictionary<string, MppValue> ChangedItems { get; private set; }
  }
}
