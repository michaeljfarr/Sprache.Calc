namespace Sprache.Calc.Models
{
    /// <summary>
    /// An IInputSlotAndValue is an IInputSlot with an inherent value.  This value can be either:
    ///  - the default value, which can be overridden or
    ///  - a fixed value which is inherent to the slot.  
    /// </summary>
    /// <remarks>
    /// An ActivityTemplate is perfectly represented by a URI.
    /// 
    /// An IInputSlotAndValue is different than an IInput in the following ways:
    ///  - an IInputSlotAndValue is a parameter specification of an ActivityTemplate
    ///      - this is an ordered list of slots, that can receive an ordered list (or by name) of values to be populated.
    ///  - an IInputSlotAndValue knows the .NET type, so can serialize and deserialize complex types from configuration
    ///  - an IInput is just absolute key/value pair whereas a IInputSlotAndValue's value can be interpreted from a property on another object.
    ///
    /// An ActivityTemplate is a non-executable but Activity Definition, that can be converted into an ActivityPrototype, it has
    ///     - a URI/name
    ///     - an ordered/named set of IInputSlot (some of which may have values)
    /// An ActivityPrototype is a ActivityTemplate that is ready to be executed, it has
    ///     - a Template URI/name
    ///     - an ordered/named set of IInputSlotAndValue (some of which may have values)
    /// </remarks>
    public interface IInputSlotAndValue : IInputSlot
    {
        object Value { get; }
    }
}