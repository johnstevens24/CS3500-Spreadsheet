//Author John Stevens

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using SS;
using SpreadsheetUtilities;
using System.IO;

namespace SpreadsheetGUI
{
    public partial class SpreadsheetForm : Form
    {
        private AbstractSpreadsheet theSpreadsheet;
        private bool arrowKeysFunctional;
        private int row;
        private int col;
        private bool showDependentsBool;
        private string mostRecentSavedFilePath;

        public SpreadsheetForm()
        {
            InitializeComponent();
            //just setting some initial values
            NameTextBox.Text = "A1";
            arrowKeysFunctional = true;
            showDependentsBool = false;
            theSpreadsheet = new Spreadsheet(s => Regex.IsMatch(s, "^[A-Z][1-9][0-9]?$"), s => s.ToUpper(), "ps6");
            
            //pairing event handlers
            spreadsheetPanel1.SelectionChanged += spreadsheetPanel1_SelectionChanged;
            ContentsTextBox.KeyDown += new KeyEventHandler(ContentsTextBox_KeyDown);
            toolStripNew.Click += ToolStripNew_Click;
            toolStripOpen.Click += ToolStripOpen_Click;
            toolStripSave.Click += ToolStripSave_Click;
            toolStripClose.Click += ToolStripClose_Click;
            FormClosing += SpreadsheetForm_FormClosing;
            toolStripHelp.Click += ToolStripHelp_Click;
        }

      


        /// <summary>
        /// The method that handles the "help" button on the tool strip
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolStripHelp_Click(object sender, EventArgs e)
        {
            //Shows the instructions included in the readme file
            MessageBox.Show(
                "\t\tGeneral use instructions:\n\n"
               +"1. When you first open a spreadsheet, a cell will be highlighted but not really \"selected\" "
               +"so you will have to click on one to begin editing /using the arrow keys to navigate.\n\n"
               +"2. Once you start typing in the cell contents text box, the arrow keys will be disabled for traversing "
               +"the spreadsheet and instead revert to normal arrow key functionality for editing text."
               +"When you hit enter, the arrow keys will again be used for traversing the spreadsheet.\n\n"
               +"3. If you click on the Show Dependents button and then click on a cell, it will highlight any dependents"
               +"of that cell in red.To get rid of the highlights, simply click on another cell.\n\n"
               +"4. Other than these few things it functions like a regular spreadsheet.\n\n"
               +"5. Also, buttons for creating new spreadsheets, closing, saving, and opening saved spreadsheets are in " 
               +"the top left hand corner under the File tab."
                );
        }

