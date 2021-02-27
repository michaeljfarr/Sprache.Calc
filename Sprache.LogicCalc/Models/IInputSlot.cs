using System;

namespace Sprache.Calc.Models
{
    /// <summary>
    /// IInputSlot are an abstraction of named variables that higher level components may depend on.  IInputSlots can be converted into IInputs by
    /// providing a value; provided the IInputSlots accepts the value (as defined by the CanAssign method).  A special case of an IInputSlot is an
    /// IInputSlotAndValue which is defined in combination with a value.
    /// .
    /// </summary>
    /// <remarks>
    /// Command Variables are stored with the command like this:
    ///     {Name, Description, TemplateRef, Params:[{ParamUri, Value}]}
    /// The command Uri may be associated define certain defaults that will be provided to the command at execution time.
    /// Command Prototypes are accessible like this
    ///    {TemplateRef, Variables:[ParamUri]}
    /// </remarks>
    public interface IInputSlot
    {
        string Name { get; }
        bool CanAssign(object value);
        Uri Uri();

        ///// <summary>
        ///// Convert the value into a parameter definition that can be serialized into a workflow definition.
        ///// </summary>
        ///// <param name="value"></param>
        ///// <returns></returns>
        //IInput ToInput(object value);


        /// <summary>
        /// Applies a value to the IInputSlot so it can be serialized into an executable workflow definition.
        /// </summary>
        /// <remarks>
        /// Used when converting the ActivityTemplate into an ActivityPrototype
        /// </remarks>
        IInputSlotAndValue ToConstant(object value);
    }
}