using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LittleCompiler
{
    /// <name>Literal</name>
    /// <type>Class</type>
    /// <summary>
    /// This class creates a definition for literal tokens.  It derives from the
    /// token base class and adds an additional property for the numeric value of
    /// the literal represented as an integer.
    /// </summary>
    public class Literal : Token
    {
        private int numberValue;
        public int NumberValue
        {
            get { return numberValue; }
            set { numberValue = value; }
        }

        /// <name>Literal</name>
        /// <type>Constructor</type>
        /// <summary>
        /// Creates a new literal and initializes the value by converting the
        /// lexeme to an integer.  Calls the base token constructor to set the
        /// type and lexeme properties.
        /// </summary>
        /// <param name="lexeme">Literal lexeme</param>
        public Literal(string lexeme) : base(TokenType.LITERAL, lexeme)
        {
            this.numberValue = Convert.ToInt32(lexeme);
        }
    }
}
