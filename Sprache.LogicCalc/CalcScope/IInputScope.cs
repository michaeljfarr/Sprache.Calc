namespace Sprache.Calc.CalcScope
{
    /// <summary>
    /// The InputScope are the readonly inputs to the calculator scope.
    /// </summary>
    public interface IInputScope
    {
        object GetValue(string key);
    }
}