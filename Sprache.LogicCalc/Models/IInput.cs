namespace Sprache.Calc.Models
{
    /// <summary>
    /// Inputs are named values that may be referenced in the calculator
    /// </summary>
    public interface IInput
    {
        /// <summary>
        /// The calculator can reference any input in its scope using this name, which can be anything that the parser accepts as a name.
        /// However, some value providers can also interpret deep references into the value (if it is a complex object).
        /// </summary>
        string Name { get; }
        /// <summary>
        /// The value can either be the actual typed object, or a JObject that can be deserialized into the type used by the ParameterUri.
        /// </summary>
        object Value { get;  }
    }
}