
#nullable disable
namespace Tuni.MppOpcUaClientLib
{
  /// <summary>Represents a value on the server.</summary>
  public abstract class MppValue
  {
    /// <summary>Constructor.</summary>
    protected MppValue(MppValue.ValueTypeType vt) => this.ValueType = vt;

    /// <summary>The type of the value.</summary>
    public MppValue.ValueTypeType ValueType { get; private set; }

    /// <summary>Returns the value.</summary>
    /// <returns>Value.</returns>
    public abstract object GetValue();

    /// <summary>Specifies the type of the value.</summary>
    public enum ValueTypeType
    {
      /// <summary>Boolean.</summary>
      Bool,
      /// <summary>Double.</summary>
      Double,
      /// <summary>Integer.</summary>
      Int,
    }
  }
}
