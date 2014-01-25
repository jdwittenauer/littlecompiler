using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LittleCompiler
{
    /// <name>StructType</name>
    /// <type>Enum</type>
    /// <summary>
    /// This enumeration creates a strongly-typed specification for 
    /// available program structures.
    /// </summary>
    public enum StructType
    {
        IF_STRUCT,
        ELSE_STRUCT,
        WHILE_STRUCT,
        PROC_STRUCT,
        PROGRAM_STRUCT
    }

    /// <name>Structure</name>
    /// <type>Class</type>
    /// <summary>
    /// This class contains addresses of forward references within
    /// if statements and while loops.  It is also used as part of a
    /// stack to determine nesting of control structures.
    /// </summary>
    public class Structure
    {
        private int conditionLocation;
        public int ConditionLocation
        {
            get { return conditionLocation; }
            set { conditionLocation = value; }
        }

        private int jumpLocation;
        public int JumpLocation
        {
            get { return jumpLocation; }
            set { jumpLocation = value; }
        }

        private StructType type;
        public StructType Type
        {
            get { return type; }
            set { type = value; }
        }

        /// <name>Structure</name>
        /// <type>Constructor</type>
        /// <summary>
        /// Creates a new structure object with initialization.
        /// </summary>
        /// <param name="type">Type of the structure</param>
        public Structure(StructType type) : base()
        {
            this.type = type;
        }
    }
}
