using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LittleCompiler
{
    /// <name>ForwardReference</name>
    /// <type>Class</type>
    /// <summary>
    /// This class keeps track of references to symbols when the address
    /// of the symbol in the code is not yet known.
    /// </summary>
    public class ForwardReference
    {
        private int instructionLocation;
        public int InstructionLocation
        {
            get { return instructionLocation; }
            set { instructionLocation = value; }
        }

        private Symbol reference;
        public Symbol Reference
        {
            get { return reference; }
            set { reference = value; }
        }

        /// <name>ForwardReference</name>
        /// <type>Constructor</type>
        /// <summary>
        /// Creates a new forward reference object with initialization.
        /// </summary>
        /// <param name="instructionLocation">Instruction initialization parameter</param>
        /// <param name="reference">Reference symbol parameter</param>
        public ForwardReference(int instructionLocation, Symbol reference) : base()
        {
            this.instructionLocation = instructionLocation;
            this.reference = reference;
        }
    }
}
