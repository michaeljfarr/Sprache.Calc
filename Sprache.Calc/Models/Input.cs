namespace Sprache.Calc.Models
{
    public class Input : IInput 
    {
        public Input()
        {

        }
        public Input(string name, string value)
        {
            Name = name;
            Value = value;
        }
        public string Name { get; set; }
        /// <summary>
        /// The value can either be the actual typed object, or a JObject that can be deserialized into the type used by the ParameterUri.
        /// </summary>
        public object Value { get; set; }
    }
}