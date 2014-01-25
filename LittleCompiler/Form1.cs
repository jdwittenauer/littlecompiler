using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LittleCompiler
{
    /// <name>Form1</name>
    /// <type>Class</type>
    /// <summary>
    /// This class contains the code for events triggered by the user interface including
    /// specifying the location of the input file and initiating file compilation.
    /// </summary>
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region Browse Button Code
        /// <name>btnBrowse_Click</name>
        /// <type>Event</type>
        /// <summary>
        /// Show the windows file dialog and displays the selected file in the text box.
        /// </summary>
        /// <param name="sender">Windows event parameter</param>
        /// <param name="e">Windows event parameter</param>
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            dlgOpenFile.ShowDialog();
            txtFileName.Text = dlgOpenFile.FileName;
        }
        #endregion

        #region Compile Button Code
        /// <name>btnCompile_Click</name>
        /// <type>Event</type>
        /// <summary>
        /// Execute code that compiles the file at the location in the text box.
        /// </summary>
        /// <param name="sender">Windows event parameter</param>
        /// <param name="e">Windows event parameter</param>
        private void btnCompile_Click(object sender, EventArgs e)
        {
            // First verify that a valid file was selected
            bool successfulCompletion = true;
            bool debugMode = chkDebug.Checked;
            string fileName = txtFileName.Text;

            if (fileName.Length == 0)
            {
                MessageBox.Show("Please specify a file name", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Now call the parser and attempt to compile the the file, catching any
            // compiler exceptions and displaying them to the user (in addition to
            // logging to a debug file if we're in debug mode)
            try
            {
                Compiler.LineNumber = 1;

                if (debugMode)
                {
                    Compiler.StartDebugging();
                }

                Parser parser = new Parser();
                parser.Compile(fileName);
            }
            catch (IOException ex)
            {
                MessageBox.Show("Error opening file: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (CompilerException ex)
            {
                successfulCompletion = false;
                MessageBox.Show("Compiler exception: " + ex.Message + " (line " + Compiler.LineNumber + ")",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (debugMode)
                {
                    Compiler.EndDebugging();
                }
            }

            // If there were no errors generated then inform the user that the
            // operation was successfully completed
            if (successfulCompletion)
            {
                MessageBox.Show("Compilation successful!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        #endregion
    }
}
