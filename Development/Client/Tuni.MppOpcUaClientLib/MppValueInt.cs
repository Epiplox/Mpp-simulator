
#nullable disable
namespace Tuni.MppOpcUaClientLib
{
  /// <summary>Integer value.</summary>
  public class MppValueInt : MppValue
  {
    /// <summary>Constructor.</summary>
    /// <param name="v">Value.</param>
    internal MppValueInt(int v)
      : base(MppValue.ValueTypeType.Int)
    {
      this.Value = v;
    }

    /// <summary>Value.</summary>
    public int Value { get; private set; }

    /// <summary>Gets the value.</summary>
    /// <returns>Value</returns>
    public override object GetValue() => (object) this.Value;
  }
}
