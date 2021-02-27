namespace Sprache.Calc.CalcScope.Stores
{
    public class EmptyInputScope : IInputScope
    {
        public static EmptyInputScope Instance => new EmptyInputScope();
        public object GetValue(string key)
        {
            return null;
        }
    }
}