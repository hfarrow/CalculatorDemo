namespace CalculatorDemo
{
    public static class Result
    {
        public static Result<T> FromValue<T>(T value) => new Result<T>(value, true, null);
        public static Result<T> FromError<T>(string errorMessage) => new Result<T>(default(T), false, errorMessage);
    }
    
    public class Result<T>
    {
        public readonly T Value;
        public readonly bool HasValue;
        public readonly string ErrorMessage;

        public Result(T value, bool hasValue, string errorMessage)
        {
            Value = value;
            HasValue = hasValue;
            ErrorMessage = errorMessage;
        }
    }
}