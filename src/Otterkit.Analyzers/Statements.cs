using static Otterkit.Analyzers.TokenHandling;
using Otterkit.Types;

namespace Otterkit.Analyzers;

public static class Statements
{
    // Analyzer Statement methods.
    // These are the methods used to parse and analyze all Standard COBOL statements.
    // All of these methods are responsible *ONLY* for statements, paragraphs and sections inside the procedure division.
    // There shouldn't be any identification, environment or data division methods here.
    public static void WithoutSections(bool isNested = false)
    {
        bool errorCheck = Current().Context != TokenContext.IsStatement
            && !(CurrentEquals(TokenType.Identifier) && LookaheadEquals(1, ".") && !isNested)
            && !(CurrentEquals(TokenType.Identifier) && LookaheadEquals(1, "SECTION") && !isNested);

        if (errorCheck)
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                Unexpected {Current().Type.Display(false)}.
                """)
            .WithSourceLine(Current(), $"""
                Expected start of a statement. Instead found {Current().Value}.
                """)
            .CloseError();

            AnchorPoint(TokenContext.IsStatement);
        }

        while (!isNested ? !CurrentEquals("EOF", "END") && !(CurrentEquals(TokenType.Identifier) && LookaheadEquals(1, "SECTION")) : CurrentEquals(TokenContext.IsStatement))
        {
            Statement(isNested);

            ScopeTerminator(isNested);

            if (isNested && !CurrentEquals(TokenContext.IsStatement)) return;

            if (CurrentEquals(TokenType.Identifier) && LookaheadEquals(1, "SECTION")) return;
        }
    }

    public static void WithSections()
    {
        if (CurrentEquals("DECLARATIVES"))
        {
            Expected("DECLARATIVES");
            Expected(".");

            Identifier();
            Expected("SECTION");
            Expected(".");
            UseStatement();
            Statements.WithoutSections();

            while (CurrentEquals(TokenType.Identifier) && LookaheadEquals(1, "SECTION"))
            {
                Identifier();
                Expected("SECTION");
                Expected(".");
                UseStatement();
                Statements.WithoutSections();
            }

            Expected("END");
            Expected("DECLARATIVES");
            Expected(".");
        }

        while (CurrentEquals(TokenType.Identifier) && LookaheadEquals(1, "SECTION"))
        {
            Identifier();
            Expected("SECTION");
            Expected(".");
            Statements.WithoutSections();
        }
    }

    private static void Statement(bool isNested = false)
    {
        if (CurrentEquals("ACCEPT"))
            AcceptStatement();

        else if (CurrentEquals("ADD"))
            AddStatement();

        else if (CurrentEquals("ALLOCATE"))
            AllocateStatement();

        else if (CurrentEquals("CALL"))
            CallStatement();

        else if (CurrentEquals("CANCEL"))
            CancelStatement();

        else if (CurrentEquals("CLOSE"))
            CloseStatement();

        else if (CurrentEquals("COMMIT"))
            CommitStatement();

        else if (CurrentEquals("CONTINUE"))
            ContinueStatement();

        else if (CurrentEquals("COMPUTE"))
            ComputeStatement();

        else if (CurrentEquals("DISPLAY"))
            DisplayStatement();

        else if (CurrentEquals("DIVIDE"))
            DivideStatement();

        else if (CurrentEquals("DELETE"))
            DeleteStatement();

        else if (CurrentEquals("IF"))
            IfStatement();

        else if (CurrentEquals("INITIALIZE"))
            InitializeStatement();

        else if (CurrentEquals("INITIATE"))
            InitiateStatement();

        else if (CurrentEquals("INSPECT"))
            InspectStatement();

        else if (CurrentEquals("INVOKE"))
            InvokeStatement();

        else if (CurrentEquals("MERGE"))
            MergeStatement();

        else if (CurrentEquals("MULTIPLY"))
            MultiplyStatement();

        else if (CurrentEquals("MOVE"))
            MoveStatement();

        else if (CurrentEquals("OPEN"))
            OpenStatement();

        else if (CurrentEquals("EXIT"))
            ExitStatement();

        else if (CurrentEquals("EVALUATE"))
            EvaluateStatement();

        else if (CurrentEquals("FREE"))
            FreeStatement();

        else if (CurrentEquals("GENERATE"))
            GenerateStatement();

        else if (CurrentEquals("GO"))
            GoStatement();

        else if (CurrentEquals("GOBACK"))
            GobackStatement();

        else if (CurrentEquals("SUBTRACT"))
            SubtractStatement();

        else if (CurrentEquals("PERFORM"))
            PerformStatement();

        else if (CurrentEquals("RELEASE"))
            ReleaseStatement();

        else if (CurrentEquals("RAISE"))
            RaiseStatement();

        else if (CurrentEquals("READ"))
            ReadStatement();

        else if (CurrentEquals("RECEIVE"))
            ReceiveStatement();

        else if (CurrentEquals("RESUME"))
            ResumeStatement();

        else if (CurrentEquals("RETURN"))
            ReturnStatement();

        else if (CurrentEquals("REWRITE"))
            RewriteStatement();

        else if (CurrentEquals("ROLLBACK"))
            RollbackStatement();

        else if (CurrentEquals("SEARCH"))
            SearchStatement();

        else if (CurrentEquals("SEND"))
            SendStatement();

        else if (CurrentEquals("SET"))
            SetStatement();

        else if (CurrentEquals("SORT"))
            SortStatement();

        else if (CurrentEquals("START"))
            StartStatement();

        else if (CurrentEquals("STOP"))
            StopStatement();

        else if (CurrentEquals("STRING"))
            StringStatement();

        else if (CurrentEquals("SUPPRESS"))
            SuppressStatement();

        else if (CurrentEquals("TERMINATE"))
            TerminateStatement();

        else if (CurrentEquals("UNLOCK"))
            UnlockStatement();

        else if (CurrentEquals("UNSTRING"))
            UnstringStatement();

        else if (CurrentEquals("VALIDATE"))
            ValidateStatement();

        else if (CurrentEquals("WRITE"))
            WriteStatement();

        else if (CurrentEquals(TokenType.Identifier) && LookaheadEquals(1, ".") && !isNested)
            ParseParagraph();
    }

    // Statement parsing methods
    // All the following uppercased methods are responsible for parsing a single COBOL statement
    // When a new method is added here to parse a new statement, we need to add it to the Statement() method as well.
    // Adding extra statements to the parser only requires a new method here, and an if statement added to the Statement() method
    private static void UseStatement()
    {
        Expected("USE");

        bool exceptionObject = CurrentEquals("AFTER") && LookaheadEquals(1, "EXCEPTION") && LookaheadEquals(2, "OBJECT")
            || CurrentEquals("AFTER") && LookaheadEquals(1, "EO")
            || CurrentEquals("EXCEPTION") && LookaheadEquals(1, "OBJECT")
            || CurrentEquals("EO");

        bool exceptionCondition = CurrentEquals("AFTER") && LookaheadEquals(1, "EXCEPTION") && LookaheadEquals(2, "CONDITION")
            || CurrentEquals("AFTER") && LookaheadEquals(1, "EC")
            || CurrentEquals("EXCEPTION") && LookaheadEquals(1, "CONDITION")
            || CurrentEquals("EC");

        bool reporting = CurrentEquals("GLOBAL") && LookaheadEquals(1, "BEFORE") || CurrentEquals("BEFORE");

        bool fileException = CurrentEquals("GLOBAL") && LookaheadEquals(1, "AFTER", "STANDARD", "EXCEPTION", "ERROR")
            || CurrentEquals("AFTER", "STANDARD", "EXCEPTION", "ERROR");

        if (exceptionObject)
        {
            Optional("AFTER");
            if (CurrentEquals("EO"))
            {
                Expected("EO");
            }
            else
            {
                Expected("EXCEPTION");
                Expected("OBJECT");
            }

            Identifier();
        }
        else if (exceptionCondition)
        {
            Optional("AFTER");
            if (CurrentEquals("EO"))
            {
                Expected("EO");
            }
            else
            {
                Expected("EXCEPTION");
                Expected("OBJECT");
            }

            Identifier();
        }
        else if (reporting)
        {
            if (CurrentEquals("GLOBAL")) Expected("GLOBAL");

            Expected("BEFORE");
            Expected("REPORTING");
            Identifier();
        }
        else if (fileException)
        {
            if (CurrentEquals("GLOBAL")) Expected("GLOBAL");

            Optional("AFTER");
            Optional("STANDARD");
            Choice("EXCEPTION", "ERROR");
            Optional("PROCEDURE");
            Optional("ON");

            if (CurrentEquals("INPUT", "OUTPUT", "I-O", "EXTEND"))
            {
                Choice("INPUT", "OUTPUT", "I-O", "EXTEND");
            }
            else
            {
                Identifier();
                while (CurrentEquals(TokenType.Identifier))
                    Identifier();
            }
        }
        else
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                Unexpected {Current().Type.Display(false)}.
                """)
            .WithSourceLine(Current(), $"""
                Expected AFTER EXCEPTION OBJECT, AFTER EXCEPTION CONDITION, BEFORE REPORTING or AFTER EXCEPTION/ERROR.
                """)
            .CloseError();

