using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LittleCompiler
{
    /// <name>SymbolType</name>
    /// <type>Enum</type>
    /// <summary>
    /// This enumeration provides data types for user-defined symbols.
    /// </summary>
    public enum SymbolType
    {
        CONST_TYPE,
        VAR_TYPE,
        ARRAY_TYPE,
        PROC_TYPE,
        FORWARD_PROC,
        PROGRAM_NAME,
        UNKNOWN_TYPE
    }

    /// <name>Symbol</name>
    /// <type>Class</type>
    /// <summary>
    /// This class creates a definition for symbol tokens.  It derives from the
    /// token base class and adds additional properties for the address of the
    /// symbol and the type of the symbol defined by the SymbolType enumeration.
    /// </summary>
    public class Symbol : Token
    {
        private int address;
        public int Address
        {
            get { return address; }
            set { address = value; }
        }

        private SymbolType symbolType;
        public SymbolType SymbolType
        {
            get { return symbolType; }
            set { symbolType = value; }
        }

        /// <name>Symbol</name>
        /// <type>Constructor</type>
        /// <summary>
        /// Creates a new symbol and initializes the address and symbol type.
        /// Calls the base token constructor to set the type and lexeme properties.
        /// </summary>
        /// <param name="lexeme">Symbol lexeme</param>
        public Symbol(string lexeme) : base(TokenType.SYMBOL, lexeme)
        {
            this.address = -1;
            this.symbolType = SymbolType.UNKNOWN_TYPE;
        }
    }
}
