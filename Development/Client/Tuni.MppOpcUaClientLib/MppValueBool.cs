
#nullable disable
namespace Tuni.MppOpcUaClientLib
{
  /// <summary>Boolean value.</summary>
  public class MppValueBool : MppValue
  {
    /// <summary>Constructor.</summary>
    /// <param name="v">Value.</param>
    internal MppValueBool(bool v)
      : base(MppValue.ValueTypeType.Bool)
    {
      this.Value = v;
    }

    /// <summary>Value.</summary>
    public bool Value { get; private set; }

    /// <summary>Gets the value.</summary>
    /// <returns>Value</returns>
    public override object GetValue() => (object) this.Value;
  }
}