            AnchorPoint(TokenContext.IsStatement);
        }

        ScopeTerminator(false);
    }

    private static void DisplayStatement()
    {
        Expected("DISPLAY");

        if (CurrentEquals(TokenType.Identifier) && LookaheadEquals(1, "AT", "LINE", "COLUMN", "COL"))
        {
            bool isConditional = false;

            Identifier();
            Optional("AT");
            Common.LineColumn();
            Common.OnException(ref isConditional);

            if (isConditional) Expected("END-COMPUTE");
            return;
        }

        if (Common.NotIdentifierOrLiteral())
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                Unexpected {Current().Type.Display(false)}.
                """)
            .WithSourceLine(Current(), $"""
                Expected an identifier or a literal.
                """)
            .CloseError();

            AnchorPoint("UPON", "WITH", "NO");
        }

        switch (Current().Type)
        {
            case TokenType.Identifier: Identifier(); break;
            case TokenType.Numeric: Number(); break;
            
            case TokenType.String:
            case TokenType.HexString:
            case TokenType.Boolean:
            case TokenType.HexBoolean:
            case TokenType.National:
            case TokenType.HexNational:
                StringLiteral(); break;
        }

        while (Common.IdentifierOrLiteral())
        {
            switch (Current().Type)
            {
                case TokenType.Identifier: Identifier(); break;
                case TokenType.Numeric: Number(); break;
                
                case TokenType.String:
                case TokenType.HexString:
                case TokenType.Boolean:
                case TokenType.HexBoolean:
                case TokenType.National:
                case TokenType.HexNational:
                    StringLiteral(); break;
            }
        }

        if (CurrentEquals("UPON"))
        {
            Expected("UPON");
            Choice("STANDARD-OUTPUT", "STANDARD-ERROR");
        }

        if (CurrentEquals("WITH", "NO"))
        {
            Optional("WITH");
            Expected("NO");
            Expected("ADVANCING");
        }

        Optional("END-DISPLAY");
    }

    private static void AcceptStatement()
    {
        bool isConditional = false;

        Expected("ACCEPT");
        Identifier();
        if (CurrentEquals("FROM"))
        {
            Expected("FROM");
            
            if (CurrentEquals("STANDARD-INPUT", "COMMAND-LINE"))
            {
                    Choice("STANDARD-INPUT", "COMMAND-LINE");
            }
            else if (CurrentEquals("DATE"))
            {
                Expected("DATE");
                Optional("YYYYMMDD"); 
            }
            else if (CurrentEquals("DAY"))
            {
                Expected("DAY");
                Optional("YYYYDDD");   
            }
            else if (CurrentEquals("DAY-OF-WEEK"))
            {
                Expected("DAY-OF-WEEK");
            }
            else if (CurrentEquals("TIME"))
            {
                Expected("TIME");

            }
        }
        else if (CurrentEquals("AT", "LINE", "COLUMN", "COL"))
        {
            Optional("AT");
            if (!CurrentEquals("LINE", "COLUMN", "COL"))
            {
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                    Unexpected {Lookahead(-1).Type.Display(false)}.
                    """)
                .WithSourceLine(Lookahead(-1), $"""
                    When specifying the AT keyword, it must be followed by a LINE NUMBER, COLUMN/COL NUMBER or both.
                    """)
                .CloseError();
            }

            Common.LineColumn();
            
            Common.OnException(ref isConditional);
        }

        if (isConditional) Expected("END-ACCEPT");
    }

    private static void AllocateStatement()
    {
        Expected("ALLOCATE");
        if (CurrentEquals(TokenType.Identifier) && !LookaheadEquals(1, "CHARACTERS") && !LookaheadEquals(1, TokenType.Symbol))
            Identifier();

        if (CurrentEquals(TokenType.Identifier, TokenType.Numeric))
        {
            Common.Arithmetic("CHARACTERS");
            Expected("CHARACTERS");
        }

        if (CurrentEquals("INITIALIZED"))
            Expected("INITIALIZED");

        if (CurrentEquals("RETURNING"))
        {
            Expected("RETURNING");
            Identifier();
        }
    }

    private static void ComputeStatement()
    {
        bool isConditional = false;

        Expected("COMPUTE");
        if (!CurrentEquals(TokenType.Identifier))
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                Unexpected {Current().Type.Display(false)}.
                """)
            .WithSourceLine(Current(), $"""
                Expected an identifier.
                """)
            .CloseError();
        }

        while (CurrentEquals(TokenType.Identifier))
        {
            Identifier();
        }

        Expected("=");
        if (Common.NotIdentifierOrLiteral())
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                Unexpected {Current().Type.Display(false)}.
                """)
            .WithSourceLine(Current(), $"""
                Expected a valid arithmetic expression.
                """)
            .CloseError();
        }

        Common.Arithmetic(".");

        if (CurrentEquals(".")) return;

        Common.SizeError(ref isConditional);

        if (isConditional) Expected("END-COMPUTE");
    }

    private static void CallStatement()
    {
        bool isConditional = false;
        bool isPrototype = false;

        Expected("CALL");
        if (CurrentEquals(TokenType.Identifier, TokenType.String))
        {
            if (CurrentEquals(TokenType.Identifier))
            {
                Identifier();
            }
            else
            {
                StringLiteral();
            }

            if (CurrentEquals("AS"))
            {
                isPrototype = true;
                Expected("AS");
            }
        }
        else
        {
            isPrototype = true;
        }

        if (isPrototype && CurrentEquals("NESTED"))
        {
            Expected("NESTED");
        }
        else if (isPrototype && !CurrentEquals("NESTED"))
        {
            Identifier();
        }

        if (!isPrototype && CurrentEquals("USING"))
        {
            Expected("USING");

            StatementUsing(false, true);
        }
        else if (isPrototype && CurrentEquals("USING"))
        {
            Expected("USING");

            StatementUsing(true, true);
        }

        if (CurrentEquals("RETURNING"))
        {
            Expected("RETURNING");
            Identifier();
        }

        Common.OnException(ref isConditional);

        if (isConditional) Expected("END-CALL");
    }

    private static void ContinueStatement()
    {
        Expected("CONTINUE");
        if (CurrentEquals("AFTER"))
        {
            Expected("AFTER");
            Common.Arithmetic("SECONDS");
            Expected("SECONDS");
        }
    }

    private static void AddStatement()
    {
        bool isConditional = false;

        Expected("ADD");

        if (CurrentEquals("CORRESPONDING", "CORR"))
        {
            Continue();
            Identifier();
            Expected("TO");
            Identifier();
            Common.SizeError(ref isConditional);

            if (isConditional) Expected("END-ADD");
            return;
        }

        if (!CurrentEquals(TokenType.Identifier, TokenType.Numeric))
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                Unexpected {Current().Type.Display(false)}.
                """)
            .WithSourceLine(Current(), $"""
                Expected an identifier or numeric literal.
                """)
            .CloseError();
        }

        while (CurrentEquals(TokenType.Identifier, TokenType.Numeric))
        {
            if (CurrentEquals(TokenType.Identifier))
                Identifier();

            if (CurrentEquals(TokenType.Numeric))
                Number();
        }

        if (CurrentEquals("TO") && LookaheadEquals(2, "GIVING"))
        {
            Optional("TO");
            switch (Current().Type)
            {
                case TokenType.Identifier:
                    Identifier();
                    break;

                case TokenType.Numeric:
                    Number();
                    break;

                default:
                    ErrorHandler
                    .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                        Unexpected {Current().Type.Display(false)}.
                        """)
                    .WithSourceLine(Current(), $"""
                        Expected an identifier or numeric literal.
                        """)
                    .CloseError();
                    break;
            }

            Expected("GIVING");
            if (Current().Type != TokenType.Identifier)
            {
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                    Unexpected {Current().Type.Display(false)}.
                    """)
                .WithSourceLine(Current(), $"""
                    Expected an identifier.
                    """)
                .CloseError();
            }

            while (Current().Type == TokenType.Identifier)
                Identifier();
        }
        else if (CurrentEquals("GIVING"))
        {
            Expected("GIVING");
            if (Current().Type != TokenType.Identifier)
            {
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                    Unexpected {Current().Type.Display(false)}.
                    """)
                .WithSourceLine(Current(), $"""
                    Expected an identifier.
                    """)
                .CloseError();
            }

            while (Current().Type == TokenType.Identifier)
                Identifier();
        }
        else if (CurrentEquals("TO"))
        {
            Expected("TO");
            if (Current().Type != TokenType.Identifier)
            {
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                    Unexpected {Current().Type.Display(false)}.
                    """)
                .WithSourceLine(Current(), $"""
                    Expected an identifier.
                    """)
                .CloseError();
            }

            while (Current().Type == TokenType.Identifier)
                Identifier();
        }
        else
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                Unexpected {Current().Type.Display(false)}.
                """)
            .WithSourceLine(Current(), $"""
                Expected TO or GIVING reserved words.
                """)
            .CloseError();
        }

        Common.SizeError(ref isConditional);

        if (isConditional) Expected("END-ADD");
    }

    private static void SubtractStatement()
    {
        bool isConditional = false;

        Expected("SUBTRACT");
        if (Current().Type != TokenType.Identifier && Current().Type != TokenType.Numeric)
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                Unexpected {Current().Type.Display(false)}.
                """)
            .WithSourceLine(Current(), $"""
                Expected an identifier or numeric literal.
                """)
            .CloseError();
        }

        while (Current().Type == TokenType.Identifier
            || Current().Type == TokenType.Numeric
        )
        {
            if (Current().Type == TokenType.Identifier)
                Identifier();

            if (Current().Type == TokenType.Numeric)
                Number();
        }

        if (CurrentEquals("FROM") && LookaheadEquals(2, "GIVING"))
        {
            Optional("FROM");
            switch (Current().Type)
            {
                case TokenType.Identifier:
                    Identifier();
                    break;

                case TokenType.Numeric:
                    Number();
                    break;

                default:
                    ErrorHandler
                    .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                        Unexpected {Current().Type.Display(false)}.
                        """)
                    .WithSourceLine(Current(), $"""
                        Expected an identifier or numeric literal.
                        """)
                    .CloseError();
                    break;
            }

            Expected("GIVING");
            if (Current().Type != TokenType.Identifier)
            {
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                    Unexpected {Current().Type.Display(false)}.
                    """)
                .WithSourceLine(Current(), $"""
                    Expected an identifier.
                    """)
                .CloseError();
            }

            while (Current().Type == TokenType.Identifier)
                Identifier();
        }
        else if (CurrentEquals("FROM"))
        {
            Expected("FROM");
            if (Current().Type != TokenType.Identifier)
            {
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                    Unexpected {Current().Type.Display(false)}.
                    """)
                .WithSourceLine(Current(), $"""
                    Expected an identifier.
                    """)
                .CloseError();
            }

            while (Current().Type == TokenType.Identifier)
                Identifier();
        }
        else
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                Unexpected {Current().Type.Display(false)}.
                """)
            .WithSourceLine(Current(), $"""
                Expected FROM reserved word.
                """)
            .CloseError();
        }

        Common.SizeError(ref isConditional);

        if (isConditional)
            Expected("END-SUBTRACT");
    }

    private static void IfStatement()
    {
        Expected("IF");
        Common.Condition("THEN");
        Optional("THEN");
        if (CurrentEquals("NEXT") && LookaheadEquals(1, "SENTENCE"))
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                Unsupported phrase: NEXT SENTENCE is an archaic feature.
                """)
            .WithSourceLine(Current(), $"""
                The CONTINUE statement can be used to accomplish the same functionality.
                """)
            .CloseError();
        }
        
        Statements.WithoutSections(true);

        if (CurrentEquals("ELSE"))
        {
            Expected("ELSE");
            Statements.WithoutSections(true);
        }

        Expected("END-IF");
    }

    private static void InitializeStatement()
    {
        Expected("INITIALIZE");
        Identifier();
        while (CurrentEquals(TokenType.Identifier))
        {
            Identifier();
        }

        if (CurrentEquals("WITH", "FILLER"))
        {
            Optional("WITH");
            Expected("FILLER");
        }

        static bool IsCategoryName()
        {
            if (CurrentEquals(
            "ALPHABETIC",
            "ALPHANUMERIC",
            "ALPHANUMERIC-EDITED",
            "BOOLEAN",
            "DATA-POINTER",
            "FUNCTION-POINTER",
            "MESSAGE-TAG",
            "NATIONAL",
            "NATIONAL-EDITED",
            "NUMERIC",
            "NUMERIC-EDITED",
            "OBJECT-REFERENCE",
            "PROGRAM-POINTER"
            )) return true;

            return false;
        };

        if (IsCategoryName())
        {
            Expected(Current().Value);
            Optional("TO");
            Expected("VALUE");
        }
        else if (CurrentEquals("ALL"))
        {
            Expected("ALL");
            Optional("TO");
            Expected("VALUE");
        }

        if (CurrentEquals("THEN", "REPLACING"))
        {
            Optional("THEN");
            Expected("REPLACING");
            if (IsCategoryName())
            {
                Expected(Current().Value);
                Optional("DATA");
                Expected("BY");

                if (Common.NotIdentifierOrLiteral())
                {
                    ErrorHandler
                    .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                        Unexpected {Current().Type.Display(false)}.
                        """)
                    .WithSourceLine(Current(), $"""
                        Expected an identifier or literal.
                        """)
                    .CloseError();

                    AnchorPoint(TokenContext.IsStatement);
                }

                if (CurrentEquals(TokenType.Identifier))
                {
                    Identifier();
                }
                else if (CurrentEquals(TokenType.String))
                {
                    StringLiteral();
                }
                else if (CurrentEquals(TokenType.Numeric))
                {
                    Number();
                }

                while (IsCategoryName())
                {
                    Expected(Current().Value);
                    Optional("DATA");
                    Expected("BY");

                    if (Common.NotIdentifierOrLiteral())
                    {
                        ErrorHandler
                        .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                            Unexpected {Current().Type.Display(false)}.
                            """)
                        .WithSourceLine(Current(), $"""
                            Expected an identifier or literal.
                            """)
                        .CloseError();

                        AnchorPoint(TokenContext.IsStatement);
                    }

                    if (CurrentEquals(TokenType.Identifier))
                    {
                        Identifier();
                    }
                    else if (CurrentEquals(TokenType.String))
                    {
                        StringLiteral();
                    }
                    else if (CurrentEquals(TokenType.Numeric))
                    {
                        Number();
                    }

                }

            }
        }

        if (CurrentEquals("THEN", "TO", "DEFAULT"))
        {
            Optional("THEN");
            Optional("TO");
            Expected("DEFAULT");
        }
    }

    private static void InitiateStatement()
    {
        Expected("INITIATE");
        if (Current().Type != TokenType.Identifier)
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                Unexpected {Current().Type.Display(false)}.
                """)
            .WithSourceLine(Current(), $"""
                Expected a report entry identifier defined in the report section.
                """)
            .CloseError();
        }
        Identifier();
        while (Current().Type == TokenType.Identifier)
            Identifier();

        if (!CurrentEquals("."))
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                Unexpected {Current().Type.Display(false)}.
                """)
            .WithSourceLine(Current(), $"""
                Expected a report entry identifier defined in the report section.
                """)
            .CloseError();
        }
    }

    private static void InspectStatement()
    {
        Expected("INSPECT");
        if (CurrentEquals("BACKWARD")) Expected("BACKWARD");

        Identifier();

        if (CurrentEquals("CONVERTING"))
        {
            Expected("CONVERTING");
            if (CurrentEquals(TokenType.Identifier))
            {
                Identifier();
            }
            else
            {
                StringLiteral();
            }

            Expected("TO");
            if (CurrentEquals(TokenType.Identifier))
            {
                Identifier();
            }
            else
            {
                StringLiteral();
            }

            Common.AfterBeforePhrase();
        }
        else if (CurrentEquals("REPLACING"))
        {
            Expected("REPLACING");
            Common.ReplacingPhrase();
        }
        else if (CurrentEquals("TALLYING"))
        {
            Expected("TALLYING");
            Common.TallyingPhrase();
            if (CurrentEquals("REPLACING"))
            {
                Expected("REPLACING");
                Common.ReplacingPhrase();
            }
        }
    }

    private static void InvokeStatement()
    {
        Expected("INVOKE");
        Identifier();
        if (CurrentEquals(TokenType.Identifier))
        {
            Identifier();
        }
        else
        {
            StringLiteral();
        }

        if (CurrentEquals("USING"))
        {
            Expected("USING");

            StatementUsing(true, true);
        }

        if (CurrentEquals("RETURNING"))
        {
            Expected("RETURNING");
            Identifier();
        }
    }

    private static void MergeStatement()
    {
        Expected("MERGE");
        Identifier();

        Optional("ON");
        if (CurrentEquals("ASCENDING"))
        {
            Expected("ASCENDING");
        }
        else
        {
            Expected("DESCENDING");
        }

        Optional("KEY");

        if (!CurrentEquals(TokenType.Identifier))
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                Unexpected {Current().Type.Display(false)}.
                """)
            .WithSourceLine(Current(), $"""
                The ON ASCENDING / DESCENDING KEY phrase must only contain key names.
                """)
            .CloseError();
        }

        Identifier();
        while (Current().Type == TokenType.Identifier)
            Identifier();


        while (CurrentEquals("ON", "ASCENDING", "DESCENDING"))
        {
            Optional("ON");
            if (CurrentEquals("ASCENDING"))
            {
                Expected("ASCENDING");
            }
            else
            {
                Expected("DESCENDING");
            }

            Optional("KEY");

            if (!CurrentEquals(TokenType.Identifier))
            {
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                    Unexpected {Current().Type.Display(false)}.
                    """)
                .WithSourceLine(Current(), $"""
                    The ON ASCENDING / DESCENDING KEY phrase must only contain key names.
                    """)
                .CloseError();
            }

            Identifier();
            while (Current().Type == TokenType.Identifier)
                Identifier();
        }

        if (CurrentEquals("COLLATING", "SEQUENCE"))
        {
            Optional("COLLATING");
            Expected("SEQUENCE");
            if (CurrentEquals("IS") && LookaheadEquals(1, TokenType.Identifier) || CurrentEquals(TokenType.Identifier))
            {
                Optional("IS");
                Identifier();

                if (CurrentEquals(TokenType.Identifier)) Identifier();
            }
            else
            {
                if (!CurrentEquals("FOR", "ALPHANUMERIC", "NATIONAL"))
                {
                    ErrorHandler
                    .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                        Unexpected {Current().Type.Display(false)}.
                        """)
                    .WithSourceLine(Current(), $"""
                        Expected an alphabet name or at least one FOR ALPHANUMERIC and FOR NATIONAL phrases.
                        """)
                    .CloseError();

                    AnchorPoint(TokenContext.IsStatement, "USING");
                }

                Common.ForAlphanumericForNational();
            }
        }

        Expected("USING");
        Identifier();
        Identifier();
        while (Current().Type == TokenType.Identifier)
            Identifier();

        if (CurrentEquals("OUTPUT"))
        {
            Expected("OUTPUT");
            Expected("PROCEDURE");
            Optional("IS");
            Identifier();

            if (CurrentEquals("THROUGH", "THRU"))
            {
                Choice("THROUGH", "THRU");
                Identifier();
            }
        }
        else
        {
            Expected("GIVING");
            Identifier();
            while (Current().Type == TokenType.Identifier)
                Identifier();
        }
    }

    private static void MultiplyStatement()
    {
        bool isConditional = false;

        Expected("MULTIPLY");
        switch (Current().Type)
        {
            case TokenType.Identifier:
                Identifier();
                break;

            case TokenType.Numeric:
                Number();
                break;

            default:
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                    Unexpected {Current().Type.Display(false)}.
                    """)
                .WithSourceLine(Current(), $"""
                    Expected an identifier or numeric literal.
                    """)
                .CloseError();
                break;
        }

        if (CurrentEquals("BY") && LookaheadEquals(2, "GIVING"))
        {
            Optional("BY");
            switch (Current().Type)
            {
                case TokenType.Identifier:
                    Identifier();
                    break;

                case TokenType.Numeric:
                    Number();
                    break;

                default:
                    ErrorHandler
                    .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                        Unexpected {Current().Type.Display(false)}.
                        """)
                    .WithSourceLine(Current(), $"""
                        Expected an identifier or numeric literal.
                        """)
                    .CloseError();
                    break;
            }

            Expected("GIVING");
            if (Current().Type != TokenType.Identifier)
            {
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                    Unexpected {Current().Type.Display(false)}.
                    """)
                .WithSourceLine(Current(), $"""
                    Expected an identifier.
                    """)
                .CloseError();
            }

            while (Current().Type == TokenType.Identifier)
                Identifier();
        }
        else if (CurrentEquals("BY"))
        {
            Expected("BY");
            if (Current().Type != TokenType.Identifier)
            {
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                    Unexpected {Current().Type.Display(false)}.
                    """)
                .WithSourceLine(Current(), $"""
                    Expected an identifier.
                    """)
                .CloseError();
            }

            while (Current().Type == TokenType.Identifier)
                Identifier();
        }
        else
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                Unexpected {Current().Type.Display(false)}.
                """)
            .WithSourceLine(Current(), $"""
                Expected BY reserved word.
                """)
            .CloseError();
        }

        Common.SizeError(ref isConditional);

        if (isConditional)
            Expected("END-MULTIPLY");
    }

    private static void MoveStatement()
    {
        Expected("MOVE");
        if (CurrentEquals("CORRESPONDING") || CurrentEquals("CORR"))
        {
            Expected(Current().Value);
            Identifier();
            Expected("TO");
            Identifier();
            return;
        }

        if (Common.NotIdentifierOrLiteral())
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                Unexpected {Current().Type.Display(false)}.
                """)
            .WithSourceLine(Current(), $"""
                Expected a single data item identifier, literal or a function.
                """)
            .CloseError();
        }

        if (Current().Type == TokenType.Identifier)
            Identifier();

        else if (Current().Type == TokenType.Numeric)
            Number();

        else if (Current().Type == TokenType.String)
            StringLiteral();

        Expected("TO");
        if (Current().Type != TokenType.Identifier)
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                Unexpected {Current().Type.Display(false)}.
                """)
            .WithSourceLine(Current(), $"""
                Expected only data item identifiers.
                """)
            .CloseError();
        }

        while (Current().Type == TokenType.Identifier)
            Identifier();
    }

    private static void OpenStatement()
    {
        Expected("OPEN");
        Choice("INPUT", "OUTPUT", "I-O", "EXTEND");

        if (CurrentEquals("SHARING"))
        {
            Expected("SHARING");
            Optional("WITH");
            if (CurrentEquals("ALL"))
            {
                Expected("ALL");
                Optional("OTHER");
            }
            else if (CurrentEquals("NO"))
            {
                Expected("NO");
                Optional("OTHER");
            }
            else if (CurrentEquals("READ"))
            {
                Expected("READ");
                Expected("ONLY");
            }
            else
            {
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                    Unexpected {Current().Type.Display(false)}.
                    """)
                .WithSourceLine(Current(), $"""
                    Expected ALL OTHER, NO OTHER or READ ONLY.
                    """)
                .WithNote("""
                    One of them must be specified in the SHARING phrase.
                    """)
                .CloseError();
            }
        }

        if (CurrentEquals("RETRY"))
        {
            Common.RetryPhrase();
        }

        Identifier();
        if (CurrentEquals("WITH", "NO"))
        {
            Optional("WITH");
            Expected("NO");
            Expected("REWIND");
        }

        while (CurrentEquals(TokenType.Identifier))
        {
            Identifier();
            if (CurrentEquals("WITH", "NO"))
            {
                Optional("WITH");
                Expected("NO");
                Expected("REWIND");
            }
        }

        while (CurrentEquals("INPUT", "OUTPUT", "I-O", "EXTEND"))
        {
            Choice("INPUT", "OUTPUT", "I-O", "EXTEND");

            if (CurrentEquals("SHARING"))
            {
                Expected("SHARING");
                Optional("WITH");
                if (CurrentEquals("ALL"))
                {
                    Expected("ALL");
                    Optional("OTHER");
                }
                else if (CurrentEquals("NO"))
                {
                    Expected("NO");
                    Optional("OTHER");
                }
                else if (CurrentEquals("READ"))
                {
                    Expected("READ");
                    Expected("ONLY");
                }
                else
                {
                    ErrorHandler
                    .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                        Unexpected {Current().Type.Display(false)}.
                        """)
                    .WithSourceLine(Current(), $"""
                        Expected ALL OTHER, NO OTHER or READ ONLY.
                        """)
                    .WithNote("""
                        One of them must be specified in the SHARING phrase.
                        """)
                    .CloseError();
                }
            }

            if (CurrentEquals("RETRY"))
            {
                Common.RetryPhrase();
            }

            Identifier();
            if (CurrentEquals("WITH", "NO"))
            {
                Optional("WITH");
                Expected("NO");
                Expected("REWIND");
            }

            while (CurrentEquals(TokenType.Identifier))
            {
                Identifier();
                if (CurrentEquals("WITH", "NO"))
                {
                    Optional("WITH");
                    Expected("NO");
                    Expected("REWIND");
                }
            }

        }
    }

    private static void DivideStatement()
    {
        bool isConditional = false;

        Expected("DIVIDE");
        switch (Current().Type)
        {
            case TokenType.Identifier:
                Identifier();
                break;

            case TokenType.Numeric:
                Number();
                break;

            default:
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                    Unexpected {Current().Type.Display(false)}.
                    """)
                .WithSourceLine(Current(), $"""
                    Expected an identifier or numeric literal.
                    """)
                .CloseError();
                break;
        }

        if ((CurrentEquals("BY") || CurrentEquals("INTO")) && LookaheadEquals(2, "GIVING") && !LookaheadEquals(4, "REMAINDER"))
        {
            Choice("BY", "INTO");
            switch (Current().Type)
            {
                case TokenType.Identifier:
                    Identifier();
                    break;

                case TokenType.Numeric:
                    Number();
                    break;

                default:
                    ErrorHandler
                    .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                        Unexpected {Current().Type.Display(false)}.
                        """)
                    .WithSourceLine(Current(), $"""
                        Expected an identifier or numeric literal.
                        """)
                    .CloseError();
                    break;
            }

            Expected("GIVING");
            if (Current().Type != TokenType.Identifier)
            {
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                    Unexpected {Current().Type.Display(false)}.
                    """)
                .WithSourceLine(Current(), $"""
                    Expected an identifier.
                    """)
                .CloseError();
            }

            while (Current().Type == TokenType.Identifier)
                Identifier();
        }
        else if ((CurrentEquals("BY") || CurrentEquals("INTO")) && LookaheadEquals(2, "GIVING") && LookaheadEquals(4, "REMAINDER"))
        {
            Choice("BY", "INTO");
            switch (Current().Type)
            {
                case TokenType.Identifier:
                    Identifier();
                    break;

                case TokenType.Numeric:
                    Number();
                    break;

                default:
                    ErrorHandler
                    .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                        Unexpected {Current().Type.Display(false)}.
                        """)
                    .WithSourceLine(Current(), $"""
                        Expected an identifier or numeric literal.
                        """)
                    .CloseError();
                    break;
            }

            Expected("GIVING");
            Identifier();
            Expected("REMAINDER");
            Identifier();
        }
        else if (CurrentEquals("INTO"))
        {
            Expected("INTO");
            if (Current().Type != TokenType.Identifier)
            {
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                    Unexpected {Current().Type.Display(false)}.
                    """)
                .WithSourceLine(Current(), $"""
                    Expected an identifier.
                    """)
                .CloseError();
            }

            while (Current().Type == TokenType.Identifier)
                Identifier();
        }
        else
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                Unexpected {Current().Type.Display(false)}.
                """)
            .WithSourceLine(Current(), $"""
                Expected BY or INTO reserved words.
                """)
            .CloseError();
        }

        Common.SizeError(ref isConditional);

        if (isConditional)
            Expected("END-MULTIPLY");
    }

    private static void DeleteStatement()
    {
        bool isConditional = false;
        bool isFile = false;

        Expected("DELETE");
        if (CurrentEquals("FILE"))
        {
            isFile = true;
            Expected("FILE");
            Optional("OVERRIDE");
            Identifier();
            while (Current().Type == TokenType.Identifier)
                Identifier();
        }
        else if (Current().Type == TokenType.Identifier)
        {
            Identifier();
            Expected("RECORD");
        }

        if (CurrentEquals("RETRY"))
            Common.RetryPhrase();

        if (!isFile)
            Common.InvalidKey(ref isConditional);

        if (isFile)
            Common.OnException(ref isConditional);

        if (isConditional)
            Expected("END-DELETE");
    }

    private static void EvaluateStatement()
    {
        var conditions = new List<EvaluateOperand>();
        var conditionsIndex = 0;
        Expected("EVALUATE");

        conditions.Add(Common.SelectionSubject());
        while (CurrentEquals("ALSO"))
        {
            Expected("ALSO");
            conditions.Add(Common.SelectionSubject());
        }

        Expected("WHEN");
        Common.SelectionObject(conditions[conditionsIndex]);
        conditionsIndex++;

        while (CurrentEquals("ALSO"))
        {
            Expected("ALSO");
            Common.SelectionObject(conditions[conditionsIndex]);
            conditionsIndex++;
        }
        conditionsIndex = 0;

        Statements.WithoutSections(true);

        while (CurrentEquals("WHEN") && !LookaheadEquals(1, "OTHER"))
        {
            Expected("WHEN");
            Common.SelectionObject(conditions[conditionsIndex]);
            conditionsIndex++;

            while (CurrentEquals("ALSO"))
            {
                Expected("ALSO");
                Common.SelectionObject(conditions[conditionsIndex]);
                conditionsIndex++;
            }
            conditionsIndex = 0;

            Statements.WithoutSections(true);
        }

        if (CurrentEquals("WHEN") && LookaheadEquals(1, "OTHER"))
        {
            Expected("WHEN");
            Expected("OTHER");
            Statements.WithoutSections(true);
        }

        Expected("END-EVALUATE");
    }

    private static void ExitStatement()
    {
        Expected("EXIT");
        if (CurrentEquals("PERFORM"))
        {
            Expected("PERFORM");
            Optional("CYCLE");
        }
        else if (CurrentEquals("PARAGRAPH"))
            Expected("PARAGRAPH");

        else if (CurrentEquals("SECTION"))
            Expected("SECTION");

        else if (CurrentEquals("PROGRAM"))
        {
            Expected("PROGRAM");
            if (CurrentEquals("RAISING"))
            {
                Expected("RAISING");
                if (CurrentEquals("EXCEPTION"))
                {
                    Expected("EXCEPTION");
                    Identifier();
                }
                else if (CurrentEquals("LAST"))
                {
                    Expected("LAST");
                    Optional("EXCEPTION");
                }
                else
                    Identifier();
            }
        }
    }

    private static void FreeStatement()
    {
        Expected("FREE");
        Identifier();
        while (CurrentEquals(TokenType.Identifier)) Identifier();

        if (!CurrentEquals("."))
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                Unexpected {Current().Type.Display(false)}.
                """)
            .WithSourceLine(Current(), $"""
                Expected a data item defined with the BASED clause.
                """)
            .CloseError();
        }

    }

    private static void GenerateStatement()
    {
        Expected("GENERATE");
        Identifier();
    }

    private static void GoStatement()
    {
        Expected("GO");
        Optional("TO");
        Identifier();
        if (CurrentEquals("DEPENDING") || Current().Type == TokenType.Identifier)
        {
            while (CurrentEquals(TokenType.Identifier)) Identifier();

            Expected("DEPENDING");
            Optional("ON");
            Identifier();
        }
    }

    private static void GobackStatement()
    {
        Expected("GOBACK");
        Common.RaisingStatus();
    }

    private static void CommitStatement()
    {
        Expected("COMMIT");
    }

    private static void CloseStatement()
    {
        Expected("CLOSE");
        if (Current().Type == TokenType.Identifier)
        {
            Identifier();
            if (CurrentEquals("REEL") || CurrentEquals("UNIT"))
            {
                Expected(Current().Value);

                if (CurrentEquals("FOR") || CurrentEquals("REMOVAL"))
                {
                    Optional("FOR");
                    Expected("REMOVAL");
                }
            }
            else if (CurrentEquals("WITH") || CurrentEquals("NO"))
            {
                Optional("WITH");
                Expected("NO");
                Expected("REWIND");
            }
        }
        else
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                Unexpected {Current().Type.Display(false)}.
                """)
            .WithSourceLine(Current(), $"""
                Expected a file connector name.
                """)
            .CloseError();
        }

        while (Current().Type == TokenType.Identifier)
        {
            Identifier();
            if (CurrentEquals("REEL", "UNIT"))
            {
                Expected(Current().Value);

                if (CurrentEquals("FOR", "REMOVAL"))
                {
                    Optional("FOR");
                    Expected("REMOVAL");
                }
            }
            else if (CurrentEquals("WITH", "NO"))
            {
                Optional("WITH");
                Expected("NO");
                Expected("REWIND");
            }
        }

        if (!CurrentEquals("."))
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                Unexpected {Current().Type.Display(false)}.
                """)
            .WithSourceLine(Current(), $"""
                Expected a file connector name.
                """)
            .CloseError();
        }
    }

    private static void CancelStatement()
    {
        Expected("CANCEL");
        if (Current().Type == TokenType.Identifier)
            Identifier();

        else if (Current().Type == TokenType.String)
            StringLiteral();

        else
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                Unexpected {Current().Type.Display(false)}.
                """)
            .WithSourceLine(Current(), $"""
                Expected an alphanumeric or national literal, or a REPOSITORY paragraph program name.
                """)
            .CloseError();

            Continue();
        }

        while (Current().Type == TokenType.Identifier || Current().Type == TokenType.String)
        {
            if (Current().Type == TokenType.Identifier)
                Identifier();

            if (Current().Type == TokenType.String)
                StringLiteral();
        }

        if (!CurrentEquals("."))
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                Unexpected {Current().Type.Display(false)}.
                """)
            .WithSourceLine(Current(), $"""
                Expected an alphanumeric or national literal, or a REPOSITORY paragraph program name.
                """)
            .CloseError();
        }
    }

    private static void PerformStatement()
    {
        bool isExceptionChecking = false;
        bool isInline = false;

        Expected("PERFORM");
        if (CurrentEquals(TokenType.Identifier))
        {
            Identifier();
            if (CurrentEquals("THROUGH", "THRU"))
            {
                Choice("THROUGH", "THRU");
                Identifier();
            }

            if (CurrentEquals(TokenType.Identifier, TokenType.Numeric))
            {
                Common.TimesPhrase();
            }
            else if (CurrentEquals("WITH", "TEST", "VARYING", "UNTIL"))
            {
                Common.WithTest();
                if (CurrentEquals("VARYING"))
                {
                    Common.VaryingPhrase();
                }
                else if (CurrentEquals("UNTIL"))
                {
                    Common.UntilPhrase();
                }
            }
        }
        else
        {
            if (CurrentEquals("LOCATION") || CurrentEquals("WITH") && LookaheadEquals(1, "LOCATION"))
            {
                isExceptionChecking = true;
                Optional("WITH");
                Expected("LOCATION");
            }
            else if (CurrentEquals(TokenType.Identifier, TokenType.Numeric))
            {
                isInline = true;

                Common.TimesPhrase();
            }
            else if (CurrentEquals("TEST", "VARYING", "UNTIL") || CurrentEquals("WITH") && LookaheadEquals(1, "TEST"))
            {
                isInline = true;

                Common.WithTest();
                if (CurrentEquals("VARYING"))
                {
                    Common.VaryingPhrase();
                }
                else if (CurrentEquals("UNTIL"))
                {
                    Common.UntilPhrase();
                }
            }

            Statements.WithoutSections(true);

            if (isInline && CurrentEquals("WHEN"))
            {
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                    Unexpected {Current().Type.Display(false)}.
                    """)
                .WithSourceLine(Current(), $"""
                    An inline PERFORM cannot contain an exception checking WHEN phrase.
                    """)
                .CloseError();

                AnchorPoint("END-PERFORM");
            }
            
            if (isExceptionChecking || !isInline && CurrentEquals("WHEN"))
            {
                isExceptionChecking = true;

                Expected("WHEN");
                if (CurrentEquals("EXCEPTION") && LookaheadEquals(1, TokenType.Identifier))
                {
                    Expected("EXCEPTION");

                    Identifier();
                    while (CurrentEquals(TokenType.Identifier))
                        Identifier();

                    Statements.WithoutSections(true);
                }
                else if (CurrentEquals("EXCEPTION"))
                {
                    Expected("EXCEPTION");
                    Choice("INPUT", "OUTPUT", "IO", "EXTEND");
                    Statements.WithoutSections(true);
                }
                else if (CurrentEquals(TokenType.Identifier) && LookaheadEquals(1, "FILE"))
                {
                    Identifier();
                    Expected("FILE");
                    Identifier();

                    while (CurrentEquals(TokenType.Identifier))
                        Identifier();
                }
                else if (CurrentEquals(TokenType.Identifier) && !LookaheadEquals(1, "FILE"))
                {
                    Identifier();
                    while (CurrentEquals(TokenType.Identifier))
                        Identifier();
                }

                while (CurrentEquals("WHEN"))
                {
                    Expected("WHEN");
                    if (CurrentEquals("EXCEPTION") && LookaheadEquals(1, TokenType.Identifier))
                    {
                        Expected("EXCEPTION");

                        Identifier();
                        while (CurrentEquals(TokenType.Identifier))
                            Identifier();

                        Statements.WithoutSections(true);
                    }
                    else if (CurrentEquals("EXCEPTION"))
                    {
                        Expected("EXCEPTION");
                        Choice("INPUT", "OUTPUT", "IO", "EXTEND");
                        Statements.WithoutSections(true);
                    }
                    else if (CurrentEquals(TokenType.Identifier) && LookaheadEquals(1, "FILE"))
                    {
                        Identifier();
                        Expected("FILE");
                        Identifier();

                        while (CurrentEquals(TokenType.Identifier))
                            Identifier();
                    }
                    else if (CurrentEquals(TokenType.Identifier) && !LookaheadEquals(1, "FILE"))
                    {
                        Identifier();
                        while (CurrentEquals(TokenType.Identifier))
                            Identifier();
                    }
                }
            }

            if (isExceptionChecking && CurrentEquals("WHEN") && LookaheadEquals(1, "OTHER"))
            {
                Expected("WHEN");
                Expected("OTHER");
                Optional("EXCEPTION");

                Statements.WithoutSections(true);
            }

            if (isExceptionChecking && CurrentEquals("WHEN", "COMMON"))
            {
                Optional("WHEN");
                Expected("COMMON");
                Optional("EXCEPTION");

                Statements.WithoutSections(true);
            }

            if (isExceptionChecking && CurrentEquals("FINALLY"))
            {
                Expected("FINALLY");

                Statements.WithoutSections(true);
            }

            Expected("END-PERFORM");
        }
    }

    private static void RaiseStatement()
    {
        Expected("RAISE");
        if (CurrentEquals("EXCEPTION"))
        {
            Expected("EXCEPTION");
            Identifier();
        }
        else
            Identifier();
    }

    private static void ReadStatement()
    {
        bool isSequential = false;
        bool isConditional = false;

        Expected("READ");
        Identifier();
        if (CurrentEquals("NEXT", "PREVIOUS"))
        {
            Expected(Current().Value);
            isSequential = true;
        }

        Expected("RECORD");
        if (CurrentEquals("INTO"))
        {
            Expected("INTO");
            Identifier();
        }

        if (CurrentEquals("ADVANCING"))
        {
            Expected("ADVANCING");
            Optional("ON");
            Expected("LOCK");
            isSequential = true;
        }
        else if (CurrentEquals("IGNORING"))
        {
            Expected("IGNORING");
            Expected("LOCK");
        }
        else if (CurrentEquals("RETRY"))
        {
            Common.RetryPhrase();
        }

        if (CurrentEquals("WITH", "LOCK"))
        {
            Optional("WITH");
            Expected("LOCK");
        }
        else if (CurrentEquals("WITH", "NO"))
        {
            Optional("WITH");
            Expected("NO");
            Expected("LOCK");
        }

        if (!isSequential && CurrentEquals("KEY"))
        {
            Expected("KEY");
            Optional("IS");
            Identifier();
        }

        if (!isSequential && CurrentEquals("INVALID", "NOT"))
        {
            Common.InvalidKey(ref isConditional);
        }
        else if (isSequential && CurrentEquals("AT", "END", "NOT"))
        {
            Common.AtEnd(ref isConditional);
        }

        if (isConditional) Expected("END-READ");
    }

    private static void ReceiveStatement()
    {
        bool isConditional = false;

        Expected("RECEIVE");
        Optional("FROM");
        Identifier();
        Expected("GIVING");
        Identifier();
        Identifier();

        if (CurrentEquals("CONTINUE"))
        {
            Expected("CONTINUE");
            Optional("AFTER");
            if (CurrentEquals("MESSAGE"))
            {
                Expected("MESSAGE");
                Expected("RECEIVED");
            }
            else
            {
                Common.Arithmetic("SECONDS");
                Optional("SECONDS");
            }
        }

        Common.OnException(ref isConditional);

        if (isConditional) Expected("END-RECEIVE");

    }

    private static void ReleaseStatement()
    {
        Expected("RELEASE");
        Identifier();

        if (CurrentEquals("FROM"))
        {
            Expected("FROM");
            if (Current().Type == TokenType.String)
                StringLiteral();

            else if (Current().Type == TokenType.Numeric)
                Number();

            else
                Identifier();
        }
    }

    private static void ReturnStatement()
    {
        bool isConditional = false;

        Expected("RETURN");
        Identifier();
        Expected("RECORD");
        if (CurrentEquals("INTO"))
        {
            Expected("INTO");
            Identifier();
        }

        Common.AtEnd(ref isConditional);

        if (isConditional)
            Expected("END-RETURN");
    }

    private static void RewriteStatement()
    {
        bool isConditional = false;
        bool isFile = false;

        Expected("REWRITE");
        if (CurrentEquals("FILE"))
        {
            isFile = true;
            Expected("FILE");
            Identifier();
        }
        else
            Identifier();

        Expected("RECORD");
        if (CurrentEquals("FROM") || isFile)
        {
            Expected("FROM");

            if (Current().Type == TokenType.Identifier)
                Identifier();

            else if (Current().Type == TokenType.Numeric)
                Number();

            else
                StringLiteral();
        }

        Common.RetryPhrase();
        if (CurrentEquals("WITH") || CurrentEquals("LOCK") || CurrentEquals("NO"))
        {
            Optional("WITH");
            if (CurrentEquals("LOCK"))
            {
                Expected("LOCK");
            }
            else
            {
                Expected("NO");
                Expected("LOCK");
            }
        }

        Common.InvalidKey(ref isConditional);

        if (isConditional)
            Expected("END-REWRITE");
    }

    private static void ResumeStatement()
    {
        Expected("RESUME");
        Optional("AT");
        if (CurrentEquals("NEXT"))
        {
            Expected("NEXT");
            Expected("STATEMENT");
        }
        else
        {
            Identifier();
        }
    }

    private static void RollbackStatement()
    {
        Expected("ROLLBACK");
    }

    private static void SearchStatement()
    {
        Expected("SEARCH");
        if (!CurrentEquals("ALL"))
        {
            Identifier();
            if (CurrentEquals("VARYING"))
            {
                Expected("VARYING");
                Identifier();
            }

            if (CurrentEquals("AT", "END"))
            {
                Optional("AT");
                Expected("END");
                Statements.WithoutSections(true);
            }

            if (!CurrentEquals("WHEN"))
            {
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                    Unexpected {Current().Type.Display(false)}.
                    """)
                .WithSourceLine(Current(), $"""
                    Expected at least one WHEN condition phrase.
                    """)
                .CloseError();
            }

            while (CurrentEquals("WHEN"))
            {
                Expected("WHEN");
                Common.Condition();
                if (CurrentEquals("NEXT") && LookaheadEquals(1, "SENTENCE"))
                {
                    ErrorHandler
                    .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                        Unsupported phrase: NEXT SENTENCE is an archaic feature.
                        """)
                    .WithSourceLine(Current(), $"""
                        The CONTINUE statement can be used to accomplish the same functionality.
                        """)
                    .CloseError();

                    AnchorPoint("WHEN", "END-SEARCH");
                }

                Statements.WithoutSections(true);
            }

            Expected("END-SEARCH");
            return;
        }

        Expected("ALL");
        Identifier();
        if (CurrentEquals("AT", "END"))
        {
            Optional("AT");
            Expected("END");
            Statements.WithoutSections(true);
        }

        Expected("WHEN");
        Identifier();
        if (CurrentEquals("IS", "EQUAL", "="))
        {
            Optional("IS");
            if (CurrentEquals("EQUAL"))
            {
                Expected("EQUAL");
                Optional("TO");
            }
            else
            {
                Expected("=");
            }
        }

        if (CurrentEquals(TokenType.Identifier, TokenType.String, TokenType.Numeric) && LookaheadEquals(1, TokenType.Symbol))
        {
            Common.Arithmetic();
        }
        else if (CurrentEquals(TokenType.Identifier) && !LookaheadEquals(1, TokenType.Symbol))
        {
            Identifier();
        }
        else if (CurrentEquals(TokenType.Numeric))
        {
            Number();
        }
        else
        {
            StringLiteral();
        }

        while (CurrentEquals("AND"))
        {
            Expected("AND");
            Identifier();
            if (CurrentEquals("IS", "EQUAL", "="))
            {
                Optional("IS");
                if (CurrentEquals("EQUAL"))
                {
                    Expected("EQUAL");
                    Optional("TO");
                }
                else
                {
                    Expected("=");
                }
            }

            if (CurrentEquals(TokenType.Identifier, TokenType.String, TokenType.Numeric) && LookaheadEquals(1, TokenType.Symbol))
            {
                Common.Arithmetic();
            }
            else if (CurrentEquals(TokenType.Identifier) && !LookaheadEquals(1, TokenType.Symbol))
            {
                Identifier();
            }
            else if (CurrentEquals(TokenType.Numeric))
            {
                Number();
            }
            else
            {
                StringLiteral();
            }

        }

        if (CurrentEquals("NEXT") && LookaheadEquals(1, "SENTENCE"))
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                Unsupported phrase: NEXT SENTENCE is an archaic feature.
                """)
            .WithSourceLine(Current(), $"""
                The CONTINUE statement can be used to accomplish the same functionality.
                """)
            .CloseError();

            AnchorPoint("END-SEARCH");
        }

        Statements.WithoutSections(true);
        Expected("END-SEARCH");
    }

    private static void SendStatement()
    {
        bool isConditional = false;
        Expected("SEND");
        Optional("TO");

        if (LookaheadEquals(3, "RETURNING"))
        {
            if (CurrentEquals(TokenType.String))
            {
                StringLiteral();
            }
            else
            {
                Identifier();
            }

            Expected("FROM");
            Identifier();
            Expected("RETURNING");
            Identifier();
        }
        else
        {
            Identifier();
            Expected("FROM");
            Identifier();

            if (CurrentEquals("RAISING"))
            {
                Expected("RAISING");
                if (CurrentEquals("LAST"))
                {
                    Expected("LAST");
                    Optional("EXCEPTION");
                }
                else
                {
                    Expected("EXCEPTION");
                    Identifier();
                }
            }
        }

        Common.OnException(ref isConditional);

        if (isConditional) Expected("END-SEND");
    }

    private static void SetStatement()
    {
        Expected("SET");

        if (CurrentEquals(TokenType.Identifier) || CurrentEquals("SIZE", "ADDRESS"))
        {
            // TODO: This needs to be fixed to lookup a qualified reference
            DataEntry dataItem = new(Current(), EntryType.DataDescription);

            if (CurrentEquals(TokenType.Identifier) && LookaheadEquals(1, "UP", "DOWN", "TO"))
            {
                Identifier();
                if (CurrentEquals("UP"))
                {
                    Expected("UP");
                    Expected("BY");
                }
                else if (CurrentEquals("DOWN"))
                {
                    Expected("DOWN");
                    Expected("BY");
                }
                else
                {
                    Expected("TO");
                }

                if (CurrentEquals(TokenType.Identifier))
                {
                    Identifier();
                }
                else
                {
                    Common.Arithmetic();
                }
            }
            else if (CurrentEquals(TokenType.Identifier) && LookaheadEquals(1, "TO") && LookaheadEquals(2, "LOCALE"))
            {
                Identifier();
                Expected("TO");
                Expected("LOCALE");
                Choice("LC_ALL", "LOCALE");
            }
            else if (dataItem.Usage == UsageType.MessageTag)
            {
                Identifier();
                Expected("TO");
                if (CurrentEquals("NULL"))
                {
                    Expected("NULL");
                }
                else
                {
                    Identifier();
                }
            }
            else if (dataItem[DataClause.DynamicLength] || CurrentEquals("SIZE"))
            {
                if (CurrentEquals("SIZE"))
                {
                    Expected("SIZE");
                    Optional("OF");
                }

                Identifier();
                Expected("TO");
                if (CurrentEquals(TokenType.Numeric))
                {
                    Number();
                }
                else
                {
                    Common.Arithmetic();
                }
            }
            else if (dataItem.Usage == UsageType.DataPointer || CurrentEquals("ADDRESS"))
            {
                bool hasAddress = false;

                if (CurrentEquals("ADDRESS"))
                {
                    hasAddress = true;
                    Expected("ADDRESS");
                    Optional("OF");
                    Identifier();
                }
                else
                {
                    Identifier();
                }

                while (CurrentEquals(TokenType.Identifier) || CurrentEquals("ADDRESS"))
                {
                    if (CurrentEquals("ADDRESS"))
                    {
                        hasAddress = true;
                        Expected("ADDRESS");
                        Optional("OF");
                        Identifier();
                    }
                    else
                    {
                        Identifier();
                    }
                }

                if (hasAddress || CurrentEquals("TO"))
                {
                    Expected("TO");
                    Identifier();
                }
                else if (!hasAddress && CurrentEquals("UP", "DOWN"))
                {
                    Choice("UP", "DOWN");
                    Expected("BY");
                    Common.Arithmetic();
                }
            }
            else if (dataItem.Usage is UsageType.Index)
            {

                Identifier();
                bool checkUsage = true;

                while (CurrentEquals(TokenType.Identifier))
                    Identifier();

                if (CurrentEquals("TO"))
                {
                    Expected("TO");
                    if (CurrentEquals(TokenType.Identifier))
                    {
                        Identifier();
                    }
                    else
                    {
                        Common.Arithmetic();
                    }
                }
                else if (checkUsage && CurrentEquals("UP", "DOWN"))
                {
                    Choice("UP", "DOWN");
                    Expected("BY");
                    Common.Arithmetic();
                }

            }
        }
        else if (CurrentEquals("LAST"))
        {
            Expected("LAST");
            Expected("EXCEPTION");
            Expected("TO");
            Expected("OFF");
        }
        else if (CurrentEquals("LOCALE"))
        {
            Expected("LOCALE");
            if (CurrentEquals("USER-DEFAULT"))
            {
                Expected("USER-DEFAULT");
            }
            else
            {
                Common.SetLocale();
            }

            Expected("TO");
            if (CurrentEquals(TokenType.Identifier))
            {
                Identifier();
            }
            else
            {
                Choice("USER-DEFAULT", "SYSTEM-DEFAULT");
            }
        }

    }

    private static void SortStatement()
    {
        Expected("SORT");
        Identifier();

        Optional("ON");
        if (CurrentEquals("ASCENDING"))
        {
            Expected("ASCENDING");
        }
        else
        {
            Expected("DESCENDING");
        }

        Optional("KEY");

        if (!CurrentEquals(TokenType.Identifier))
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                Unexpected {Current().Type.Display(false)}.
                """)
            .WithSourceLine(Current(), $"""
                The ON ASCENDING / DESCENDING KEY phrase must only contain key names.
                """)
            .CloseError();
        }

        Identifier();
        while (Current().Type == TokenType.Identifier)
            Identifier();


        while (CurrentEquals("ON", "ASCENDING", "DESCENDING"))
        {
            Optional("ON");
            if (CurrentEquals("ASCENDING"))
            {
                Expected("ASCENDING");
            }
            else
            {
                Expected("DESCENDING");
            }

            Optional("KEY");

            if (!CurrentEquals(TokenType.Identifier))
            {
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                    Unexpected {Current().Type.Display(false)}.
                    """)
                .WithSourceLine(Current(), $"""
                    The ON ASCENDING / DESCENDING KEY phrase must only contain key names.
                    """)
                .CloseError();
            }

            Identifier();
            while (Current().Type == TokenType.Identifier)
                Identifier();
        }

        if (CurrentEquals("WITH", "DUPLICATES"))
        {
            Optional("WITH");
            Expected("DUPLICATES");
            Optional("IN");
            Optional("ORDER");
        }

        if (CurrentEquals("COLLATING", "SEQUENCE"))
        {
            Optional("COLLATING");
            Expected("SEQUENCE");
            if (CurrentEquals("IS") && LookaheadEquals(1, TokenType.Identifier) || CurrentEquals(TokenType.Identifier))
            {
                Optional("IS");
                Identifier();

                if (CurrentEquals(TokenType.Identifier)) Identifier();
            }
            else
            {
                if (!CurrentEquals("FOR", "ALPHANUMERIC", "NATIONAL"))
                {
                    ErrorHandler
                    .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                        Unexpected {Current().Type.Display(false)}.
                        """)
                    .WithSourceLine(Current(), $"""
                        Expected an alphabet name or at least one FOR ALPHANUMERIC and FOR NATIONAL phrases.
                        """)
                    .CloseError();

                    AnchorPoint(TokenContext.IsStatement, "USING");
                }

                Common.ForAlphanumericForNational();
            }
        }

        if (CurrentEquals("INPUT"))
        {
            Expected("INPUT");
            Expected("PROCEDURE");
            Optional("IS");
            Identifier();

            if (CurrentEquals("THROUGH", "THRU"))
            {
                Choice("THROUGH", "THRU");
                Identifier();
            }
        }
        else
        {
            Expected("USING");
            Identifier();
            while (Current().Type == TokenType.Identifier)
                Identifier();
        }

        if (CurrentEquals("OUTPUT"))
        {
            Expected("OUTPUT");
            Expected("PROCEDURE");
            Optional("IS");
            Identifier();

            if (CurrentEquals("THROUGH", "THRU"))
            {
                Choice("THROUGH", "THRU");
                Identifier();
            }
        }
        else
        {
            Expected("GIVING");
            Identifier();
            while (Current().Type == TokenType.Identifier)
                Identifier();
        }
    }

    private static void StartStatement()
    {
        bool isConditional = false;

        Expected("START");
        Identifier();

        if (CurrentEquals("FIRST"))
        {
            Expected("FIRST");
        }
        else if (CurrentEquals("LAST"))
        {
            Expected("LAST");
        }
        else if (CurrentEquals("KEY"))
        {
            Expected("KEY");
            Common.StartRelationalOperator();
            Identifier();

            if (CurrentEquals("WITH", "LENGTH"))
            {
                Optional("WITH");
                Expected("LENGTH");
                Common.Arithmetic(".");
            }

        }

        Common.InvalidKey(ref isConditional);

        if (isConditional) Expected("END-START");
    }

    private static void StopStatement()
    {
        Expected("STOP");
        Expected("RUN");
        if (CurrentEquals("WITH") || CurrentEquals("NORMAL") || CurrentEquals("ERROR"))
        {
            Optional("WITH");
            Choice("NORMAL", "ERROR");
            Optional("STATUS");
            switch (Current().Type)
            {
                case TokenType.Identifier:
                    Identifier();
                    break;
                case TokenType.Numeric:
                    Number();
                    break;
                case TokenType.String:
                    StringLiteral();
                    break;
            }
        }
    }

    private static void StringStatement()
    {
        bool isConditional = false;

        Expected("STRING");
        if (CurrentEquals(TokenType.Identifier)) Identifier();

        else StringLiteral();

        while (CurrentEquals(TokenType.Identifier, TokenType.String))
        {
            if (CurrentEquals(TokenType.Identifier)) Identifier();

            else StringLiteral();
        }

        Expected("DELIMITED");
        Optional("BY");
        if (CurrentEquals(TokenType.Identifier)) Identifier();

        else if (CurrentEquals("SIZE")) Expected("SIZE");

        else StringLiteral();

        while (CurrentEquals(TokenType.Identifier, TokenType.String))
        {
            if (CurrentEquals(TokenType.Identifier)) Identifier();

            else StringLiteral();

            while (CurrentEquals(TokenType.Identifier, TokenType.String))
            {
                if (CurrentEquals(TokenType.Identifier)) Identifier();

                else StringLiteral();
            }

            Expected("DELIMITED");
            Optional("BY");
            if (CurrentEquals(TokenType.Identifier)) Identifier();

            else if (CurrentEquals("SIZE")) Expected("SIZE");

            else StringLiteral();
        }

        Expected("INTO");
        Identifier();

        if (CurrentEquals("WITH", "POINTER"))
        {
            Optional("WITH");
            Expected("POINTER");
            Identifier();
        }

        Common.OnOverflow(ref isConditional);

        if (isConditional) Expected("END-STRING");
    }

    private static void SuppressStatement()
    {
        Expected("SUPPRESS");
        Optional("PRINTING");
    }

    private static void TerminateStatement()
    {
        Expected("TERMINATE");
        if (Current().Type != TokenType.Identifier)
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                Unexpected {Current().Type.Display(false)}.
                """)
            .WithSourceLine(Current(), $"""
                Expected a report entry identifier defined in the report section.
                """)
            .CloseError();
        }
        Identifier();
        while (Current().Type == TokenType.Identifier)
            Identifier();

        if (!CurrentEquals("."))
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                Unexpected {Current().Type.Display(false)}.
                """)
            .WithSourceLine(Current(), $"""
                Expected a report entry identifier defined in the report section.
                """)
            .CloseError();
        }
    }

    private static void UnlockStatement()
    {
        Expected("UNLOCK");
        Identifier();
        Choice("RECORD", "RECORDS");
    }

    private static void UnstringStatement()
    {
        bool isConditional = false;

        Expected("UNSTRING");
        Identifier();

        if (CurrentEquals("DELIMITED"))
        {
            Expected("DELIMITED");
            Optional("BY");
            if (CurrentEquals("ALL")) Expected("ALL");

            if (CurrentEquals(TokenType.Identifier))
            {
                Identifier();
            }
            else
            {
                StringLiteral();
            }

            while (CurrentEquals("OR"))
            {
                Expected("OR");
                if (CurrentEquals("ALL")) Expected("ALL");

                if (CurrentEquals(TokenType.Identifier))
                {
                    Identifier();
                }
                else
                {
                    StringLiteral();
                }
            }
        }

        Expected("INTO");
        Identifier();

        if (CurrentEquals("DELIMITER"))
        {
            Expected("DELIMITER");
            Optional("IN");
            Identifier();
        }
        if (CurrentEquals("COUNT"))
        {
            Expected("COUNT");
            Optional("IN");
            Identifier();
        }

        while (CurrentEquals(TokenType.Identifier))
        {
            Identifier();

            if (CurrentEquals("DELIMITER"))
            {
                Expected("DELIMITER");
                Optional("IN");
                Identifier();
            }
            if (CurrentEquals("COUNT"))
            {
                Expected("COUNT");
                Optional("IN");
                Identifier();
            }
        }

        if (CurrentEquals("WITH", "POINTER"))
        {
            Optional("WITH");
            Expected("POINTER");
            Identifier();
        }

        if (CurrentEquals("TALLYING"))
        {
            Expected("TALLYING");
            Optional("IN");
            Identifier();
        }

        Common.OnOverflow(ref isConditional);

        if (isConditional) Expected("END-UNSTRING");
    }

    private static void ValidateStatement()
    {
        Expected("VALIDATE");
        if (Current().Type != TokenType.Identifier)
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                Unexpected {Current().Type.Display(false)}.
                """)
            .WithSourceLine(Current(), $"""
                Expected an identifier.
                """)
            .CloseError();
        }
        Identifier();
        while (Current().Type == TokenType.Identifier)
            Identifier();

        if (!CurrentEquals("."))
        {
            ErrorHandler
            .Build(ErrorType.Analyzer, ConsoleColor.Red, 5, $"""
                Unexpected {Current().Type.Display(false)}.
                """)
            .WithSourceLine(Current(), $"""
                Expected an identifier.
                """)
            .CloseError();
        }
    }

    private static void WriteStatement()
    {
        bool isSequential = false;
        bool isConditional = false;

        Expected("WRITE");
        if (CurrentEquals("FILE"))
        {
            Expected("FILE");
            Identifier();
        }
        else
        {
            Identifier();
        }

        if (CurrentEquals("FROM"))
        {
            if (CurrentEquals(TokenType.Identifier))
            {
                Identifier();
            }
            else
            {
                StringLiteral();
            }
        }

        if (CurrentEquals("BEFORE", "AFTER"))
        {
            isSequential = true;

            Common.WriteBeforeAfter();
            Optional("ADVANCING");
            if (CurrentEquals("PAGE"))
            {
                Expected("PAGE");
                // Missing mnemonic-name handling
            }
            else if (CurrentEquals(TokenType.Identifier, TokenType.Numeric))
            {
                if (CurrentEquals(TokenType.Identifier)) Identifier();

                else Number();

                if (CurrentEquals("LINE", "LINES"))
                {
                    Expected(Current().Value);
                }
            }
        }

        Common.RetryPhrase();

        if (CurrentEquals("WITH", "LOCK"))
        {
            Optional("WITH");
            Expected("LOCK");
        }
        else if (CurrentEquals("WITH", "NO"))
        {
            Optional("WITH");
            Expected("NO");
            Expected("LOCK");
        }

        if (isSequential && CurrentEquals("AT", "NOT", "END-OF-PAGE", "EOP"))
        {
            Common.AtEndOfPage(ref isConditional);
        }
        else if (!isSequential && CurrentEquals("INVALID", "NOT"))
        {
            Common.InvalidKey(ref isConditional);
        }

        if (isConditional) Expected("END-WRITE");
    }

    private static void ParseParagraph()
    {
        Identifier(Current());
    }

    // Different from Using(), this one is for the CALL and INVOKE only.
    private static void StatementUsing(bool byValue, bool byContent)
    {
        while (CurrentEquals("BY", "REFERENCE", "VALUE") || CurrentEquals(TokenType.Identifier))
        {   
            if (CurrentEquals(TokenType.Identifier))
            {
                Identifier();
                while (CurrentEquals(TokenType.Identifier) || CurrentEquals("OPTIONAL"))
                {
                    if (CurrentEquals("OPTIONAL"))
                    {
                        Expected("OPTIONAL");
                    }
                    // TODO: Reimplement parameter item resolution
                    Identifier();
                }  
            }

            if (CurrentEquals("BY") && !LookaheadEquals(1, "VALUE", "REFERENCE", "CONTENT"))
            {
                ErrorHandler
                .Build(ErrorType.Analyzer, ConsoleColor.Red, 128,"""
                    Using phrase, missing keyword.
                    """)
                .WithSourceLine(Current(), """
                    Expected REFERENCE, VALUE or CONTENT after this token
                    """)
                .CloseError();

                AnchorPoint(TokenContext.IsStatement, "RETURNING", ".");
            }

            if (CurrentEquals("REFERENCE") || CurrentEquals("BY") && LookaheadEquals(1, "REFERENCE"))
            {
                Optional("BY");
                Expected("REFERENCE");

                if (CurrentEquals("OPTIONAL"))
                {
                    Expected("OPTIONAL");
                }

                if (!CurrentEquals(TokenType.Identifier))
                {
                    ErrorHandler
                    .Build(ErrorType.Analyzer, ConsoleColor.Red, 128,"""
                        Using phrase, missing identifier.
                        """)
                    .WithSourceLine(Current(), """
                        BY REFERENCE phrase must contain at least one data item name.
                        """)
                    .CloseError();
                }
                
                // TODO: Reimplement parameter item resolution

                Identifier();
                while (CurrentEquals(TokenType.Identifier) || CurrentEquals("OPTIONAL"))
                {
                    if (CurrentEquals("OPTIONAL"))
                    {
                        Expected("OPTIONAL");
                    }
                    // TODO: Reimplement parameter item resolution
                    Identifier();
                }
            }

            if (byValue && CurrentEquals("VALUE") || CurrentEquals("BY") && LookaheadEquals(1, "VALUE"))
            {
                Optional("BY");
                Expected("VALUE");
                if (!CurrentEquals(TokenType.Identifier))
                {
                    ErrorHandler
                    .Build(ErrorType.Analyzer, ConsoleColor.Red, 128,"""
                        Using phrase, missing identifier.
                        """)
                    .WithSourceLine(Current(), """
                        BY VALUE phrase must contain at least one data item name.
                        """)
                    .CloseError();
                }
                
                // TODO: Reimplement parameter item resolution
                Identifier();
                while (CurrentEquals(TokenType.Identifier))
                {
                    // TODO: Reimplement parameter item resolution
                    Identifier();
                }
            }

            if (byContent && CurrentEquals("CONTENT") || CurrentEquals("BY") && LookaheadEquals(1, "CONTENT"))
            {
                Optional("BY");
                Expected("CONTENT");
                if (!CurrentEquals(TokenType.Identifier))
                {
                    ErrorHandler
                    .Build(ErrorType.Analyzer, ConsoleColor.Red, 128,"""
                        Using phrase, missing identifier.
                        """)
                    .WithSourceLine(Current(), """
                        BY CONTENT phrase must contain at least one data item name.
                        """)
                    .CloseError();
                }
                
                // TODO: Reimplement parameter item resolution
                Identifier();
                while (CurrentEquals(TokenType.Identifier))
                {
                    // TODO: Reimplement parameter item resolution
                    Identifier();
                }
            }
        }
    }

    // This method handles COBOL's slightly inconsistent separator period rules.
    // Statements that are nested inside another statement cannot end with a separator period,
    // since that separator period would mean the end of the containing statement and not the contained statement.
    private static void ScopeTerminator(bool isNested)
    {
        if (isNested) return;

        if (CurrentEquals(".", ". "))
        {
            Expected(".");
            return;
        }

        if (!CurrentEquals(TokenContext.IsStatement))
        {
            Expected(".");
        }
    }

}
