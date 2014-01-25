using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LittleCompiler
{
    #region Status Enumeration
    /// <name>Status</name>
    /// <type>Enum</type>
    /// <summary>
    /// This enumeration creates a strongly-typed specification for 
    /// available status types during parser execution.
    /// </summary>
    public enum Status
    {
        CONTINUE,
        FREEZE,
        EXIT
    }
    #endregion

    /// <name>Parser</name>
    /// <type>Class</type>
    /// <summary>
    /// This class is responsible for managing the overall functionality of the
    /// compiler including instantiating and using the scanner and emitter classes.
    /// The parser also handles the step between scanning and emitting where the
    /// tokens passed by the scanner are analyzed and the program structure is built.
    /// </summary>
    public class Parser
    {
        Token currentOperator, nextOperator, operand;
        Literal literal;
        Symbol symbol;
        bool inExpression;
        Status status;
        int nextLocation;
        Dictionary<string, Symbol> symbolTable;
        Dictionary<string, Literal> literalTable;
        Stack<Token> operatorStack;
        Stack<Structure> structureStack;
        Stack<ForwardReference> forwardRefStack;
        Emitter jvm;
        Scanner source;

        #region CONO Table Declaration
        string[,] CONO = 
        {
            { "EQ", "LT", "LT", "GT", "LT", "GT", "GT", "GT", "GT", "XX", "XX", "XX", "XX", "XX", "XX", "GT", "XX" },
            { "GT", "EQ", "LT", "GT", "LT", "GT", "GT", "GT", "GT", "XX", "XX", "XX", "XX", "XX", "XX", "GT", "XX" },
            { "LT", "LT", "LT", "PA", "LT", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX" },
            { "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX" },
            { "LT", "LT", "LT", "XX", "LT", "SU", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX" },
            { "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX" },
            { "XX", "XX", "XX", "XX", "AS", "XX", "XX", "XX", "XX", "AS", "NO", "EB", "EB", "BC", "NO", "XX", "WR" },
            { "LT", "LT", "LT", "XX", "LT", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "EE", "XX" },
            { "LT", "LT", "LT", "XX", "LT", "XX", "EE", "XX", "EE", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX" },
            { "LT", "LT", "LT", "XX", "LT", "XX", "EE", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX" },
            { "XX", "XX", "XX", "XX", "XX", "XX", "CA", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX" },
            { "XX", "XX", "XX", "XX", "AS", "XX", "XX", "XX", "XX", "AS", "NO", "XX", "EB", "BC", "XX", "XX", "WR" },
            { "XX", "XX", "XX", "XX", "XX", "XX", "NO", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX" },
            { "LT", "LT", "LT", "XX", "LT", "XX", "XX", "EE", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX" },
            { "XX", "XX", "XX", "XX", "XX", "XX", "PR", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX" },
            { "XX", "XX", "XX", "XX", "AS", "XX", "XX", "XX", "XX", "AS", "NO", "XX", "XX", "BC", "XX", "XX", "WR" },
            { "LT", "LT", "LT", "XX", "LT", "XX", "EE", "XX", "EE", "XX", "XX", "XX", "XX", "XX", "XX", "XX", "XX" }
        };
        #endregion

        #region Public Methods
        /// <name>Parser</name>
        /// <type>Constructor</type>
        /// <summary>
        /// Creates a parser object.
        /// </summary>
        public Parser() : base()
        {
            // Initalize class variables
            currentOperator = new Token(TokenType.NO_TOKEN, String.Empty);
            nextOperator = new Token(TokenType.NO_TOKEN, String.Empty);
            operand = null;
            literal = null;
            symbol = null;
            inExpression = false;
            nextLocation = 1;
            symbolTable = new Dictionary<string, Symbol>();
            literalTable = new Dictionary<string, Literal>();
            operatorStack = new Stack<Token>();
            structureStack = new Stack<Structure>();
            forwardRefStack = new Stack<ForwardReference>();
        }

        /// <name>Compile</name>
        /// <type>Method</type>
        /// <summary>
        /// Main method that controls program compilation.
        /// </summary>
        /// <param name="sourceFileName">Input source file</param>
        public void Compile(string sourceFilePath)
        {
            // Initialize scanner and emitter objects
            source = new Scanner();
            jvm = new Emitter();

            // Open the source file and handle some initalization
            source.OpenSourceFile(sourceFilePath);
            Advance();
            Advance();

            // Make sure the first three tokens are legal
            if (currentOperator.Lexeme != "program" || nextOperator.Lexeme != ";" || symbol == null)
            {
                Compiler.ThrowCompilerException("Illegal program definition");
            }

            symbolTable[operand.Lexeme].Type = TokenType.PROGRAM;
            symbolTable[operand.Lexeme].SymbolType = SymbolType.PROGRAM_NAME;
            structureStack.Push(new Structure(StructType.PROGRAM_STRUCT));
            Advance();

            // Check for constant declarations
            if (nextOperator.Type == TokenType.CONST)
            {
                CompileConstants();
            }

            // Check for variable declarations
            if (nextOperator.Type == TokenType.VAR)
            {
                CompileVariables();
            }

            // Compile the rest of the program and output object code
            CompileProcedures();
            jvm.OutputObjectFile();
            source.CloseSourceFile();

            Compiler.WriteToDebug("Symbol Table Output");
            foreach (Symbol item in symbolTable.Values)
            {
                if (item.Lexeme.Length < 8)
                {
                    Compiler.WriteToDebug("Lexeme: " + item.Lexeme + "\t\tType: " +
                        item.SymbolType + "  \tAddress: " + item.Address);
                }
                else
                {
                    Compiler.WriteToDebug("Lexeme: " + item.Lexeme + "\tType: " +
                        item.SymbolType + "  \tAddress: " + item.Address);
                }
            }
            Compiler.WriteToDebug(String.Empty);

            Compiler.WriteToDebug("Literal Table Output");
            foreach (Literal item in literalTable.Values)
            {
                Compiler.WriteToDebug("Lexeme: " + item.Lexeme);
            }
            Compiler.WriteToDebug(String.Empty);
        }
        #endregion

        #region Private Methods
        /// <name>GetTokenTypeIndex</name>
        /// <type>Method</type>
        /// <summary>
        /// Converts an operator type into an integer of the appropriate position
        /// on the CONO table for a code generator call.
        /// </summary>
        /// <param name="type">Type of the operator</param>
        /// <returns>Integer representing a position in the CONO table</returns>
        private int GetTokenTypeIndex(TokenType type)
        {
            switch (type)
            {
                case TokenType.ADD_OP:
                    return 0;
                case TokenType.MULT_OP:
                    return 1;
                case TokenType.L_PAREN:
                    return 2;
                case TokenType.R_PAREN:
                    return 3;
                case TokenType.L_BRACKET:
                    return 4;
                case TokenType.R_BRACKET:
                    return 5;
                case TokenType.SEMICOLON:
                    return 6;
                case TokenType.REL_OP:
                    return 7;
                case TokenType.COMMA:
                    return 8;
                case TokenType.ASSIGN:
                    return 9;
                case TokenType.CALL:
                    return 10;
                case TokenType.ELSE:
                    return 11;
                case TokenType.END:
                    return 12;
                case TokenType.IF_WHILE:
                    return 13;
                case TokenType.PROC:
                    return 14;
                case TokenType.THEN_DO:
                    return 15;
                case TokenType.WRITE:
                    return 16;
                default:
                    Compiler.ThrowCompilerException("Token passed to code generator is not a valid type");
                    return -1;
            }
        }

        /// <name>CodeGenerator</name>
        /// <type>Method</type>
        /// <summary>
        /// Calls the appropriate code generator function based on the set of
        /// two-character codes used in the CONO table.  Each valid operator
        /// type is converted to an integer representing a position in the table,
        /// which returns the string that identifies which code generator to call.
        /// </summary>
        /// <param name="currentOp">Current operator token</param>
        /// <param name="nexttOp">Next operator token</param>
        private void CodeGenerator(Token currentOp, Token nextOp)
        {
            if (currentOp.Type == TokenType.CALL && nextOp.Type != TokenType.SEMICOLON)
            {
                Compiler.ThrowCompilerException("No semicolon found following procedure call");
            }
            else if (currentOp.Type == TokenType.PROC && nextOp.Type != TokenType.SEMICOLON)
            {
                Compiler.ThrowCompilerException("No semicolon found following procedure definition");
            }
            else if (currentOp.Type == TokenType.END && nextOp.Type != TokenType.SEMICOLON)
            {
                Compiler.ThrowCompilerException("No semicolon found following end of control block");
            }

            int first = GetTokenTypeIndex(currentOp.Type);
            int second = GetTokenTypeIndex(nextOp.Type);
            string code = CONO[first,second];

            switch (code)
            {
                case "AS":
                    Assignment();
                    break;
                case "BC":
                    BeginCondition();
                    break;
                case "CA":
                    CallProcedure();
                    break;
                case "EB":
                    EndBlock();
                    break;
                case "EE":
                    EndExpression();
                    break;
                case "EQ":
                    EqualityForOperators();
                    break;
                case "GT":
                    GreaterThanForOperators();
                    break;
                case "LT":
                    LessThanForOperators();
                    break;
                case "NO":
                    NoOperation();
                    break;
                case "PA":
                    Parentheses();
                    break;
                case "PR":
                    ProcedureDefinition();
                    break;
                case "SU":
                    Subscript();
                    break;
                case "WR":
                    Write();
                    break;
                default:
                    Error();
                    break;
            }

        }

        /// <name>Advance</name>
        /// <type>Method</type>
        /// <summary>
        /// Advances the current and next operators from the input file and 
        /// populates the operand token between the operators if one exists.
        /// </summary>
        private void Advance()
        {
            // Advance one token
            currentOperator = (Token)nextOperator.Clone();
            Token peekToken = source.GetNextToken();

            // Make sure operand, symbol and literal are null
            operand = null;
            symbol = null;
            literal = null;

            if (peekToken.Type == TokenType.SYMBOL)
            {
                // If the new token is a symbol then set operand to that token
                operand = (Token)peekToken.Clone();

                // Now check to see if the symbol is already defined - if
                // it is then assign to symbol, otherwise add it to the table
                if (symbolTable.ContainsKey(peekToken.Lexeme))
                {
                    symbol = symbolTable[peekToken.Lexeme];
                }
                else
                {
                    symbolTable.Add(peekToken.Lexeme, new Symbol(peekToken.Lexeme));
                    symbol = symbolTable[peekToken.Lexeme];
                }

                // Finally get the next token
                nextOperator = source.GetNextToken();
            }
            else if (peekToken.Type == TokenType.LITERAL)
            {
                // If the new token is a literal then set operand to that token
                operand = (Token)peekToken.Clone();

                // Now check to see if the literal is already defined - if
                // it is then assign to literal, otherwise add it to the table
                if (literalTable.ContainsKey(peekToken.Lexeme))
                {
                    literal = literalTable[peekToken.Lexeme];
                }
                else
                {
                    literalTable.Add(peekToken.Lexeme, new Literal(peekToken.Lexeme));
                    literal = literalTable[peekToken.Lexeme];
                }

                // Finally get the next token
                nextOperator = source.GetNextToken();
            }
            else
            {
                // Set the next operator to the token we just retrieved
                nextOperator = (Token)peekToken.Clone();
            }

            // Output debug information
            Compiler.WriteToDebug("Parser - Advance called (line " + Compiler.LineNumber + ")");
            Compiler.WriteToDebug("   current operator:\t" + currentOperator.Lexeme);
            if (operand != null)
            {
                Compiler.WriteToDebug("   operand:\t\t" + operand.Lexeme);
            }
            Compiler.WriteToDebug("   next operator:\t" + nextOperator.Lexeme);
            Compiler.WriteToDebug(String.Empty);
        }

        /// <name>CompileCondition</name>
        /// <type>Method</type>
        /// <summary>
        /// Generates code for a conditional statement.
        /// </summary>
        private void CompileCondition()
        {
            // First part of the condition
            CompileExpression();

            // Save the operator and handle the second part of the condition
            string relationalOperator = nextOperator.Lexeme;
            Advance();
            CompileExpression();

            // Now generate the appropriate statement for the operator above
            switch (relationalOperator)
            {
                case "<":
                    jvm.Emit(Opcode.IF_ICMPGE, 0);
                    break;
                case "<=":
                    jvm.Emit(Opcode.IF_ICMPGT, 0);
                    break;
                case "=":
                    jvm.Emit(Opcode.IF_ICMPNE, 0);
                    break;
                case ">=":
                    jvm.Emit(Opcode.IF_ICMPLT, 0);
                    break;
                case ">":
                    jvm.Emit(Opcode.IF_ICMPLE, 0);
                    break;
                case "<>":
                    jvm.Emit(Opcode.IF_ICMPEQ, 0);
                    break;
                default:
                    Compiler.ThrowCompilerException("Invalid relational operator");
                    break;
            }
        }

        /// <name>CompileConstants</name>
        /// <type>Method</type>
        /// <summary>
        /// Generates code for all constant declarations in the program.
        /// </summary>
        private void CompileConstants()
        {
            // Keep processing until we reach the end of the declarations
            while (nextOperator.Type != TokenType.SEMICOLON)
            {
                // Advance to the first constant declaration and record the address and
                // symbol type for the constant name in the symbol table
                Advance();

                if (symbol.Address == -1)
                {
                    symbolTable[operand.Lexeme].Address = nextLocation;
                    symbolTable[operand.Lexeme].SymbolType = SymbolType.CONST_TYPE;
                }
                else
                {
                    Compiler.ThrowCompilerException("Symbol has already been defined");
                }

                // Now generate bytecode for the declaration
                if (nextOperator.Lexeme == "=")
                {
                    // If the next operator is an equal sign then we have an initalization
                    Advance();
                    PushConstant(literalTable[operand.Lexeme].NumberValue);
                }
                else if (nextOperator.Type == TokenType.COMMA || nextOperator.Type == TokenType.SEMICOLON)
                {
                    // Otherwise initalize to zero
                    PushConstant(0);
                }
                else
                {
                    Compiler.ThrowCompilerException("Invalid operator found in constant declarations");
                }

                // Generate code to store the inital value at the constant's address
                jvm.ChooseOp(Opcode.ISTORE, Opcode.ISTORE_0, nextLocation);

                // Finally increase the location index since we assigned the current address
                nextLocation++;
            }

            Advance();
        }

        /// <name>CompileExpression</name>
        /// <type>Method</type>
        /// <summary>
        /// Compile arithmetic expression.
        /// </summary>
        private void CompileExpression()
        {
            // Perform initalization and generate code for the first operand
            inExpression = true;
            status = Status.FREEZE;

            if (operand != null)
            {
                if (operand.Lexeme == "length")
                {
                    // First we need to save the new current operator
                    operatorStack.Push(currentOperator);

                    // Push array length onto the stack
                    Advance();
                    jvm.ChooseOp(Opcode.ALOAD, Opcode.ALOAD_0, symbol.Address);
                    jvm.Emit(Opcode.ARRAYLENGTH);
                    Advance();

                    // Now retrieve the current operator that we started with
                    currentOperator = operatorStack.Pop();
                }
                else
                {
                    PushOperand(operand);
                }
            }

            // Loop continues until we reach the end of the expression
            while (inExpression == true)
            {
                if (status == Status.EXIT)
                {
                    inExpression = false;
                }
                else if (status == Status.FREEZE)
                {
                    status = Status.CONTINUE;
                }
                else
                {
                    // Advance but save the current operator on the stack
                    operatorStack.Push(currentOperator);
                    Advance();

                    // Generate code to push operand onto the stack
                    if (operand != null)
                    {
                        if (operand.Lexeme == "length")
                        {
                            // First we need to save the new current operator
                            operatorStack.Push(currentOperator);

                            // Push array length onto the stack
                            Advance();
                            jvm.ChooseOp(Opcode.ALOAD, Opcode.ALOAD_0, symbol.Address);
                            jvm.Emit(Opcode.ARRAYLENGTH);
                            Advance();

                            // Now retrieve the current operator that we started with
                            currentOperator = operatorStack.Pop();
                        }
                        else
                        {
                            PushOperand(operand);
                        }
                    }
                }

                // Call the appropriate code generator for the current operators
                CodeGenerator(currentOperator, nextOperator);
            }
        }

        /// <name>CompileProcedures</name>
        /// <type>Method</type>
        /// <summary>
        /// Compile program procedures.
        /// </summary>
        private void CompileProcedures()
        {
            status = Status.CONTINUE;

            // If there's a procedure declaration in the program then we need a new
            // symbol to define the start on the main program's execution when we get
            // to it, so create the symbol now and save it as a forward reference
            if (nextOperator.Type == TokenType.PROC)
            {
                Symbol main = new Symbol("$MAIN");
                symbolTable.Add(main.Lexeme, main);
                SaveForwardReference(main, jvm.ProgramCounter);
                jvm.Emit(Opcode.GOTO, 0);
            }

            // Keep going until the end block of the program structure is reached
            while (structureStack.Count > 0)
            {
                CodeGenerator(currentOperator, nextOperator);
                Advance();
            }

            // Pop the forward references left and fill with the appropriate address
            FillForwardReferences();
        }

        /// <name>CompileVariables</name>
        /// <type>Method</type>
        /// <summary>
        /// Generates code for all variable declarations in the program.
        /// </summary>
        private void CompileVariables()
        {
            // Keep processing until we reach the end of the declarations
            while (nextOperator.Type != TokenType.SEMICOLON)
            {
                // Advance to the next variable declaration and record the address
                Advance();

                if (symbol.Address == -1)
                {
                    symbolTable[operand.Lexeme].Address = nextLocation;
                }
                else
                {
                    Compiler.ThrowCompilerException("Symbol has already been defined");
                }

                // Now generate bytecode for the declaration
                if (nextOperator.Type == TokenType.L_BRACKET)
                {
                    // Array declaration
                    Token array = new Token(TokenType.SYMBOL, operand.Lexeme);
                    symbolTable[operand.Lexeme].SymbolType = SymbolType.ARRAY_TYPE;
                    Advance();

                    // Push the number representing the size of the array
                    if (literal.NumberValue > 0 && literal.NumberValue < 101)
                    {
                        PushConstant(literal.NumberValue);
                    }
                    else
                    {
                        Compiler.ThrowCompilerException("Illegal array declaration");
                    }

                    jvm.Emit(Opcode.NEWARRAY, (byte)10);
                    jvm.ChooseOp(Opcode.ASTORE, Opcode.ASTORE_0, nextLocation);
                    Advance();

                    if (nextOperator.Type == TokenType.REL_OP)
                    {
                        // If the next operator is an equal sign then we have an initalization
                        Advance();

                        int arrayIndex = 0;
                        while (nextOperator.Type != TokenType.R_BRACE)
                        {
                            // Keep going until we come to the closing bracket
                            Advance();
                            PushOperand(array);
                            PushConstant(arrayIndex);
                            PushOperand(operand);
                            jvm.Emit(Opcode.IASTORE);
                            arrayIndex++;
                        }

                        // Advance past the right brace and comma
                        Advance();
                    }
                }
                else
                {
                    // Variable declaration
                    symbolTable[operand.Lexeme].SymbolType = SymbolType.VAR_TYPE;

                    if (nextOperator.Lexeme == "=")
                    {
                        // If the next operator is an equal sign then we have an initalization
                        Advance();

                        if (operand.Type == TokenType.LITERAL)
                        {
                            // Assignment is a literal
                            PushConstant(literalTable[operand.Lexeme].NumberValue);
                        }
                        else
                        {
                            // Assignment is a previously-defined symbol
                            PushOperand(operand);
                        }
                    }
                    else if (nextOperator.Type == TokenType.COMMA || nextOperator.Type == TokenType.SEMICOLON)
                    {
                        // Variable declaration with no initalization so initalize to zero
                        PushConstant(0);
                    }
                    else
                    {
                        Compiler.ThrowCompilerException("Invalid operator found in variable declarations");
                    }

                    // Generate code to store the inital value at the variable's address
                    jvm.ChooseOp(Opcode.ISTORE, Opcode.ISTORE_0, nextLocation);
                }

                // Finally increase the location index since we assigned the current address
                nextLocation++;
            }

            Advance();
        }

        /// <name>FillAddress</name>
        /// <type>Method</type>
        /// <summary>
        /// Fill an address for a forward reference.  This is used for procedures
        /// and conditional statements where we generate an instruction but the
        /// correct offset is not yet known so it must be corrected later.
        /// </summary>
        /// <param name="address">Location of address to be filled</param>
        /// <param name="value">Value to fill in</param>
        private void FillAddress(int address, int value)
        {
            jvm.Emit(address + 1, value - address);
        }

        /// <name>FillForwardReferences</name>
        /// <type>Method</type>
        /// <summary>
        /// Pops a forward reference off the stack and modifies the code for that
        /// reference with the correct address offset.
        /// </summary>
        private void FillForwardReferences()
        {
            while (forwardRefStack.Count > 0)
            {
                ForwardReference pop = forwardRefStack.Pop();
                if (pop.Reference.Address > 0)
                {
                    FillAddress(pop.InstructionLocation, pop.Reference.Address);
                }
                else
                {
                    Compiler.ThrowCompilerException("Symbol is referenced but was not defined");
                }
            }
        }

        /// <name>PushConstant</name>
        /// <type>Method</type>
        /// <summary>
        /// Push a constant onto the runtime stack.
        /// </summary>
        /// <param name="constant">Constant being pushed</param>
        private void PushConstant(int constant)
        {
            if (constant <= 5)
            {
                // We have a 1-byte instruction for this
                switch (constant)
                {
                    case 0:
                        jvm.Emit(Opcode.ICONST_0);
                        break;
                    case 1:
                        jvm.Emit(Opcode.ICONST_1);
                        break;
                    case 2:
                        jvm.Emit(Opcode.ICONST_2);
                        break;
                    case 3:
                        jvm.Emit(Opcode.ICONST_3);
                        break;
                    case 4:
                        jvm.Emit(Opcode.ICONST_4);
                        break;
                    case 5:
                        jvm.Emit(Opcode.ICONST_5);
                        break;
                }
            }
            else if (constant <= 127)
            {
                // A 2-byte bipush will work
                jvm.Emit(Opcode.BIPUSH, (byte)constant);
            }
            else if (constant <= 32767)
            {
                // Need to use 3-byte sipush
                jvm.Emit(Opcode.SIPUSH, constant);
            }
            else if (constant <= 2147483647)
            {
                // Constant is larger than 2 bytes - first divide by the
                // 2-byte limit to determine necessary multiplication
                int multiple = constant / 32767;
                
                // Now emit code to push the 2-byte limit and the
                // multiplication level onto the stack and multiply
                // them together
                jvm.Emit(Opcode.SIPUSH, 32767);
                jvm.Emit(Opcode.SIPUSH, multiple);
                jvm.Emit(Opcode.IMUL);

                // Finally the constant's remainder after division
                // needs to be added to the result above
                jvm.Emit(Opcode.SIPUSH, (constant % 32767));
                jvm.Emit(Opcode.IADD);
            }
            else
            {
                Compiler.ThrowCompilerException("Integer out of range");
            }
        }

        /// <name>PushOperand</name>
        /// <type>Method</type>
        /// <summary>
        /// Push an operand onto the runtime stack.
        /// </summary>
        private void PushOperand(Token op)
        {
            if (op.Type == TokenType.LITERAL)
            {
                // Literal value
                PushConstant(literalTable[op.Lexeme].NumberValue);
            }
            else
            {
                // Previously-declared symbol
                if (symbolTable[op.Lexeme].Address != -1)
                {
                    Symbol s = symbolTable[op.Lexeme];
                    if (s.SymbolType == SymbolType.ARRAY_TYPE)
                    {
                        // Emit code to load the array
                        jvm.ChooseOp(Opcode.ALOAD, Opcode.ALOAD_0, s.Address);
                    }
                    else
                    {
                        if (nextOperator.Type != TokenType.L_BRACKET)
                        {
                            // Emit code to load the constant or variable
                            jvm.ChooseOp(Opcode.ILOAD, Opcode.ILOAD_0, s.Address);
                        }
                        else
                        {
                            Compiler.ThrowCompilerException("Subscript given for non-array symbol");
                        }
                    }
                }
                else
                {
                    Compiler.ThrowCompilerException("Reference to undefined symbol");
                }
            }
        }

        /// <name>SaveForwardReference</name>
        /// <type>Method</type>
        /// <summary>
        /// Push a forward reference onto the stack.  This contains a symbol
        /// whose actual address in the code is not yet known.
        /// </summary>
        /// <param name="reference">Reference symbol</param>
        /// <param name="address">Address of the reference symbol</param>
        private void SaveForwardReference(Symbol reference, int address)
        {
            forwardRefStack.Push(new ForwardReference(address, reference));
        }
        #endregion

        #region Code Generator Methods
        /// <name>Assignment</name>
        /// <type>Method</type>
        /// <summary>
        /// Assignment code generator.
        /// </summary>
        private void Assignment()
        {
            if (nextOperator.Type == TokenType.L_BRACKET)
            {
                // Assignment to an array member
                nextOperator.Lexeme = "[[";

                // First push the symbol for the array name
                PushOperand(operand);
                Advance();

                // Handle whatever is between the subscript
                CompileExpression();
                Advance();
                Advance();

                // Now handle whatever is after the assignment operator
                CompileExpression();
                jvm.Emit(Opcode.IASTORE);
            }
            else
            {
                // General assignment statement
                if (symbol.SymbolType != SymbolType.CONST_TYPE)
                {
                    if (symbol.Address != -1)
                    {
                        int symbolAddress = symbol.Address;
                        Advance();
                        CompileExpression();
                        jvm.ChooseOp(Opcode.ISTORE, Opcode.ISTORE_0, symbolAddress);
                    }
                    else
                    {
                        Compiler.ThrowCompilerException("Reference to undefined symbol");
                    }
                }
                else
                {
                    Compiler.ThrowCompilerException("Cannot change the value of a const symbol");
                }
            }
        }

        /// <name>BeginCondition</name>
        /// <type>Method</type>
        /// <summary>
        /// Begin condition code generator.
        /// </summary>
        private void BeginCondition()
        {
            if (nextOperator.Lexeme == "if")
            {
                // Start of if block, push the new structure onto the stack
                Structure ifStruct = new Structure(StructType.IF_STRUCT);
                ifStruct.ConditionLocation = jvm.ProgramCounter;
                Advance();

                // Handle the conditional part of the statement
                CompileCondition();

                // Now set the return location to the beginning of the branch
                ifStruct.JumpLocation = jvm.ProgramCounter - 3;
                structureStack.Push(ifStruct);
            }
            else
            {
                // Start of while block, push the new structure onto the stack
                Structure whileStruct = new Structure(StructType.WHILE_STRUCT);
                whileStruct.ConditionLocation = jvm.ProgramCounter;
                Advance();

                // Handle the conditional part of the statement
                CompileCondition();

                // Now set the return location to the beginning of the branch
                whileStruct.JumpLocation = jvm.ProgramCounter - 3;
                structureStack.Push(whileStruct);
            }
        }

        /// <name>CallProcedure</name>
        /// <type>Method</type>
        /// <summary>
        /// Call procedure code generator.
        /// </summary>
        private void CallProcedure()
        {
            if (symbol != null)
            {
                if (symbol.SymbolType == SymbolType.UNKNOWN_TYPE)
                {
                    // Procedure that hasn't been encountered yet, set the type to
                    // forward proc and save forward reference along with JSR
                    symbol.SymbolType = SymbolType.FORWARD_PROC;
                    SaveForwardReference(symbol, jvm.ProgramCounter);
                    jvm.Emit(Opcode.JSR, 0);
                }
                else if (symbol.SymbolType == SymbolType.FORWARD_PROC)
                {
                    // Procedure was encountered but not yet defined, so save
                    // the forward reference and generate JSR command
                    SaveForwardReference(symbol, jvm.ProgramCounter);
                    jvm.Emit(Opcode.JSR, 0);
                }
                else
                {
                    // Procedure has already been defined so we know the address
                    jvm.Emit(Opcode.JSR, symbol.Address - jvm.ProgramCounter);
                }
            }
            else
            {
                Compiler.ThrowCompilerException("No symbol name provided for procedure call");
            }
        }

        /// <name>EndBlock</name>
        /// <type>Method</type>
        /// <summary>
        /// End block code generator.
        /// </summary>
        private void EndBlock()
        {
            Structure s = structureStack.Pop();
            if (s.Type == StructType.ELSE_STRUCT && nextOperator.Lexeme == "endif")
            {
                // End of an else block
                FillAddress(s.JumpLocation, jvm.ProgramCounter);
            }
            else if (s.Type == StructType.IF_STRUCT && nextOperator.Lexeme == "else")
            {
                // End of an if block but start of else block
                Structure elseStruct = new Structure(StructType.ELSE_STRUCT);
                elseStruct.JumpLocation = jvm.ProgramCounter;
                jvm.Emit(Opcode.GOTO, 0);
                structureStack.Push(elseStruct);
                FillAddress(s.JumpLocation, jvm.ProgramCounter);
            }
            else if (s.Type == StructType.IF_STRUCT && nextOperator.Lexeme == "endif")
            {
                // End of an if block with no else block
                FillAddress(s.JumpLocation, jvm.ProgramCounter);
            }
            else if (s.Type == StructType.WHILE_STRUCT && nextOperator.Lexeme == "endwhile")
            {
                // End of a while block
                jvm.Emit(Opcode.GOTO, s.ConditionLocation - jvm.ProgramCounter);
                FillAddress(s.JumpLocation, jvm.ProgramCounter);
            }
            else if (s.Type == StructType.PROC_STRUCT && nextOperator.Lexeme == "endproc")
            {
                // End of a procedure
                jvm.Emit(Opcode.ASTORE_0);
                jvm.Emit(Opcode.RET, (byte)0);
                symbolTable["$MAIN"].Address = jvm.ProgramCounter;
            }
            else if (s.Type == StructType.PROGRAM_STRUCT && nextOperator.Lexeme == "endprogram")
            {
                // End of the program
                jvm.Emit(Opcode.RETURN);
                status = Status.EXIT;
            }
            else
            {
                Compiler.ThrowCompilerException("Structure type does not match control block");
            }
        }

        /// <name>EndExpression</name>
        /// <type>Method</type>
        /// <summary>
        /// End expression code generator.
        /// </summary>
        private void EndExpression()
        {
            status = Status.EXIT;
        }

        /// <name>EqualityForOperators</name>
        /// <type>Method</type>
        /// <summary>
        /// Equality for operators code generator.
        /// </summary>
        private void EqualityForOperators()
        {
            switch (currentOperator.Lexeme)
            {
                case "+":
                    jvm.Emit(Opcode.IADD);
                    break;
                case "-":
                    jvm.Emit(Opcode.ISUB);
                    break;
                case "*":
                    jvm.Emit(Opcode.IMUL);
                    break;
                case "/":
                    jvm.Emit(Opcode.IDIV);
                    break;
                default:
                    Compiler.ThrowCompilerException("Invalid expression operator");
                    break;
            }

            currentOperator = operatorStack.Pop();
        }

        /// <name>GreaterThanForOperators</name>
        /// <type>Method</type>
        /// <summary>
        /// Greater than for operators code generator.
        /// </summary>
        private void GreaterThanForOperators()
        {
            status = Status.FREEZE;
            EqualityForOperators();
        }

        /// <name>LessThanForOperators(</name>
        /// <type>Method</type>
        /// <summary>
        /// Less than for operators code generator.
        /// </summary>
        private void LessThanForOperators() { }

        /// <name>NoOperation</name>
        /// <type>Method</type>
        /// <summary>
        /// No operation code generator.
        /// </summary>
        private void NoOperation() { }

        /// <name>Parentheses</name>
        /// <type>Method</type>
        /// <summary>
        /// Parentheses code generator.
        /// </summary>
        private void Parentheses()
        {
            Advance();
            currentOperator = operatorStack.Pop();
            status = Status.FREEZE;
        }

        /// <name>ProcedureDefinition</name>
        /// <type>Method</type>
        /// <summary>
        /// Procedure definition code generator.
        /// </summary>
        private void ProcedureDefinition()
        {
            if (symbol.Address == -1)
            {
                symbol.Address = jvm.ProgramCounter;
                symbol.SymbolType = SymbolType.PROC_TYPE;
                structureStack.Push(new Structure(StructType.PROC_STRUCT));
            }
            else
            {
                Compiler.ThrowCompilerException("Symbol has already been defined");
            }
        }

        /// <name>Subscript</name>
        /// <type>Method</type>
        /// <summary>
        /// Subscript code generator.
        /// </summary>
        private void Subscript()
        {
            if (currentOperator.Lexeme == "[[")
            {
                // Part of assignment statement
                EndExpression();
            }
            else
            {
                // Part of reference to a location in the array
                jvm.Emit(Opcode.IALOAD);
                Parentheses();
            }
        }

        /// <name>Write</name>
        /// <type>Method</type>
        /// <summary>
        /// Write code generator.
        /// </summary>
        private void Write()
        {
            while (nextOperator.Type != TokenType.SEMICOLON)
            {
                Advance();
                jvm.Emit(Opcode.GETSTATIC, 6);
                CompileExpression();
                jvm.Emit(Opcode.INVOKEVIRTUAL, 7);
            }
        }

        /// <name>Error</name>
        /// <type>Method</type>
        /// <summary>
        /// Error code generator.
        /// </summary>
        private void Error()
        {
            Compiler.ThrowCompilerException("Invalid code generator function call");
        }
        #endregion
    }
}
