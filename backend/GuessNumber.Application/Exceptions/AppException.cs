namespace GuessNumber.Application.Exceptions;

//Thrown for expected business errors -> mapped to HTTP 400 by middleware
public class AppException : Exception
{
    public AppException(string message) : base(message) { }
}