namespace Sprache.Calc.CalcScope
{
    /// <summary>
    /// The IMutableScope is the input to the calculator.  It allows the result of a calculations to be written
    /// back into it, allowing subsequent calculations to rely on previous ones.
    /// </summary>
    public interface IMutableScope : IInputScope
    {
        void StoreValue(string key, object value);
    }
}