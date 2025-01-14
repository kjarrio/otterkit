using Otterkit.Types;

namespace Otterkit.Analyzers;

public static class TokenHandling
{
    /// <summary>
    /// Used for keeping track of the current token list index.
    /// </summary>  
    public static int Index;
    public static List<Token> Source => CompilerContext.SourceTokens;
    
    // Analyzer token handling methods.
    // These are the main methods used to interact with and iterate through the List of Tokens.
    // (And other helpers that it easier to iterate through the List)
    // All other methods inside of the analyzer depend on these to parse through the tokens.
    public static bool PictureString(ReadOnlySpan<char> picture, out int size)
    {
        var set = new HashSet<char>();
        var isAfterDecimalPoint = false;
        var dataSize = 0;
        var isValid = true;

        for (var index = 0; index < picture.Length; index++)
        {
            var character = char.ToUpperInvariant(picture[index]);

            if (character is 'S' && !set.IsEmpty())
            {
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 20, """
                    Invalid picture clause character string.
                    """)
                .WithSourceLine(Lookahead(-1))
                .WithNote("""
                    Symbol 'S' must be the first symbol of the picture string.
                    """)
                .CloseError();

                isValid = false;
            }

            if(character is 'N' && set.ContainsAny('9', 'A', 'X', 'S', 'V', 'P', '1', 'E'))
            {
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 20, """
                    Invalid picture clause character string.
                    """)
                .WithSourceLine(Lookahead(-1))
                .WithNote("""
                    Symbol 'N' must not follow any symbols other than 'N'.
                    """)
                .CloseError();

                isValid = false;
            }

            if(character is '1' && set.ContainsAny('9', 'A', 'X', 'S', 'V', 'P', 'N', 'E'))
            {
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 20, """
                    Invalid picture clause character string.
                    """)
                .WithSourceLine(Lookahead(-1))
                .WithNote("""
                    Symbol '1' must not follow any symbols other than '1'.
                    """)
                .CloseError();

