using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LittleCompiler
{
    #region Opcode Enumeration
    /// <name>Opcode</name>
    /// <type>Enum</type>
    /// <summary>
    /// This enumeration creates a strongly-typed specification for 
    /// available operator codes.
    /// </summary>
    public enum Opcode
    {
        ALOAD       = 0x19, // load array reference from variable
        ALOAD_0     = 0x2a, // load array reference from variable #0
        ALOAD_1     = 0x2b, // load array reference from variable #1
        ALOAD_2     = 0x2c, // load array reference from variable #2
        ALOAD_3     = 0x2d, // load array reference from variable #3
        ARRAYLENGTH = 0xbe, // get length of array
        ASTORE      = 0x3a, // store array reference into variable
        ASTORE_0    = 0x4b, // store array reference into variable #0
        ASTORE_1    = 0x4c, // store array reference into variable #1
        ASTORE_2    = 0x4d, // store array reference into variable #2
        ASTORE_3    = 0x4e, // store array reference into variable #3
        BIPUSH      = 0x10, // push byte constant
        DUP         = 0x59, // duplicate the top stack value
        DUP_X1      = 0x5a, // duplicate stack top and insert 2 values down
        DUP_X2      = 0x5b, // duplicate stack top, insert 3 values down
        DUP2        = 0x5c, // duplicate top 2 stack values
        DUP2_x1     = 0x5d, // duplicate top 2 stack values, insert 2 values down
        DUP2_x2     = 0x5e, // duplicate top 2 stack values, insert 3 values down
        GETSTATIC   = 0xb2, // get static field from class
        GOTO        = 0xa7, // unconditional branch
        IADD        = 0x60, // add integers
        IALOAD      = 0x2e, // load integer from array
        IAND        = 0x7e, // Boolean AND integers
        IASTORE     = 0x4f, // store into integer array
        ICONST_M1   = 0x02, // push -1 onto stack
        ICONST_0    = 0x03, // push 0 onto stack
        ICONST_1    = 0x04, // push 1 onto stack
        ICONST_2    = 0x05, // push 2 onto stack
        ICONST_3    = 0x06, // push 3 onto stack
        ICONST_4    = 0x07, // push 4 onto stack
        ICONST_5    = 0x08, // push 5 onto stack
        IDIV        = 0x6c, // divide integers
        IF_ICMPEQ   = 0x9f, // branch if equal
        IF_ICMPNE   = 0xa0, // branch if not equal
        IF_ICMPLT   = 0xa1, // branch if less than
        IF_ICMPGE   = 0xa2, // branch if greater than or equal
        IF_ICMPGT   = 0xa3, // branch if greater than
        IF_ICMPLE   = 0xa4, // branch if less than or equal
        IFEQ        = 0x99, // branch if zero
        IFNE        = 0x9a, // branch if not zero
        IFLT        = 0x9b, // branch if less than zero
        IFGE        = 0x9c, // branch if greater than or equal to zero
        IFGT        = 0x9d, // branch if greater than zero
        IFLE        = 0x9e, // branch if less than or equal to zero
        IINC        = 0x84, // increment variable by constant
        ILOAD       = 0x15, // load integer from local variable
        ILOAD_0     = 0x1a, // load integer from variable #0
        ILOAD_1     = 0x1b, // load integer from variable #1
        ILOAD_2     = 0x1c, // load integer from variable #2
        ILOAD_3     = 0x1d, // load integer from variable #3
        IMUL        = 0x68, // multiply integers
        INEG        = 0x74, // negate integers
        INVOKEVIRTUAL = 0xb6, // invoke virtual function (println)
        IOR         = 0x80, // Boolean OR integers
        IREM        = 0x70, // remainder (mod)
        ISHL        = 0x78, // shift left
        ISHR        = 0x7a, // shift right
        ISTORE      = 0x36, // store integer into local variable
        ISTORE_0    = 0x3b, // store integer into variable #0
        ISTORE_1    = 0x3c, // store integer into variable #1
        ISTORE_2    = 0x3d, // store integer into variable #2
        ISTORE_3    = 0x3e, // store integer into variable #3
        ISUB        = 0x64, // subtract integers
        IUSHR       = 0x7c, // logical shift right
        IXOR        = 0x82, // Boolean exclusive OR
        JSR         = 0xa8, // jump to subroutine
        NEWARRAY    = 0xbc, // create new array
        NOP         = 0x00, // no operation
        POP         = 0x57, // pop the stack
        POP2        = 0x58, // pop top two stack values
        RET         = 0xa9, // return from subroutine
        RETURN      = 0xb1, // return from main program
        SIPUSH      = 0x11, // push constant onto stack
        SWAP        = 0x5f  // swap the top two stack values
    }
    #endregion

    /// <name>Emitter</name>
    /// <type>Class</type>
    /// <summary>
    /// This class is responsible for generating the final object code for the
    /// compiler.  It is used by the parser class to create and output the java
    /// bytecode instructions that will be run by the java vurtual machine.
    /// </summary>
    public class Emitter
    {
        private int programCounter;
        public int ProgramCounter
        {
            get { return programCounter; }
        }

        #region Code Array Declaration
        private byte[] code = new byte[0x10000];
        private byte[] initalCode = 
        {
            (byte)'\xca',(byte)'\xfe',(byte)'\xba',(byte)'\xbe',(byte)'\x00',(byte)'\x03',(byte)'\x00',(byte)'\x2d',
            (byte)'\x00',(byte)'\x18',(byte)'\x07',(byte)'\x00',(byte)'\x11',(byte)'\x07',(byte)'\x00',(byte)'\x12',
            (byte)'\x07',(byte)'\x00',(byte)'\x13',(byte)'\x07',(byte)'\x00',(byte)'\x17',(byte)'\x0a',(byte)'\x00',
            (byte)'\x02',(byte)'\x00',(byte)'\x08',(byte)'\x09',(byte)'\x00',(byte)'\x03',(byte)'\x00',(byte)'\x09',
            (byte)'\x0a',(byte)'\x00',(byte)'\x01',(byte)'\x00',(byte)'\x0a',(byte)'\x0c',(byte)'\x00',(byte)'\x0e',
            (byte)'\x00',(byte)'\x0b',(byte)'\x0c',(byte)'\x00',(byte)'\x15',(byte)'\x00',(byte)'\x10',(byte)'\x0c',
            (byte)'\x00',(byte)'\x16',(byte)'\x00',(byte)'\x0c',(byte)'\x01',(byte)'\x00',(byte)'\x03',(byte)'\x28',
            (byte)'\x29',(byte)'\x56',(byte)'\x01',(byte)'\x00',(byte)'\x04',(byte)'\x28',(byte)'\x49',(byte)'\x29',
            (byte)'\x56',(byte)'\x01',(byte)'\x00',(byte)'\x16',(byte)'\x28',(byte)'\x5b',(byte)'\x4c',(byte)'\x6a',
            (byte)'\x61',(byte)'\x76',(byte)'\x61',(byte)'\x2f',(byte)'\x6c',(byte)'\x61',(byte)'\x6e',(byte)'\x67',
            (byte)'\x2f',(byte)'\x53',(byte)'\x74',(byte)'\x72',(byte)'\x69',(byte)'\x6e',(byte)'\x67',(byte)'\x3b',
            (byte)'\x29',(byte)'\x56',(byte)'\x01',(byte)'\x00',(byte)'\x06',(byte)'\x3c',(byte)'\x69',(byte)'\x6e',
            (byte)'\x69',(byte)'\x74',(byte)'\x3e',(byte)'\x01',(byte)'\x00',(byte)'\x04',(byte)'\x43',(byte)'\x6f',
            (byte)'\x64',(byte)'\x65',(byte)'\x01',(byte)'\x00',(byte)'\x15',(byte)'\x4c',(byte)'\x6a',(byte)'\x61',
            (byte)'\x76',(byte)'\x61',(byte)'\x2f',(byte)'\x69',(byte)'\x6f',(byte)'\x2f',(byte)'\x50',(byte)'\x72',
            (byte)'\x69',(byte)'\x6e',(byte)'\x74',(byte)'\x53',(byte)'\x74',(byte)'\x72',(byte)'\x65',(byte)'\x61',
            (byte)'\x6d',(byte)'\x3b',(byte)'\x01',(byte)'\x00',(byte)'\x13',(byte)'\x6a',(byte)'\x61',(byte)'\x76',
            (byte)'\x61',(byte)'\x2f',(byte)'\x69',(byte)'\x6f',(byte)'\x2f',(byte)'\x50',(byte)'\x72',(byte)'\x69',
            (byte)'\x6e',(byte)'\x74',(byte)'\x53',(byte)'\x74',(byte)'\x72',(byte)'\x65',(byte)'\x61',(byte)'\x6d',
            (byte)'\x01',(byte)'\x00',(byte)'\x10',(byte)'\x6a',(byte)'\x61',(byte)'\x76',(byte)'\x61',(byte)'\x2f',
            (byte)'\x6c',(byte)'\x61',(byte)'\x6e',(byte)'\x67',(byte)'\x2f',(byte)'\x4f',(byte)'\x62',(byte)'\x6a',
            (byte)'\x65',(byte)'\x63',(byte)'\x74',(byte)'\x01',(byte)'\x00',(byte)'\x10',(byte)'\x6a',(byte)'\x61',
            (byte)'\x76',(byte)'\x61',(byte)'\x2f',(byte)'\x6c',(byte)'\x61',(byte)'\x6e',(byte)'\x67',(byte)'\x2f',
            (byte)'\x53',(byte)'\x79',(byte)'\x73',(byte)'\x74',(byte)'\x65',(byte)'\x6d',(byte)'\x01',(byte)'\x00',
            (byte)'\x04',(byte)'\x6d',(byte)'\x61',(byte)'\x69',(byte)'\x6e',(byte)'\x01',(byte)'\x00',(byte)'\x03',
            (byte)'\x6f',(byte)'\x75',(byte)'\x74',(byte)'\x01',(byte)'\x00',(byte)'\x07',(byte)'\x70',(byte)'\x72',
            (byte)'\x69',(byte)'\x6e',(byte)'\x74',(byte)'\x6c',(byte)'\x6e',(byte)'\x01',(byte)'\x00',(byte)'\x03',
            (byte)'\x72',(byte)'\x75',(byte)'\x6e',(byte)'\x00',(byte)'\x21',(byte)'\x00',(byte)'\x04',(byte)'\x00',
            (byte)'\x02',(byte)'\x00',(byte)'\x00',(byte)'\x00',(byte)'\x00',(byte)'\x00',(byte)'\x02',(byte)'\x00',
            (byte)'\x01',(byte)'\x00',(byte)'\x0e',(byte)'\x00',(byte)'\x0b',(byte)'\x00',(byte)'\x01',(byte)'\x00',
            (byte)'\x0f',(byte)'\x00',(byte)'\x00',(byte)'\x00',(byte)'\x11',(byte)'\x00',(byte)'\x01',(byte)'\x00',
            (byte)'\x01',(byte)'\x00',(byte)'\x00',(byte)'\x00',(byte)'\x05',(byte)'\x2a',(byte)'\xb7',(byte)'\x00',
            (byte)'\x05',(byte)'\xb1',(byte)'\x00',(byte)'\x00',(byte)'\x00',(byte)'\x00',(byte)'\x00',(byte)'\x09',
            (byte)'\x00',(byte)'\x14',(byte)'\x00',(byte)'\x0d',(byte)'\x00',(byte)'\x01',(byte)'\x00',(byte)'\x0f',
            (byte)'\x00',(byte)'\x00',(byte)'\x00',(byte)'\xff',    // attribute length
            (byte)'\x00',(byte)'\x40',                              // max stack length
            (byte)'\x01',(byte)'\x00',                              // max local variables
            (byte)'\x00',(byte)'\x00',(byte)'\x00',(byte)'\xff'     // code length
        };
        #endregion

        #region Public Methods
        /// <name>Emitter</name>
        /// <type>Constructor</type>
        /// <summary>
        /// Creates an emmiter object and initializes the program counter.
        /// </summary>
        public Emitter() : base()
        {
            // Start of object code
            programCounter = 0x11c;

            // We cannot declare an appropriately sized array and initalize only part
            // of it so we must create the inital code array and copy it over to the
            // full array via the built-in array copy method
            Array.Copy(initalCode, code, initalCode.Length);
        }

        /// <name>ChooseOp</name>
        /// <type>Method</type>
        /// <summary>
        /// Chooses a 1-byte or 2-byte opcode depending on the address.
        /// </summary>
        /// <param name="longOp">2-byte opcode</param>
        /// <param name="shortOp">1-byte opcode</param>
        /// <param name="location">Address of the opcode</param>
        public void ChooseOp(Opcode longOp, Opcode shortOp, int location)
        {
            if (location <= 3)
            {
                Emit((Opcode)((int)shortOp + location));
            }
            else
            {
                Emit(longOp, (byte)location);
            }
        }

        /// <name>Emit</name>
        /// <type>Method</type>
        /// <summary>
        /// Generates one-byte instructions.
        /// </summary>
        /// <param name="op">Opcode for the instruction</param>
        public void Emit(Opcode op)
        {
            code[programCounter++] = (byte)op;

            Compiler.WriteToDebug("Emitter - Emit(op) called (line " + Compiler.LineNumber + ")");
            Compiler.WriteToDebug("   program counter:\t" + (programCounter - 1));
            Compiler.WriteToDebug("   instruction:\t\t" + Enum.GetName(typeof(Opcode), op));
            Compiler.WriteToDebug(String.Empty);
        }

        /// <name>Emit</name>
        /// <type>Method</type>
        /// <summary>
        /// Generates two-byte instructions.
        /// </summary>
        /// <param name="op">Opcode for the instruction</param>
        /// <param name="b1">Second byte of the instruction</param>
        public void Emit(Opcode op, byte b1)
        {
            code[programCounter++] = (byte)op;
            code[programCounter++] = b1;

            Compiler.WriteToDebug("Emitter - Emit(op, b1) called (line " + Compiler.LineNumber + ")");
            Compiler.WriteToDebug("   program counter:\t" + (programCounter - 2));
            Compiler.WriteToDebug("   instruction:\t\t" + Enum.GetName(typeof(Opcode), op) + " " + (int)b1);
            Compiler.WriteToDebug(String.Empty);
        }

        /// <name>Emit</name>
        /// <type>Method</type>
        /// <summary>
        /// Generates three-byte instructions.
        /// </summary>
        /// <param name="op">Opcode for the instruction</param>
        /// <param name="i1">Used to create second and third bytes</param>
        public void Emit(Opcode op, int i1)
        {
            byte b1, b2;
            b1 = (byte)((i1 >> 8) & 0xff);
            b2 = (byte)(i1 & 0xff);
            code[programCounter++] = (byte)op;
            code[programCounter++] = b1;
            code[programCounter++] = b2;

            Compiler.WriteToDebug("Emitter - Emit(op, i1) called (line " + Compiler.LineNumber + ")");
            Compiler.WriteToDebug("   program counter:\t" + (programCounter - 3));
            Compiler.WriteToDebug("   instruction:\t\t" + Enum.GetName(typeof(Opcode), op) + 
                " " + (int)b1 + " " + (int)b2);
            Compiler.WriteToDebug(String.Empty);
        }

        /// <name>Emit</name>
        /// <type>Method</type>
        /// <summary>
        /// Used to fill a forward reference and to save a large literal 
        /// in the data area.
        /// </summary>
        /// <param name="address">Address of the reference</param>
        /// <param name="contents">Contents of the reference</param>
        public void Emit(int address, int contents)
        {
            byte b1, b2;
            b1 = (byte)((contents >> 8) & 0xff);
            b2 = (byte)(contents & 0xff);
            code[address++] = b1;
            code[address] = b2;

            Compiler.WriteToDebug("Emitter - Emit(address, contents) called (line " + Compiler.LineNumber + ")");
            Compiler.WriteToDebug("   address:\t\t" + (address - 1));
            Compiler.WriteToDebug("   contents:\t\t" + (int)b1 + (int)b2);
            Compiler.WriteToDebug(String.Empty);
        }

        /// <name>OutputObjectFile</name>
        /// <type>Method</type>
        /// <summary>
        /// Writes out the object code in binary form to the output file.
        /// </summary>
        public void OutputObjectFile()
        {
            int codeLength = programCounter - 0x11c;
            int attributeLength = codeLength + 12;

            FileStream stream = new FileStream("run.class", FileMode.Create, FileAccess.Write);
            BinaryWriter objectFile = new BinaryWriter(stream);

            if (attributeLength > 0x10000)
            {
                Compiler.ThrowCompilerException("Program is too long to compile");
            }

            code[0x11a] = (byte)((codeLength >> 8) & 0xff);
            code[0x11b] = (byte)(codeLength & 0xff);
            code[0x112] = (byte)((attributeLength >> 8) & 0xff);
            code[0x113] = (byte)(attributeLength & 0xff);

            for (int i = 0; i < 6; i++)
            {
                code[programCounter++] = 0;
            }

            objectFile.Write(code, 0, programCounter);
            objectFile.Close();
            stream.Close();

            Compiler.WriteToDebug("Emitter - OutputObjectFile completed (line " + Compiler.LineNumber + ")");
            Compiler.WriteToDebug(String.Empty);
        }
        #endregion
    }
}
