using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LittleCompiler
{
    #region Token Type Enumeration
    /// <name>TokenType</name>
    /// <type>Enum</type>
    /// <summary>
    /// This enumeration creates a strongly-typed specification for 
    /// available token types.
    /// </summary>
    public enum TokenType
    {
        ADD_OP,      // + and -
        MULT_OP,     // * and /
        L_PAREN,     // (
        R_PAREN,     // )
        L_BRACKET,   // [
        R_BRACKET,   // ]
        SEMICOLON,   // ;
        REL_OP,      // <, <=, =, >, >=, !=
        COMMA,       // ,
        ASSIGN,      // :=
        CALL,        // call
        ELSE,        // else
        END,         // endif, endproc, endprogram, endwhile
        IF_WHILE,    // if, while
        PROC,        // proc
        THEN_DO,     // then, do
        WRITE,       // write
        END_FILE,    // end-of-file
        PROGRAM,     // program
        CONST,       // const
        VAR,         // var
        L_BRACE,     // {
        R_BRACE,     // }
        LITERAL,     // numeric literal
        SYMBOL,      // symbol
        NO_TOKEN     // null token
    }
    #endregion

    /// <name>Token</name>
    /// <type>Class</type>
    /// <summary>
    /// This class creates a definition for token objects with properties
    /// that represent the type of the token and the lexeme of the token.
    /// It has an overloaded constructor that allows an instance to be
    /// created with or without initialization.  Cloning the token object
    /// is also allowed.
    /// </summary>
    public class Token : ICloneable
    {
        protected TokenType type;
        public TokenType Type
        {
            get { return type; }
            set { type = value; }
        }

        protected string lexeme;
        public string Lexeme
        {
            get { return lexeme; }
            set { lexeme = value; }
        }

        /// <name>Token</name>
        /// <type>Constructor</type>
        /// <summary>
        /// Creates a new token with no initialization.
        /// </summary>
        public Token() : base() {}

        /// <name>Token</name>
        /// <type>Constructor</type>
        /// <summary>
        /// Creates a new token and initializes the type and lexeme.
        /// </summary>
        /// <param name="type">Token type</param>
        /// <param name="lexeme">Token lexeme</param>
        public Token(TokenType type, string lexeme) : base()
        {
            this.type = type;
            this.lexeme = lexeme;
        }

        public object Clone()
        {
            Token copyToken = new Token(this.Type, this.Lexeme);
            return copyToken;
        }
    }
}