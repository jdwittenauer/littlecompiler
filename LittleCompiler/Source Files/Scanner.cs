using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LittleCompiler
{
    /// <name>Scanner</name>
    /// <type>Class</type>
    /// <summary>
    /// This class is responsible for scanning and tokenizing the input file and sending
    /// the tokens on to the parser.  Tokens are defined by a token class with two properties - 
    /// one for the token type (enumeration) and one for the lexeme (string representation).
    /// Each successive token can be obtained by the caller via the GetNextToken method.
    /// </summary>
    public class Scanner
    {
        private Token currentToken;
        private StreamReader sourceFile;
        private Dictionary<string, TokenType> operatorTable;

        #region Public Methods
        /// <name>Scanner</name>
        /// <type>Constructor</type>
        /// <summary>
        /// Creates a scanner object and initializes the operator lookup table and 
        /// global token object for use during execution of the class's public methods.
        /// </summary>
        public Scanner() : base()
        {
            operatorTable = new Dictionary<string, TokenType>();
            operatorTable["+"] = TokenType.ADD_OP;
            operatorTable["-"] = TokenType.ADD_OP;
            operatorTable["*"] = TokenType.MULT_OP;
            operatorTable["/"] = TokenType.MULT_OP;
            operatorTable["("] = TokenType.L_PAREN;
            operatorTable[")"] = TokenType.R_PAREN;
            operatorTable["["] = TokenType.L_BRACKET;
            operatorTable["]"] = TokenType.R_BRACKET;
            operatorTable[";"] = TokenType.SEMICOLON;
            operatorTable["<"] = TokenType.REL_OP;
            operatorTable["<="] = TokenType.REL_OP;
            operatorTable["="] = TokenType.REL_OP;
            operatorTable[">="] = TokenType.REL_OP;
            operatorTable[">"] = TokenType.REL_OP;
            operatorTable["!="] = TokenType.REL_OP;
            operatorTable[","] = TokenType.COMMA;
            operatorTable[":="] = TokenType.ASSIGN;
            operatorTable["call"] = TokenType.CALL;
            operatorTable["else"] = TokenType.ELSE;
            operatorTable["endif"] = TokenType.END;
            operatorTable["endproc"] = TokenType.END;
            operatorTable["endprogram"] = TokenType.END;
            operatorTable["endwhile"] = TokenType.END;
            operatorTable["if"] = TokenType.IF_WHILE;
            operatorTable["while"] = TokenType.IF_WHILE;
            operatorTable["proc"] = TokenType.PROC;
            operatorTable["then"] = TokenType.THEN_DO;
            operatorTable["do"] = TokenType.THEN_DO;
            operatorTable["write"] = TokenType.WRITE;
            operatorTable["program"] = TokenType.PROGRAM;
            operatorTable["const"] = TokenType.CONST;
            operatorTable["var"] = TokenType.VAR;
            operatorTable["{"] = TokenType.L_BRACE;
            operatorTable["}"] = TokenType.R_BRACE;

            currentToken = new Token(TokenType.NO_TOKEN, String.Empty);
        }

        /// <name>OpenSourceFile</name>
        /// <type>Method</type>
        /// <summary>
        /// Opens the source file specified from the user interface.  A StreamReader
        /// object is used because the pipe is one-way (read but not write).
        /// </summary>
        /// <param name="sourceFilePath">Location of the source file</param>
        public void OpenSourceFile(string sourceFilePath)
        {
            sourceFile = new StreamReader(sourceFilePath);
        }

        /// <name>CloseSourceFile</name>
        /// <type>Method</type>
        /// <summary>
        /// Closes the source file previously opened by the scanner.
        /// </summary>
        public void CloseSourceFile()
        {
            sourceFile.Close();
        }

        /// <name>GetNextToken</name>
        /// <type>Method</type>
        /// <summary>
        /// Returns the next token in the file and consumes the token.
        /// </summary>
        /// <returns>Next valid token from the input file</returns>
        public Token GetNextToken()
        {
            if (sourceFile.EndOfStream)
            {
                // If we're at the end of the file stream then return end file token
                currentToken.Type = TokenType.END_FILE;
                currentToken.Lexeme = String.Empty;

                return currentToken;
            }
            else
            {
                int nextChar = sourceFile.Read();

                // Skip over white space but process newline characters by updating
                // the line count (static variable to the compiler class)
                while (Char.IsWhiteSpace((char)nextChar))
                {
                    if (sourceFile.EndOfStream)
                    {
                        currentToken.Type = TokenType.END_FILE;
                        currentToken.Lexeme = String.Empty;
                        return currentToken;
                    }
                    else if (nextChar == '\n')
                    {
                        Compiler.LineNumber++;
                    }

                    nextChar = sourceFile.Read();
                }

                if (Char.IsLetter((char)nextChar))
                {
                    // Symbol token - call ReadSymbol to handle the rest
                    currentToken = ReadSymbol(nextChar);
                }
                else if (Char.IsDigit((char)nextChar))
                {
                    // Literal token - call ReadLiteral to handle the rest
                    currentToken = ReadLiteral(nextChar);
                }
                else if (nextChar == '\'')
                {
                    // Literal token in apostrophes - get the integer representation
                    // of the token and skip over the second apostrophe
                    nextChar = sourceFile.Read();
                    currentToken.Lexeme = nextChar.ToString();
                    currentToken.Type = TokenType.LITERAL;
                    sourceFile.Read();
                }
                else if (nextChar == '/')
                {
                    // We either have a comment or an operator - run a series of checks
                    // to determine which token type we've encountered
                    if (sourceFile.Peek() == '/')
                    {
                        // Comment - skip over the rest of the line then get the next token
                        while (nextChar != '\n')
                        {
                            nextChar = sourceFile.Read();
                        }

                        Compiler.LineNumber++;
                        currentToken = GetNextToken();
                    }
                    else if (sourceFile.Peek() == '*')
                    {
                        // Multi-line comment - keep reading until we encounter the end
                        // of the comment (and update line number on newline characters)
                        // then get the next token
                        while (nextChar != '*' || sourceFile.Peek() != '/')
                        {
                            nextChar = sourceFile.Read();

                            if (nextChar == '\n')
                            {
                                Compiler.LineNumber++;
                            }
                        }

                        sourceFile.Read();
                        currentToken = GetNextToken();
                    }
                    else
                    {
                        // Operator token - check the lookup table to get the type
                        string op = ((char)nextChar).ToString();

                        if (operatorTable.ContainsKey(op))
                        {
                            currentToken.Lexeme = op;
                            currentToken.Type = operatorTable[op];
                        }
                        else
                        {
                            Compiler.ThrowCompilerException("Illegal operator");
                        }
                    }
                }
                else if (sourceFile.Peek() == '=')
                {
                    // Two-character operator token - check the lookup table to get the type
                    string op = ((char)nextChar).ToString() + ((char)sourceFile.Read()).ToString();

                    if (operatorTable.ContainsKey(op))
                    {
                        currentToken.Lexeme = op;
                        currentToken.Type = operatorTable[op];
                    }
                    else
                    {
                        Compiler.ThrowCompilerException("Illegal operator");
                    }
                }
                else
                {
                    // Remaining operator token - check the lookup table to get the type
                    string op = ((char)nextChar).ToString();

                    if (operatorTable.ContainsKey(op))
                    {
                        currentToken.Lexeme = op;
                        currentToken.Type = operatorTable[op];
                    }
                    else
                    {
                        Compiler.ThrowCompilerException("Illegal operator");
                    }
                }

                return currentToken;
            }
        }
        #endregion

        #region Private Methods
        /// <name>IsHexDigit</name>
        /// <type>Method</type>
        /// <summary>
        /// Returns true if the character is a hex character, false if it is not.
        /// </summary>
        /// <param name="testChar">Character being checked</param>
        /// <returns>Boolean value</returns>
        private bool IsHexDigit(int testChar)
        {
            if (testChar == 'a' || testChar == 'b' || testChar == 'c' || testChar == 'd' || testChar == 'e' 
                || testChar == 'f' || testChar == 'A' || testChar == 'B' || testChar == 'C' || testChar == 'D' 
                || testChar == 'E' || testChar == 'F')
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <name>ReadLiteral</name>
        /// <type>Method</type>
        /// <summary>
        /// Advances through the input file and tokenizes literal values.  Literals are
        /// numbers that can be in base 10, base 8 (octal) or base 16 (hex).  Return value
        /// is always in base 10.  If the literal is already in base 10, a string is returned
        /// representing that number.  If the literal is in base 8 or base 16, it is converted
        /// to base 10 and then returned.
        /// </summary>
        /// <param name="firstChar">First character of the token</param>
        /// <returns>Token object with the type and lexeme of the literal</returns>
        private Token ReadLiteral(int firstChar)
        {
            Token newToken = new Token();
            string literal = String.Empty, tempString = String.Empty;
            int numberBase = 0, power = 0, result = 0;

            // If the token starts with a 0 then it is octal, 0x and it is hex
            if (firstChar == '0' && (sourceFile.Peek() == 'x' || sourceFile.Peek() == 'X'))
            {
                sourceFile.Read();
                numberBase = 16;
            }
            else if (firstChar == '0')
            {
                numberBase = 8;
            }
            else
            {
                // First char is part of the literal in base 10 so append it
                tempString += ((char)firstChar).ToString();
                numberBase = 10;
            }

            // Keep reading digits (or hex characters) until we have the entire literal
            while (Char.IsDigit((char)sourceFile.Peek()) || IsHexDigit((char)sourceFile.Peek()))
            {
                tempString += ((char)sourceFile.Read()).ToString();
            }

            if (numberBase == 8)
            {
                // If the number is octal then convert it to base 10 and set the literal
                // to the string value of the base 10 number
                for (int i = tempString.Length - 1; i >= 0; i--)
                {
                    int digit = 0;
                    try
                    {
                        digit = Convert.ToInt32(tempString[i].ToString());
                    }
                    catch
                    {
                        Compiler.ThrowCompilerException("Invalid literal");
                    }

                    if (digit < 8)
                    {
                        result += digit * (int)Math.Pow(8, power);
                        power++;
                    }
                    else
                    {
                        Compiler.ThrowCompilerException("Invalid literal");
                    }
                }

                literal = result.ToString();
            }
            else if (numberBase == 16)
            {
                // If the number is hex then convert it to base 10 and set the literal
                // to the string value of the base 10 number
                for (int i = tempString.Length - 1; i >= 0; i--)
                {
                    char tempChar = Char.ToLower(tempString[i]);
                    int digit = 0;

                    switch (tempChar)
                    {
                        case 'a':
                            digit = 10;
                            break;
                        case 'b':
                            digit = 11;
                            break;
                        case 'c':
                            digit = 12;
                            break;
                        case 'd':
                            digit = 13;
                            break;
                        case 'e':
                            digit = 14;
                            break;
                        case 'f':
                            digit = 15;
                            break;
                        default:
                            digit = Convert.ToInt32(tempChar.ToString());
                            break;
                    }

                    result += digit * (int)Math.Pow(16, power);
                    power++;
                }

                literal = result.ToString();
            }
            else
            {
                // Already in base 10 so no conversion is needed
                for (int i = tempString.Length - 1; i >= 0; i--)
                {
                    if (Char.IsDigit((char)tempString[i]) == false)
                    {
                        Compiler.ThrowCompilerException("Invalid literal");
                    }
                }
                literal = tempString;
            }

            // Return the token
            newToken.Lexeme = literal;
            newToken.Type = TokenType.LITERAL;
            return newToken;
        }

        /// <name>ReadSymbol</name>
        /// <type>Method</type>
        /// <summary>
        /// Advances through the input file and tokenizes character sets classified
        /// as symbols.  The input file is read until a non-letter character is
        /// encountered and the entire token is returned.
        /// </summary>
        /// <param name="firstChar">First character of the token</param>
        /// <returns>Token object with the type and lexeme of the symbol</returns>
        private Token ReadSymbol(int firstChar)
        {
            Token newToken = new Token();
            string symbol = Char.ToLower(((char)firstChar)).ToString();

            // Keep reading letters until we have the entire literal (converting each
            // letter to lowercase along the way)
            while (Char.IsLetter((char)sourceFile.Peek()))
            {
                symbol += Char.ToLower(((char)sourceFile.Read())).ToString();
            }

            // Check to see if we have a reserved word
            if (operatorTable.ContainsKey(symbol))
            {
                newToken.Type = operatorTable[symbol];
            }
            else
            {
                newToken.Type = TokenType.SYMBOL;
            }

            // Return the token
            newToken.Lexeme = symbol;
            return newToken;
        }
        #endregion
    }
}
