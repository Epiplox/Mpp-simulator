
#nullable disable
namespace Tuni.MppOpcUaClientLib
{
  /// <summary>Double value.</summary>
  public class MppValueDouble : MppValue
  {
    /// <summary>Constructor.</summary>
    /// <param name="v">Value.</param>
    internal MppValueDouble(double v)
      : base(MppValue.ValueTypeType.Double)
    {
      this.Value = v;
    }

    /// <summary>Value.</summary>
    public double Value { get; private set; }

    /// <summary>Gets the value.</summary>
    /// <returns>Value</returns>
    public override object GetValue() => (object) this.Value;
  }
}
