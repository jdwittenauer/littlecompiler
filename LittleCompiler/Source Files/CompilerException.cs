using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LittleCompiler
{
    /// <name>CompilerException</name>
    /// <type>Class</type>
    /// <summary>
    /// This class is derived from the exception base class and defines a new exception
    /// type with an additional property that specifies the line on the input file where
    /// the exception occured.
    /// </summary>
    public class CompilerException : Exception
    {
        private int line;

        /// <name>CompilerException</name>
        /// <type>Constructor</type>
        /// <summary>
        /// Creates a new compiler exception with line number and description.
        /// </summary>
        /// <param name="line">Line number the exception occured on</param>
        /// <param name="message">Text describing the exception</param>
        public CompilerException(int line, string message) : base(message) 
        {
            this.line = line;
        }
    }
}
