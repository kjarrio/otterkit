namespace Otterkit.Types;

public readonly ref partial struct ErrorHandler
{
    public static ErrorType SuppressedError { private get; set; }
    internal static bool HasOccurred = false;

    private readonly ErrorType ErrorType;
    private readonly ConsoleColor ConsoleColor;


    public ErrorHandler(ErrorType errorType, ConsoleColor consoleColor)
    {
        ErrorType = errorType;
        ConsoleColor = consoleColor;
    }
}
