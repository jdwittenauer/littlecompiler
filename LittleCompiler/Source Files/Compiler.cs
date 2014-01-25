using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LittleCompiler
{
    /// <name>Compiler</name>
    /// <type>Class</type>
    /// <summary>
    /// This is a static class used specifically for exceptions thrown by the compiler
    /// and logging debug information.  The class has static members for line number
    /// and debug mode as well as an output file for log writing.  The methods are
    /// public and can be called from any class but no instantiations can be created.
    /// </summary>
    public static class Compiler
    {
        private static StreamWriter debugFile;

        private static bool debugMode = false;
        public static bool DebugMode
        {
            get { return debugMode; }
        }

        private static int lineNumber = 1;
        public static int LineNumber
        {
            get { return lineNumber; }
            set { lineNumber = value; }
        }

        #region Public Methods
        /// <name>StartDebugging</name>
        /// <type>Method</type>
        /// <summary>
        /// Initializes the debugging log file and sets the debug flag to true.  A
        /// StreamWriter object is used because the pipe is one-way (write but not read).
        /// </summary>
        public static void StartDebugging()
        {
            debugMode = true;
            debugFile = new StreamWriter("debug.txt");
        }

        /// <name>EndDebugging</name>
        /// <type>Method</type>
        /// <summary>
        /// Closes the debugging log file and sets the debug flag to false.
        /// </summary>
        public static void EndDebugging()
        {
            debugMode = false;
            debugFile.Close();
        }

        /// <name>WriteToDebug</name>
        /// <type>Method</type>
        /// <summary>
        /// Writes a message to the debugging log file.
        /// </summary>
        /// <param name="message">Text being written to log file</param>
        public static void WriteToDebug(string message)
        {
            if (debugMode == true)
            {
                debugFile.WriteLine(message);
            }
        }

        /// <name>ThrowCompilerException</name>
        /// <type>Method</type>
        /// <summary>
        /// Throws a compiler exception with the included message and references
        /// the current line number of the input file for error detection.
        /// </summary>
        /// <param name="message">Text describing the exception</param>
        public static void ThrowCompilerException(string message)
        {
            if (debugMode == true)
            {
                debugFile.WriteLine("*** Error on line " + lineNumber + ": " + message + " ***");
            }

            throw new CompilerException(lineNumber, message);
        }
        #endregion
    }
}