                isValid = false;
            }

            if(character is 'A' or 'X' && set.ContainsAny('S', 'V', 'P', '1', 'N', 'E'))
            {
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 20, """
                    Invalid picture clause character string.
                    """)
                .WithSourceLine(Lookahead(-1))
                .WithNote("""
                    Symbols 'A' and 'X' may only follow the symbols '9', 'A' or 'X'.
                    """)
                .CloseError();

                isValid = false;
            }

            if (character is 'V' && set.Contains('V'))
            {
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 20, """
                    Invalid picture clause character string.
                    """)
                .WithSourceLine(Lookahead(-1))
                .WithNote("""
                    Symbol 'V' may only appear once in the same picture string.
                    """)
                .CloseError();

                isValid = false;
            }

            if (character is 'V' or 'P' && set.ContainsAny('A', 'X', '1', 'N', 'E'))
            {
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 20, """
                    Invalid picture clause character string.
                    """)
                .WithSourceLine(Lookahead(-1))
                .WithNote("""
                    Symbols 'V' and 'P' must not follow the symbols 'A', 'X', '1', 'N' or 'E'.
                    """)
                .CloseError();

                isValid = false;
            }

            if (character is 'V' && !set.Contains('V')) isAfterDecimalPoint = true;
            
            if (character is 'P' && set.ContainsAny('9', 'P') && isAfterDecimalPoint)
            {
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 20, """
                    Invalid picture clause character string.
                    """)
                .WithSourceLine(Lookahead(-1))
                .WithNote("""
                    Symbol 'P' or a string of 'P' may only appear once in a picture clause.
                    """)
                .CloseError();

                isValid = false;
            }

            if (character is 'P' && !set.IsEmpty() && set.Contains('9') && !isAfterDecimalPoint)
            {
                while (picture[index] is 'P' or 'p')
                {
                    if (index >= picture.Length - 1) break;

                    index++; 
                    dataSize++;
                }
            }

            if (character is 'P' && (set.IsEmpty() || !set.Contains('9') || isAfterDecimalPoint))
            {
                while (picture[index] is 'P' or 'p')
                {
                    if (index >= picture.Length - 1) break;

                    index++; 
                    dataSize++;
                }

                isAfterDecimalPoint = true;
            }

            if (character is '(')
            {
                var start = index;

                while (picture[index] != ')') index++;

                var end = index;

                var count = int.Parse(picture.Slice(start + 1, end - start - 1));

                dataSize += count - 1;

                continue;
            }

            set.Add(character);

            dataSize++;
        }
        
        size = dataSize;
        return isValid;
    }

    public static void AnchorPoint(params string[] anchors)
    {
        while (!CurrentEquals(TokenType.EOF))
        {
            if (CurrentEquals("."))
            {
                Continue();
                return;
            }

            if (CurrentEquals(anchors))
            {
                return;
            }

            Continue();
        }
    }

    public static void AnchorPoint(TokenContext anchor)
    {
        while (!CurrentEquals(TokenType.EOF))
        {
            if (CurrentEquals("."))
            {
                Continue();
                return;
            }

            if (CurrentEquals(anchor))
            {
                return;
            }

            Continue();
        }
    }

    public static void AnchorPoint(TokenContext anchor, params string[] anchors)
    {
        while (!CurrentEquals(TokenType.EOF))
        {
            if (CurrentEquals("."))
            {
                Continue();
                return;
            }

            if (CurrentEquals(anchors) || CurrentEquals(anchor))
            {
                return;
            }

            Continue();
        }
    }

    /// <summary>
    /// Token <c>Lookahead</c>: This method returns a Token from an index of Current Index + the amount parameter
    /// <para>When passed a positive amount it will act as a lookahead method, and when passed a negative amount it will act as a lookbehind method</para>
    /// <para>Technically this method allows for infinite lookahead and lookbehind, as long as the Index + amount is not bigger than the
    /// number of items on the list of Tokens or smaller than 0.</para>
    /// </summary>
    public static Token Lookahead(int amount)
    {
        if (Index + amount >= Source.Count) return Source[^1];

        return Index + amount < 0 ? Source[0] : Source[Index + amount];
    }

    /// <summary>
    /// <c>LookaheadEquals</c>: This method returns true or false depending on if the Token value from an index of Current Index + the first parameter is equal to the values of the second paramenter
    /// <para>When passed a positive amount it will act as a lookahead comparison method, and when passed a negative amount it will act as a lookbehind comparison method</para>
    /// <para>Technically this method allows for infinite lookahead and lookbehind comparisons, as long as the Index + amount is not bigger than the
    /// number of items on the list of Tokens or smaller than 0.</para>
    /// </summary>
    public static bool LookaheadEquals(int lookahead, string value)
    {
        if (Lookahead(lookahead).Value.Equals(value, StringComparison.OrdinalIgnoreCase)) return true;

        return false;
    }

    public static bool LookaheadEquals(int lookahead, params string[] valuesToCompare)
    {
        foreach (var value in valuesToCompare)
        {
            if (Lookahead(lookahead).Value.Equals(value, StringComparison.OrdinalIgnoreCase)) return true;
        }

        return false;
    }

    /// <summary>
    /// Boolean <c>LookaheadEquals</c> TokenType[]: This method returns true or false depending on if the Token type from an index of Current Index + the first parameter is equal to the types of the second paramenter
    /// <para>When passed a positive amount it will act as a lookahead comparison method, and when passed a negative amount it will act as a lookbehind comparison method</para>
    /// <para>Technically this method allows for infinite lookahead and lookbehind comparisons, as long as the Index + amount is not bigger than the
    /// number of items on the list of Tokens or smaller than 0.</para>
    /// </summary>
    public static bool LookaheadEquals(int lookahead, params TokenType[] tokenTypesToCompare)
    {
        foreach (var type in tokenTypesToCompare)
        {
            if (Lookahead(lookahead).Type.Equals(type)) return true;
        }

        return false;
    }

    /// <summary>
    /// Token <c>Current</c>: This method returns the current token from the current Index.
    /// <para>The returned token in encapsulated inside of the Token type, which holds not only the value of the token but also some additional context
    /// that might be required by the parser in certain situations</para>
    /// </summary>
    public static Token Current()
    {
        return Source[Index];
    }

    public static bool CurrentEquals(string value)
    {
        if (Current().Value.Equals(value, StringComparison.OrdinalIgnoreCase)) return true;
        
        return false;
    }

    /// <summary>
    /// Boolean <c>CurrentEquals</c> string[]: This method returns true or false depending on if the current token from the current Index has the same value as the parameter.
    /// <para>This helper method is an alternative to the <c>"Current().value.Equals()"</c> syntax, which could become verbose and harder to read when it's used frequently</para>
    /// </summary>
    public static bool CurrentEquals(params string[] valuesToCompare)
    {
        foreach (var value in valuesToCompare)
        {
            if (Current().Value.Equals(value, StringComparison.OrdinalIgnoreCase)) return true;
        }

        return false;
    }

    public static bool CurrentEquals(TokenType tokenType)
    {
        if (Current().Type == tokenType) return true;
        
        return false;
    }

    /// <summary>
    /// Boolean <c>CurrentEquals</c> TokenType[]: This method returns true or false depending on if the current token from the current Index has the same type as the parameter.
    /// <para>This helper method is an alternative to the <c>"Current().value.Equals()"</c> syntax, which could become verbose and harder to read when it's used frequently</para>
    /// </summary>
    public static bool CurrentEquals(params TokenType[] tokenTypesToCompare)
    {
        foreach (var type in tokenTypesToCompare)
        {
            if (Current().Type == type) return true;
        }

        return false;
    }

    /// <summary>
    /// Boolean <c>CurrentEquals</c> TokenContext[]: This method returns true or false depending on if the current token from the current Index has the same context as the parameter.
    /// <para>This helper method is an alternative to the <c>"Current().value.Equals()"</c> syntax, which could become verbose and harder to read when it's used frequently</para>
    /// </summary>
    public static bool CurrentEquals(params TokenContext[] tokenContextsToCompare)
    {
        foreach (var context in tokenContextsToCompare)
        {
            if (Current().Context.Equals(context)) return true;
        }

        return false;
    }

    /// <summary>
    /// Void <c>Continue</c>: This method adds +1 to the current index, moving the index to the next token.
    /// <para>This method will be called in the Expected(), Optional(), Choice(), Identifier(), Number() and String() methods,
    /// so there's no need to call Continue after calling those methods.</para>
    /// </summary>
    public static void Continue(int amount = 1)
    {
        if (CurrentEquals(TokenType.EOF) && Index >= Source.Count - 1) return;

        Index += amount;
    }

    /// <summary>
    /// Void <c>Choice</c>: This method checks if the current token matches one of the values passed in its parameters.
    /// <para>If the current token matches one the values, it moves to the next token,
    /// if the current token doesn't match any of the values it calls the ErrorHandler to report a parsing error</para>
    /// </summary>
    public static void Choice(params string[] choices)
    {
        foreach (string choice in choices)
        {
            if (CurrentEquals(choice))
            {
                Continue();
                return;
            }
        }

        ErrorHandler
        .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, """
            Unexpected token.
            """)
        .WithSourceLine(Current(), $"""
            Expected one of the following: {string.Join(", ", choices)},
            """)
        .CloseError();

        Continue();
    }

    /// <summary>
    /// Void <c>Optional</c>: This method checks if the current token is equal to it's first parameter.
    /// <para>If the current token matches the value, it moves to the next token,
    /// if the current token doesn't match the value it ignores the token and returns without moving to the next token</para>
    /// </summary>
    public static void Optional(string optional)
    {
        if (CurrentEquals(optional)) Continue();
    }

    public static void OptionalChoice(params string[] choices)
    {
        foreach (string choice in choices)
        {
            if (CurrentEquals(choice))
            {
                Continue();
                return;
            }
        }
    }

    /// <summary>
    /// Void <c>Expected</c>: This method checks if the current token is equal to it's first parameter.
    /// <para>If the current token matches the value, it moves to the next token,
    /// if the current token doesn't match the value it calls the ErrorHandler to report a parsing error</para>
    /// </summary>
    public static void Expected(string expected, string? custom = null, int position = 0, params string[] wordAnchors)
    {
        if (CurrentEquals(TokenType.EOF))
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 0, """
                Unexpected end of file.
                """)
            .WithSourceLine(Lookahead(-1), $"""
                Expected {expected} after this token.
                """)
            .CloseError();

            return;
        }

        if (!CurrentEquals(expected))
        {
            var lookahead = Lookahead(position);

            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, """
                Unexpected token.
                """)
            .WithSourceLine(lookahead, $"""
                {custom ?? $"Expected {expected}, instead of {Current().Value}"}
                """)
            .CloseError();

            if (wordAnchors.Length != 0) AnchorPoint(wordAnchors);

            if (wordAnchors.Length == 0) Continue();
        }
        else
        {
            Continue();
        }
    }

    public static bool Expected(string expected, bool useDefaultError = true)
    {
        if (CurrentEquals(TokenType.EOF))
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 0, """
                Unexpected end of file.
                """)
            .WithSourceLine(Lookahead(-1), $"""
                Expected {expected} after this token.
                """)
            .CloseError();

            // Error has already been handled above
            return true;
        }

        var isExpectedToken = CurrentEquals(expected);

        if (!isExpectedToken && useDefaultError)
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, """
                Unexpected token.
                """)
            .WithSourceLine(Current(), $"""
                $"Expected '{expected}' here, found '{Current().Value}' instead"
                """)
            .CloseError();

            // Error has already been handled above
            // Handled using a default error message
            return true;
        }

        if (!isExpectedToken && !useDefaultError)
        {
            // Error has not been handled
            // Expected to be handled by the consumer
            // of this method using an if statement
            return false;
        }
        
        Continue();

        return true;
    }


    /// <summary>
    /// Void <c>Identifier</c>: This method checks if the current token is an identifier.
    /// <para>If the current token's type is TokenType.Identifier, it moves to the next token,
    /// if the current token's type is TokenType.Identifier it calls the ErrorHandler to report a parsing error</para>
    /// </summary>
    public static void Identifier(IdentifierType allowedTypes = IdentifierType.None)
    {
        // TODO:
        // All Identifiers here need a check from the symbol table.
        // The symbol table itself needs to be refactored to accommodate this.
        if (CurrentEquals(TokenType.EOF))
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 0, """
                Unexpected end of file.
                """)
            .WithSourceLine(Lookahead(-1), $"""
                Expected an identifier after this token.
                """)
            .CloseError();

            return;
        }

        if (!CurrentEquals(TokenType.Identifier))
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 1, """
                Unexpected token.
                """)
            .WithSourceLine(Current(), $"""
                Expected a user-defined word (an identifier).
                """)
            .CloseError();

            Continue();
            return;
        }

        static bool HasFlag(Enum currentFlags, Enum flag)
        {
            return currentFlags.HasFlag(flag);
        }

        if (CurrentEquals("FUNCTION"))
        {
            Expected("FUNCTION");
            if (!HasFlag(allowedTypes, IdentifierType.Function))
            {
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 15, """
                    Unexpected function identifier.
                    """)
                .WithSourceLine(Current(), $"""
                    This function call should not be here.
                    """)
                .WithNote("""
                    Function calls must not be used as receiving operands
                    """)
                .CloseError();
            }

            Continue();
            if (CurrentEquals("("))
            {
                // TODO:
                // This needs a check from the symbol table to verify the number and type
                // of the function's parameters.
                Expected("(");
                while (!CurrentEquals(")")) Continue();
                Expected(")");
            }

            return;
        }

        if (CurrentEquals("EXCEPTION-OBJECT"))
        {
            Expected("EXCEPTION-OBJECT");
            if (!HasFlag(allowedTypes, IdentifierType.ExceptionObject))
            {
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 15, """
                    Unexpected EXCEPTION-OBJECT identifier.
                    """)
                .WithSourceLine(Current(), $"""
                    This EXCEPTION-OBJECT should not be here.
                    """)
                .WithNote("""
                    EXCEPTION-OBJECT must not be used as receiving operand
                    """)
                .CloseError();
            }

            return;
        }
        
        if (CurrentEquals("SELF"))
        {
            Expected("SELF");
            if (!HasFlag(allowedTypes, IdentifierType.Self))
            {
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 15, """
                    Unexpected SELF identifier.
                    """)
                .WithSourceLine(Current(), $"""
                    This SELF should not be here.
                    """)
                .WithNote("""
                    SELF must not be used as receiving operand
                    """)
                .CloseError();
            }

            return;
        }
        
        if (CurrentEquals("NULL"))
        {
            Expected("NULL");
            if (!HasFlag(allowedTypes, IdentifierType.NullAddress) && !HasFlag(allowedTypes, IdentifierType.NullObject))
            {
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 15, """
                    Unexpected NULL identifier.
                    """)
                .WithSourceLine(Current(), $"""
                    This NULL reference should not be here.
                    """)
                .WithNote("""
                    NULL must not be used as receiving operand
                    """)
                .CloseError();
            }

            return;
        }
        
        if (CurrentEquals("ADDRESS") && !LookaheadEquals(1, "PROGRAM", "FUNCTION") && !LookaheadEquals(2, "PROGRAM", "FUNCTION"))
        {
            Expected("ADDRESS");
            Optional("OF");
            if (!HasFlag(allowedTypes, IdentifierType.DataAddress))
            {
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 15, """
                    Unexpected ADDRESS OF identifier.
                    """)
                .WithSourceLine(Current(), $"""
                    This ADDRESS OF reference should not be here.
                    """)
                .WithNote("""
                    ADDRESS OF must not be used as receiving operand
                    """)
                .CloseError();
            }

            Continue();
            return;
        }
        
        if (CurrentEquals("ADDRESS") && LookaheadEquals(1, "FUNCTION") || LookaheadEquals(2, "FUNCTION"))
        {
            Expected("ADDRESS");
            Optional("OF");
            Expected("FUNCTION");
            if (!HasFlag(allowedTypes, IdentifierType.FunctionAddress))
            {
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 15, """
                    Unexpected ADDRESS OF FUNCTION identifier.
                    """)
                .WithSourceLine(Current(), $"""
                    This ADDRESS OF FUNCTION should not be here.
                    """)
                .WithNote("""
                    ADDRESS OF FUNCTION must not be used as receiving operand
                    """)
                .CloseError();
            }

            if (CurrentEquals(TokenType.Identifier))
            {
                Continue();
            }
            else
            {
                StringLiteral();
            }

            return;
        }
        
        if (CurrentEquals("ADDRESS") && LookaheadEquals(1, "PROGRAM") || LookaheadEquals(2, "PROGRAM"))
        {
            Expected("ADDRESS");
            Optional("OF");
            Expected("PROGRAM");
            if (!HasFlag(allowedTypes, IdentifierType.ProgramAddress))
            {
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 15, """
                    Unexpected ADDRESS OF PROGRAM identifier.
                    """)
                .WithSourceLine(Current(), $"""
                    This ADDRESS OF PROGRAM should not be here.
                    """)
                .WithNote("""
                    ADDRESS OF PROGRAM must not be used as receiving operand
                    """)
                .CloseError();
            }

            if (CurrentEquals(TokenType.Identifier))
            {
                Continue();
            }
            else
            {
                StringLiteral();
            }

            return;
        }
        
        if (CurrentEquals("LINAGE-COUNTER"))
        {
            Expected("LINAGE-COUNTER");
            Choice("IN", "OF");
            if (!HasFlag(allowedTypes, IdentifierType.LinageCounter))
            {
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 15, """
                    Unexpected LINAGE-COUNTER identifier.
                    """)
                .WithSourceLine(Current(), $"""
                    This LINAGE-COUNTER should not be here.
                    """)
                .WithNote("""
                    LINAGE-COUNTER must not be used as receiving operand
                    """)
                .CloseError();
            }

            Continue();
            return;
        }
        
        if (CurrentEquals("PAGE-COUNTER", "LINE-COUNTER"))
        {
            var token = Current();

            Choice("PAGE-COUNTER", "LINE-COUNTER");
            Choice("IN", "OF");

            if (!HasFlag(allowedTypes, IdentifierType.ReportCounter))
            {
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 15, $"""
                    Unexpected {token.Value} identifier.
                    """)
                .WithSourceLine(Current(), $"""
                    This {token.Value} should not be here.
                    """)
                .WithNote($"""
                    {token.Value} must not be used as receiving operand
                    """)
                .CloseError();
            }

            Continue();
            return;
        }

        if (LookaheadEquals(1, "AS"))
        {
            // var isFactory = false;
            // var isStronglyTyped = false;

            // Need to implement identifier resolution first
            // To parse the rest of this identifier correctly
            // and to add extra compile time checks
        
            Continue();
            Expected("AS");

            if (!HasFlag(allowedTypes, IdentifierType.ObjectView))
            {
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 15, """
                    Unexpected Object View identifier.
                    """)
                .WithSourceLine(Current(), $"""
                    This Object View should not be here.
                    """)
                .WithNote("""
                    Object View must not be used as receiving operand
                    """)
                .CloseError();
            }

            if (CurrentEquals("UNIVERSAL"))
            {
                Expected("UNIVERSAL");
                return;
            }

            if (CurrentEquals("Factory"))
            {
                Expected("FACTORY");
                Optional("OF");
                // isFactory = true;
            }

            Continue();

            if (CurrentEquals("ONLY"))
            {
                Expected("ONLY");
                // isStronglyTyped = true
            }

            return;
        }

        if (LookaheadEquals(1, "::"))
        {


            if (!HasFlag(allowedTypes, IdentifierType.MethodInvocation))
            {
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 15, """
                    Unexpected inline method call.
                    """)
                .WithSourceLine(Current(), $"""
                    This method call should not be here.
                    """)
                .WithNote("""
                    Inline method calls must not be used as receiving operand
                    """)
                .CloseError();
            }

            // TODO: Replace Continue with an identifier check for the method name
            Continue();
            Expected("::");
            StringLiteral();

            if (CurrentEquals("("))
            {
                // TODO:
                // This needs a check from the symbol table to verify the number and type
                // of the function's parameters.
                Expected("(");
                while (!CurrentEquals(")")) Continue();
                Expected(")");
            }

            return;
        }        

        Continue();
        if (CurrentEquals("("))
        {
            // TODO:
            // This needs a check from the symbol table 
            // to verify the identifier type.
            Expected("(");
            while (!CurrentEquals(")")) Continue();
            Expected(")");
        }
    }

    public static bool Identifier(Token identifierToken, bool useDefaultError = true)
    {
        if (CurrentEquals(TokenType.EOF))
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 0, """
                Unexpected end of file.
                """)
            .WithSourceLine(Lookahead(-1), $"""
                Expected an identifier after this token.
                """)
            .CloseError();

            // Error has already been handled above
            return true;
        }

        if (!CurrentEquals(TokenType.Identifier))
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 1, """
                Unexpected token type.
                """)
            .WithSourceLine(Current(), $"""
                Expected a user-defined word (an identifier).
                """)
            .CloseError();

            Continue();

            // Error has already been handled above
            return true;
        }

        var isExpectedToken = CurrentEquals(identifierToken.Value);

        if (!isExpectedToken && useDefaultError)
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 2, """
                Unexpected user-defined name.
                """)
            .WithSourceLine(Current(), $"""
                Expected the following identifier: {identifierToken.Value}.
                """)
            .CloseError();

            // Error has already been handled above
            // Handled using a default error message
            return true;
        }

        if (!isExpectedToken && !useDefaultError)
        {
            // Error has not been handled
            // Expected to be handled by the consumer
            // of this method using an if statement
            return false;
        }
        
        Continue();

        return true;
    }

    /// <summary>
    /// Void <c>Number</c>: This method checks if the current token is a Number.
    /// <para>If the current token's type is TokenType.Numeric, it moves to the next token,
    /// if the current token's type is TokenType.Numeric it calls the ErrorHandler to report a parsing error</para>
    /// </summary>
    public static void Number()
    {
        if (!CurrentEquals(TokenType.Numeric))
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 1, """
                Unexpected token type.
                """)
            .WithSourceLine(Current(), $"""
                Expected a numeric literal.
                """)
            .CloseError();

            Continue();
            return;
        }

        Continue();
    }

    /// <summary>
    /// Void <c>String</c>: This method checks if the current token is a National, Alphanumeric, Alphabetic or Boolean.
    /// <para>If the current token's type is TokenType.String, it moves to the next token,
    /// if the current token's type is TokenType.String it calls the ErrorHandler to report a parsing error</para>
    /// </summary>
    public static void StringLiteral()
    {
        if (!CurrentEquals(
            TokenType.String, 
            TokenType.HexString, 
            TokenType.Boolean, 
            TokenType.HexBoolean, 
            TokenType.National, 
            TokenType.HexNational
        ))
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 1, """
                Unexpected token type.
                """)
            .WithSourceLine(Current(), $"""
                Expected a string type literal.
                """)
            .CloseError();

            Continue();
            return;
        }

        Continue();
    }

    public static void BooleanLiteral()
    {
        if (!CurrentEquals(
            TokenType.Boolean, 
            TokenType.HexBoolean
        ))
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 1, """
                Unexpected token type.
                """)
            .WithSourceLine(Current(), $"""
                Expected a boolean literal.
                """)
            .CloseError();

            Continue();
            return;
        }

        Continue();
    }

    public static void NationalLiteral()
    {
        if (!CurrentEquals(
            TokenType.National, 
            TokenType.HexNational
        ))
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 1, """
                Unexpected token type.
                """)
            .WithSourceLine(Current(), $"""
                Expected a national literal.
                """)
            .CloseError();

            Continue();
            return;
        }

        Continue();
    }

    /// <summary>
    /// Void <c>FigurativeLiteral</c>: This method checks if the current token is a Figurative Literal.
    /// <para>If the current token's type is TokenType.FigurativeLiteral, it moves to the next token,
    /// if the current token's type is TokenType.FigurativeLiteral it calls the ErrorHandler to report a parsing error</para>
    /// </summary>
    public static void FigurativeLiteral()
    {
        if (!CurrentEquals(TokenType.FigurativeLiteral))
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 1, """
                Unexpected token type.
                """)
            .WithSourceLine(Current(), $"""
                Expected a figurative literal.
                """)
            .CloseError();

            Continue();
            return;
        }

        Continue();
    }
}