        /// <summary>
        /// This is the closing method where it asks you if you want to save before you close
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SpreadsheetForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if the current spreadsheet hasn't been saved, offer to save before closing
            if (theSpreadsheet.Changed == true && MessageBox.Show("Your speadsheet hasn't been saved. Would you like to do that before closing?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                saveFile();
            //now it will close
        }

        /// <summary>
        /// The method that handles the "close" button on the tool strip
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolStripClose_Click(object sender, EventArgs e)
        {
            //just jumps to SpreadsheetForm_FormClosing where theres an option to save before closing
            Close();
        }

        /// <summary>
        /// The method that handles the "save" button on the tool strip
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolStripSave_Click(object sender, EventArgs e)
        {
            saveFile();
        }

        /// <summary>
        /// The method that handles the "open" button on the tool strip
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolStripOpen_Click(object sender, EventArgs e)
        {
            //if the current spreadsheet hasn't been saved, offer to save it first before opening a new one
            if(theSpreadsheet.Changed == true && MessageBox.Show("Would you like to go back and save before you open a new spreadsheet?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                saveFile();
            
            //begin the opening process
            String filePath = "";
            //open file browser
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "All files (*.*)|*.*|Spreadsheet files (*.sprd)|*.sprd";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;
                }
            }

            //if they exit the file browser, filepath will be null so this is just a check against that
            if (filePath != "")
            {
                try
                {
                    //reset the spreadsheet and make a new one based off the contents of the xml file
                    theSpreadsheet = new Spreadsheet(s => Regex.IsMatch(s, "^[A-Z][1-9][0-9]?$"), s => s.ToUpper(), "ps6");
                    theSpreadsheet.GetSavedVersion(filePath);

                    //clear all the old cells
                    spreadsheetPanel1.Clear();

                    //draw all the cells
                    IEnumerable<string> list = theSpreadsheet.GetNamesOfAllNonemptyCells();
                    foreach (string s in list)
                    {
                        CellNameToCoords(s, out col, out row);
                        object value = theSpreadsheet.GetCellValue(s);
                        spreadsheetPanel1.SetValue(col, row, value.ToString());
                    }

                    mostRecentSavedFilePath = filePath;
                }
                catch (Exception exception)
                {
                    MessageBox.Show("There was an issue loading the file you selected.", "", MessageBoxButtons.OK);
                }
            }

            //if the spreadsheet you open has contents for the cell currently selected, it will update to show them
            updateTextBoxes(row, col);
        }

        /// <summary>
        /// The method that handles the "new" button on the tool strip
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolStripNew_Click(object sender, EventArgs e)
        {
            SpreadsheetApplicationContext.getAppContext().RunForm(new SpreadsheetForm());
        }

        /// <summary>
        /// This updates the highlighted cell whenever a new one is clicked
        /// </summary>
        /// <param name="sender"></param>
        private void spreadsheetPanel1_SelectionChanged(SpreadsheetPanel sender)
        {
            sender.GetSelection(out this.col, out this.row);
            //updates the text boxes to the current cell
            updateTextBoxes(row, col);
            //keeps the arrow keys functional until you start modifying the contents of the cell
            arrowKeysFunctional = true;
            //stops the panel from highlighting dependent cells every time you click on a new cell
            spreadsheetPanel1.setDrawDependents(false);

            if (showDependentsBool)
            {
                spreadsheetPanel1.setDrawDependents(true);
                showDependents();
                showDependentsBool = false;
            }

            //Makes sure the contents text box is in focus
            ContentsTextBox.Focus();
        }

        /// <summary>
        /// Attempts to save the spreadsheet to the given file path
        /// </summary>
        /// <param name="filePath"></param>
        private void trySave(string filePath)
        {
            try
            {
                theSpreadsheet.Save(filePath);
                mostRecentSavedFilePath = filePath;
                MessageBox.Show("Your spreadsheet has been saved.", "", MessageBoxButtons.OK);
            }
            catch (Exception e)
            {
                MessageBox.Show("There was an error saving your spreadsheet: " + e.Message);
            }
        }

        /// <summary>
        /// This method opens a dialogue and allows the user to choose where to save the file then saves it.
        /// </summary>
        private void saveFile()
        {
            string filePath = "";
              
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.InitialDirectory = "c:\\";
                saveFileDialog.Filter = "All files (*.*)|*.*|Spreadsheet files (*.sprd)|*.sprd";
                saveFileDialog.FilterIndex = 2;
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.DefaultExt = ".sprd";
                saveFileDialog.OverwritePrompt = false;
                
                //this automatically sets the filepath to the most recent one so you can just hit save
                if (mostRecentSavedFilePath != null && mostRecentSavedFilePath != "")
                    saveFileDialog.FileName = mostRecentSavedFilePath;

                //Get the path of specified file
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    filePath = saveFileDialog.FileName;                        

            }

            //this is a check against when the dialogue box closes early and submits an illegal filepath
            if(filePath != "")
            {
                //if the file already exists, its either overwriting a file or writing to itself
                if(File.Exists(filePath))
                {
                    if(mostRecentSavedFilePath == filePath)
                    {
                        trySave(filePath);
                    }
                    else
                    {
                        if (MessageBox.Show("You're going to overwrite a different spreadsheet. Would you like to continue?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            trySave(filePath);
                    }
                }
                else 
                    trySave(filePath);
            }

        }
       
        /// <summary>
        /// Intakes coordinates and turns it into the matching cell name
        /// </summary>
        /// <param name="col">column coordinate</param>
        /// <param name="row">row coordinate</param>
        /// <returns></returns>
        private string CellCoordsToName(int col, int row)
        {
            return $"{(char)('A' + col)}{row+1}"; //this evaluates these functions then make them strings and returns it
        }

        /// <summary>
        /// Intakes a cell name and outputs that cell's coordinates
        /// </summary>
        /// <param name="name">name of the cell</param>
        /// <param name="col">the output column coordinate</param>
        /// <param name="row">the output row coordinate</param>
        private void CellNameToCoords(string name, out int col, out int row)
        {
            char letter = name[0];
            col = letter - 'A';
            row = Int32.Parse(name.Substring(1)) - 1;
        }

        /// <summary>
        /// Loads the spreadsheet panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void spreadsheetPanel1_Load(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Handles when the contents text box changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContentsTextBox_TextChanged(object sender, EventArgs e)
        {
            //get cell name
            String name = CellCoordsToName(col, row);

            //get cell contents
            object contents = theSpreadsheet.GetCellContents(name);

            //this block below basically just checks to see if the contents of the contents box matches the contents of the cell.
            //if they do, leave the arrow keys functional so they can keep traversing through the array. If they don't match, disable
            //their traversal functionality so they can be used to move through the text in the contents text box.
            if (contents is Formula)
            {
                if (!("=" + contents.ToString()).Equals(ContentsTextBox.Text)) //if this if statement is combined with the one above, it will cause problems. Not sure why. Spent like 15 min trying to make it work
                    arrowKeysFunctional = false;
            }
            else
                if (!contents.ToString().Equals(ContentsTextBox.Text))
                    arrowKeysFunctional = false;
        }

        /// <summary>
        /// Checks to see if the enter key has been pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContentsTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            //if the key pressed is enter, update the cell that is selected            
            if(e.KeyCode == Keys.Enter)
            {
                String contents = ContentsTextBox.Text;
                String cellName = NameTextBox.Text;
                IList<string> cellsToUpdate;
               
                
                    try
                    {
                        if (contents.Equals(""))
                        {
                            //it wont accept creating a cell with nothing as the contents, thus necessitating giving it a single space for the contents.
                            //The user will never notice this. *abstraction* 
                            cellsToUpdate = theSpreadsheet.SetContentsOfCell(cellName, " ");
                        }
                        else
                            cellsToUpdate = theSpreadsheet.SetContentsOfCell(cellName, contents);

                        //this covers a very niche case where something that seems like a formula is entered such as "=a_2"
                        if(theSpreadsheet.GetCellContents(cellName) is Formula && theSpreadsheet.GetCellContents(cellName).ToString() == "")
                        {
                            //resets it to a blank cells and then throws an exception so that the message box will pop up
                            cellsToUpdate = theSpreadsheet.SetContentsOfCell(cellName, " ");
                            throw new InvalidDataException();
                        }

                        //update dependent cells
                        foreach (String s in cellsToUpdate)
                        {
                            int row;
                            int col;
                            CellNameToCoords(s, out col, out row);


                            String value;
                            object c = theSpreadsheet.GetCellValue(s);
                            if (c is Formula)
                                value = "=" + c.ToString();
                            else
                                if (c is FormulaError)
                                value = "FormulaError";
                            else
                                value = c.ToString();

                            spreadsheetPanel1.SetValue(col, row, value);
                        }

                        //update value textbox
                        Object valueObj = theSpreadsheet.GetCellValue(cellName);
                        if (valueObj is FormulaError)
                        {
                            ValueTextBox.Text = "FormulaError";
                        }
                        else
                            ValueTextBox.Text = valueObj.ToString();
                    }
                    catch (Exception exception)
                    {
                        if (exception is CircularException)
                            MessageBox.Show("No circular exceptions. Input something else.", "", MessageBoxButtons.OK);
                        else
                            MessageBox.Show("Your input contains an illegal argument. Try again.", "", MessageBoxButtons.OK);
                    }
    
                e.Handled = true;
                //this stops that annoying sound from playing when you hit enter
                e.SuppressKeyPress = true;
                //after hitting enter, it automatically makes the arrow keys functional again so the user can go back to traversing the spreadsheet
                arrowKeysFunctional = true;
            }
            else 
                if(arrowKeysFunctional == true) //the arrow keys are used to traverse the spreadsheet (pretty obvious)
                {
                    if (e.KeyCode == Keys.Down && row < 98)
                        row++;

                    if (e.KeyCode == Keys.Up && row > 0)
                        row--;
                
                    if (e.KeyCode == Keys.Left && col > 0)
                        col--;

                    if (e.KeyCode == Keys.Right && col < 25)
                        col++;
 
                    //updates the text boxes to the new cell's data
                    updateTextBoxes(row, col);

                    //changes the selection to the cell you just navigated to with the arrow keys
                    spreadsheetPanel1.SetSelection(col, row);
                }
                else
                    arrowKeysFunctional = false;
        }

        /// <summary>
        /// updates text boxes with current data
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        private void updateTextBoxes(int row, int col)
        {
            String name = CellCoordsToName(col, row);

            //update name textbox
            NameTextBox.Text = name;

            //update value textbox
            object valueObj = theSpreadsheet.GetCellValue(name);
            if (valueObj is FormulaError)
            {
                ValueTextBox.Text = "FormulaError";
            }
            else
                ValueTextBox.Text = valueObj.ToString();

            //update the contents box
            object contents = theSpreadsheet.GetCellContents(name);
            if (contents is Formula)
            {
                ContentsTextBox.Text = "=" + contents.ToString();
            }
            else
                ContentsTextBox.Text = contents.ToString();

        }

       
        
        //the methods below are used for the show dependents feature

        /// <summary>
        /// creates a list of dependent cells coordinates and sends it off to the spreadsheet panel file where they will be highlighted
        /// </summary>
        private void showDependents()
        {
            //get name of current cell
            String name = CellCoordsToName(col, row);

            //changes the contents of the cell to exactly what they were before, but it gets a list of dependent cells
            object contents = theSpreadsheet.GetCellContents(name);
            IList<string> list = null;
            if (contents is Formula)
                list = theSpreadsheet.SetContentsOfCell(name, "=" + contents.ToString());
            if (contents is double)
                list = theSpreadsheet.SetContentsOfCell(name, contents.ToString());

            //if the cell contained a string, this would be null
            if(list != null)
            {
                list.Remove(name); //don't want to highlight the cell as its own dependent
                int[] x = new int[list.Count];
                int[] y = new int[list.Count];
                int count = 0;

                //get the coordinates of dependent's cells
                foreach (String s in list)
                {
                    CellNameToCoords(s, out x[count], out y[count]);
                    count++;
                }

                //input the coordinates of the dependent cells
                spreadsheetPanel1.setDependentsCoords(x, y);
            }
            //if the cell's contents are a string, it doesn't have dependents so do nothing

        }

        /// <summary>
        /// Sets the showDependentsBool to true
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowDependentsButton_Click(object sender, EventArgs e)
        {
                showDependentsBool = true;
        }

    }
}
