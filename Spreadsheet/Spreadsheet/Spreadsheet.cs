//Author John Stevens 10/03/2021
//PS5 for CS3500 Prof Kopta

using SpreadsheetUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace SS
{
    public class Spreadsheet : AbstractSpreadsheet
    {
        private Hashtable cells;
        private DependencyGraph DG;
        private Func<string, bool> isValid;
        private Func<string, string> normalize;
        private string version;
        private bool changed;
        private Func<string, double> lookupDel;


        /// <summary>
        /// Four parameter constructor for reading a spreadsheet from a file.
        /// </summary>
        /// <param name="filePath">filepath to read the new spreadsheet xml from</param>
        /// <param name="isValid">isValid delegate</param>
        /// <param name="normalize">normalize delegate</param>
        /// <param name="version">spreadsheet version</param>
        public Spreadsheet(string filePath, Func<string, bool> isValid, Func<string, string> normalize, string version) : base(isValid, normalize, version)
        {
            cells = new Hashtable();
            DG = new DependencyGraph();
            lookupDel = lookupMethod;
            changed = false;

            //these just give the delegates some default values so even if a null is passed in, the spreadsheet will still work
            {
                if (isValid is null)
                    this.isValid = s => true;
                else
                    this.isValid = isValid;

                if (normalize is null)
                    this.normalize = s => s;
                else
                    this.normalize = normalize;

                if (version is null)
                    this.version = "default";
                else
                    this.version = version;
            }

            if (filePath is null)
                throw new ArgumentNullException("Invalid file path.");

            GetSavedVersion(filePath);
        }

        /// <summary>
        /// Three parameter constructor for making a spreadsheet with isValid and normalize delegates
        /// </summary>
        /// <param name="isValid">isValid delegate</param>
        /// <param name="normalize">normalize delegate</param>
        /// <param name="version">spreadsheet version</param>
        public Spreadsheet(Func<string, bool> isValid, Func<string, string> normalize, string version) : base(isValid, normalize, version)
        {
            cells = new Hashtable();
            DG = new DependencyGraph();
            lookupDel = lookupMethod;
            changed = false;
            
            //these just give the delegates some default values so even if a null is passed in, the spreadsheet will still work
            {
                if (isValid is null)
                    this.isValid = s => true;
                else
                    this.isValid = isValid;

                if (normalize is null)
                    this.normalize = s => s;
                else
                    this.normalize = normalize;

                if (version is null)
                    this.version = "default";
                else
                    this.version = version;
            } 
        }

        /// <summary>
        /// Zero parameter constructor that creates a spreadsheet with default values for version, isvalid, and normalize
        /// </summary>
        public Spreadsheet() : base(x => true, s => s, "default")
        {
            cells = new Hashtable();
            DG = new DependencyGraph();
            changed = false;
            lookupDel = lookupMethod;

            //these just give the delegates some default values
            {
                if (isValid is null)
                    this.isValid = s => true;

                if (normalize is null)
                    this.normalize = s => s;

                if (version is null)
                    this.version = "default";
            }
        }

        /// <summary>
        /// A getter for the changed variable. Only the methods can set changed
        /// </summary>
        public override bool Changed { get => changed; protected set => throw new NotImplementedException(); }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// value should be either a string, a double, or a Formula.
        /// </summary>
        /// <param name="name">name of a cell</param>
        public override object GetCellContents(string name)
        {
            cellNameIsNullOrInvalid(name);
            name = normalize(name);
            if (cells.ContainsKey(name))
            {
                cell c1 = (cell)cells[name];
                return c1.getContents();
            }
            else
                return "";
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the value (as opposed to the contents) of the named cell.  The return
        /// value should be either a string, a double, or a SpreadsheetUtilities.FormulaError.
        /// </summary>
        /// <param name="name">name of a cell</param>
        public override object GetCellValue(string name)
        {
            cellNameIsNullOrInvalid(name);
            if (cells.ContainsKey(name))
            {
                cell temp = (cell)cells[name];
                return temp.getValue();
            }
            else
                return "";
        }

        /// <summary>
        /// Enumerates the names of all the non-empty cells in the spreadsheet.
        /// </summary>
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            List<string> cellNames = new List<string>();
            ICollection list = cells.Keys;
            foreach (object s in list)
            {
                cellNames.Add((string)s);
            }

            return cellNames;
        }

        /// <summary>
        /// Returns the version information of the spreadsheet saved in the named file.
        /// If there are any problems opening, reading, or closing the file, the method
        /// should throw a SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        /// <param name="filename">file path of the xml file</param>
        public override string GetSavedVersion(string filename)
        {
            String version = null;
            String tempName;
            String tempContents;

            ////checks for invalid file paths
            //try
            //{
            //    XmlReader test = XmlReader.Create(filename);
            //}
            //catch
            //{
            //    throw new SpreadsheetReadWriteException("Invalid file path.");
            //}

            try
            {
                using (XmlReader reader = XmlReader.Create(filename))
                {
                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
                            switch (reader.Name)
                            {
                                case "spreadsheet":
                                    if (reader.HasAttributes == true)
                                        version = reader.GetAttribute(0);
                                    break;

                                case "cell":
                                    reader.Read();
                                    if (reader.IsStartElement() && reader.Name == "name")
                                    {
                                        //gets and checks the name
                                        tempName = reader.ReadElementContentAsString();
                                        if (!xmlIsValid(tempName))
                                            throw new SpreadsheetReadWriteException("Invalid value of name element.");

                                        if (reader.IsStartElement() && reader.Name == "contents")
                                        {
                                            //gets the contents
                                            tempContents = reader.ReadElementContentAsString();
                                            //this try loop will catch any problems in creating a new cell such as circular dependencies and throw spreadSheetReadWriteExceptions as needed
                                            try
                                            {
                                                this.SetContentsOfCell(tempName, tempContents);
                                            }
                                            catch
                                            {
                                                //this throws if there was an issue creating a new cell with the given name/contents
                                                throw new SpreadsheetReadWriteException("There was a problem creating a cell with the contents provided in the file. The data in the file either creates a circular exception or an invalid formula.");
                                            }
                                        }
                                        else //this throws if the element following a name element isn't a contents element
                                            throw new SpreadsheetReadWriteException("Missing a contents element. Check to make sure the file is formatted correctly.");
                                    }
                                    else //this throws if the element following a cell element isn't a name element
                                        throw new SpreadsheetReadWriteException("Missing a name element. Check to make sure the file is formatted correctly.");
                                    break;

                                default:
                                    throw new SpreadsheetReadWriteException("There is an illegal element in this file.");
                            }
                        }
                    }
                }
            }
            catch
            {
                throw new SpreadsheetReadWriteException("Invalid file path.");
            }


            //if version is still null, that means that the file didn't have a version tag
            if (version is null || (this.version != version && this.version != "default"))
                throw new SpreadsheetReadWriteException("Either the file is an incorrect version or its missing a <spreadsheet version> tag.");
            else
                this.version = version;
            //if (version != this.version)
            //    throw new SpreadsheetReadWriteException("The version of the file doesn't match the version of the current spreadsheet");

            //just in case cells load in the wrong order, recalculate them all at the end
            IEnumerable<string> list = GetNamesOfAllNonemptyCells();
            List<string> list2 = new List<string>();
            foreach (string s in list)
            {
                list2.Add(s);
            }
            updateCells(list2);

            return version;
        }

        /// <summary>
        /// Writes the contents of this spreadsheet to the named file using an XML format.
        /// The XML elements should be structured as follows:
        /// 
        /// <spreadsheet version="version information goes here">
        /// 
        /// <cell>
        /// <name>cell name goes here</name>
        /// <contents>cell contents goes here</contents>    
        /// </cell>
        /// 
        /// </spreadsheet>
        /// 
        /// There should be one cell element for each non-empty cell in the spreadsheet.  
        /// If the cell contains a string, it should be written as the contents.  
        /// If the cell contains a double d, d.ToString() should be written as the contents.  
        /// If the cell contains a Formula f, f.ToString() with "=" prepended should be written as the contents.
        /// 
        /// If there are any problems opening, writing, or closing the file, the method should throw a
        /// SpreadsheetReadWriteException with an explanatory message.
        /// </summary>
        /// <param name="filename">file path for the xml file</param>
        public override void Save(string filename)
        {
            changed = false;
            IEnumerable<string> cellList = GetNamesOfAllNonemptyCells();

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "  ";
            try
            {
                using (XmlWriter writer = XmlWriter.Create(filename, settings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("spreadsheet");
                    writer.WriteAttributeString("version", version);

                    foreach (string s in cellList)
                    {
                        writer.WriteStartElement("cell");
                        writer.WriteElementString("name", s);
                        if(GetCellContents(s) is Formula)
                            writer.WriteElementString("contents", "=" + GetCellContents(s).ToString());
                        else
                            writer.WriteElementString("contents", GetCellContents(s).ToString());
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            }
            catch
            {
                throw new SpreadsheetReadWriteException("Invalid file path.");
            }
                
        }

        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes number.  The method returns a
        /// list consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        /// <param name="name">string name of the cell</param>
        /// <param name="number">double value of the cell</param>
        protected override IList<string> SetCellContents(string name, double number)
        {
            //create new/update cell
            if (!cells.Contains(name))
                cells.Add(name, new cell(name, number));
            else
                cells[name] = new cell(name, number);

            //just in case this cell previously contained a formula, get rid of any old dependencies in the DG
            DG.ReplaceDependees(name, new List<String>());

            //get a list of all direct and indirect dependents
            List<string> list = new List<string>();
            foreach (string s in GetCellsToRecalculate(name))
                list.Add(s);

            //if its the only cell updating, its already updated
            if (list.Count > 1)
                updateCells(list);

            return list;
        }

        /// <summary>
        /// If text is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes text.  The method returns a
        /// list consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        /// <param name="name">string name of the cell</param>
        /// <param name="text">text contents of the cell</param>
        protected override IList<string> SetCellContents(string name, string text)
        {
            if (!text.Equals(""))
            {
                //create new/update cell
                if (!cells.Contains(name))
                    cells.Add(name, new cell(name, text));
                else
                    cells[name] = new cell(name, text);
            }

            //just in case this cell previously contained a formula, get rid of any old dependencies in the DG
            DG.ReplaceDependees(name, new List<String>());

            //get a list of all direct and indirect dependents
            List<string> list = new List<string>();
            foreach (string s in GetCellsToRecalculate(name))
                list.Add(s);

            //if its the only cell updating, its already updated
            if(list.Count > 1)
                updateCells(list); //this is necessary so it can change the formulas of all dependent cells to formulaErrors

            return list;
        }

        /// <summary>
        /// If the formula parameter is null, throws an ArgumentNullException.
        /// 
        /// Otherwise, if name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, if changing the contents of the named cell to be the formula would cause a 
        /// circular dependency, throws a CircularException, and no change is made to the spreadsheet.
        /// 
        /// Otherwise, the contents of the named cell becomes formula.  The method returns a
        /// list consisting of name plus the names of all other cells whose value depends,
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        /// <param name="name">string name of the cell</param>
        /// <param name="formula">formula contents of the cell</param>
        protected override IList<string> SetCellContents(string name, Formula formula)
        {
            cell tempCell = new cell(name, formula);

            //sets the value of the new cell
            object value = formula.Evaluate(lookupDel);
            tempCell.setValue(value);

            //adds or updates the cell
            if (!cells.Contains(name))
                cells.Add(name, tempCell);            
            else
                cells[name] = tempCell;
            
            //update new dependees
            List<string> listOfNewDependees = (List<string>)formula.GetVariables();
            DG.ReplaceDependees(name, listOfNewDependees);

            //get a list of all direct and indirect dependents. This will also check for circular dependencies
            List<string> list = new List<string>();
            foreach (string s in GetCellsToRecalculate(name))
                list.Add(s);

            //if its the only cell updating, its already updated
            if (list.Count > 1)
                updateCells(list);

            return list;
        }
       
        /// <summary>
        /// A public method that intakes any two strings and determines which protected setCellContents method to use
        /// </summary>
        /// <param name="name">cell name</param>
        /// <param name="content">contents of the cell</param>
        /// <returns></returns>
        public override IList<string> SetContentsOfCell(string name, string content)
        {
            cellNameIsNullOrInvalid(name);

            if (content is null)
                throw new ArgumentNullException();

            //it normalizes afterwards to avoid the possibility of throwing a null exception
            name = normalize(name);
            changed = true;
            
            double doubleValue;
            if (double.TryParse(content, out doubleValue))
                return SetCellContents(name, doubleValue);
            else
                if (content.StartsWith("=")) //if it starts with '=' it must be a formula cell
                    return SetCellContents(name, new Formula(normalize(content.Substring(1)), normalize, isValid));
                else //otherwise its a string cell
                    return SetCellContents(name, content);
        }

        /// <summary>
        /// Returns an enumeration, without duplicates, of the names of all cells whose
        /// values depend directly on the value of the named cell.  In other words, returns
        /// an enumeration, without duplicates, of the names of all cells that contain
        /// formulas containing name.
        /// 
        /// For example, suppose that
        /// A1 contains 3
        /// B1 contains the formula A1 * A1
        /// C1 contains the formula B1 + A1
        /// D1 contains the formula B1 - C1
        /// The direct dependents of A1 are B1 and C1
        /// </summary>
        /// <param name="name">name of the cell</param>
        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            List<string> dependents = new List<string>();
            if (DG.HasDependents(name))
            {
                foreach (String s in DG.GetDependents(name))
                    dependents.Add(s);
                return dependents;
            }
            else
                return dependents;
        }



        //-------------PRIVATE HELPER METHODS-------------------

        /// <summary>
        /// Similar to the isNullOrInvalid methods but this one returns a boolean value instead of throwing an error.
        /// This is really important because it needs to be able to detect if something is valid without throwing an InvalidNameExecption,
        /// so that a SpreadSheetReadWrite exception can be thrown instead.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>bool if name is valid</returns>
        private bool xmlIsValid(String name)
        {
            //this variable is used to make sure that once the name switches to number chars,
            //it can't switch back to letter chars, since that would be an invalid variable name under ps5 conditions
            bool charSwitch = false;

            if (name == null || name.Length < 2)
                return false;

            Char[] chars = name.ToCharArray();

            //this if statement needs to be separate from the one above or else it might throw a null reference exception
            if (!char.IsLetter(chars[0]))
                return false;

            foreach (char c in chars)
            {
                if (char.IsDigit(c) || char.IsLetter(c))
                {
                    if (char.IsDigit(c))
                        charSwitch = true;
                    if (char.IsLetter(c) && charSwitch == true)
                        return false;
                }
                else
                    return false;
            }

            //now that its passed the basic qualifications for being a variable, it will check it against the passed in isValid delegate
            if (!isValid(name))
                return false;

            return true;
        }

        /// <summary>
        /// Checks whether the passedd in string object is null, a valid cell name, or an cell variable name
        /// </summary>
        /// <param name="name">String representing a cell name</param>
        private void cellNameIsNullOrInvalid(String name)
        {
            //this variable is used to make sure that once the name switches to number chars,
            //it can't switch back to letter chars, since that would be an invalid variable name under ps5 conditions
            bool charSwitch = false;

            if (name == null || name.Length < 2)
                throw new InvalidNameException();

            Char[] chars = name.ToCharArray();

            //this if statement needs to be separate from the one above or else it might throw a null reference exception
            if (!char.IsLetter(chars[0]))
                throw new InvalidNameException();
            
            foreach (char c in chars)
            {
                if (char.IsDigit(c) || char.IsLetter(c))
                {
                    if (char.IsDigit(c))
                        charSwitch = true;
                    if (char.IsLetter(c) && charSwitch == true)
                        throw new InvalidNameException();
                }
                else
                    throw new InvalidNameException();
            }

            //now that its passed the basic qualifications for being a variable, it will check it against the passed in isValid delegate
            if (!isValid(name))
                throw new InvalidNameException();

        }

        /// <summary>
        /// Looks up a cell's values
        /// </summary>
        /// <param name="name">name of the cell who's value is being looked up</param>
        /// <returns></returns>
        private double lookupMethod(String name)
        {
            cellNameIsNullOrInvalid(name);
            if (cells.ContainsKey(normalize(name)))
            {
                cell temp = (cell)cells[name];
                object tempObj = temp.getValue();
                if (tempObj is double)
                    return (double)tempObj;
                else
                    throw new InvalidNameException();
            }
            else
                throw new InvalidNameException();
        }

        /// <summary>
        /// This method updates the values of cells starting with the ones in the first layer of dependency, then updates the cells
        /// that depend on those cells, then the cells that depend on those cells, etc.
        /// </summary>
        /// <param name="listOfCellsToUpdate">an in order list of the cells that need to be updated</param>
        private void updateCells(IList<string> listOfCellsToUpdate)
        {
            foreach(string s in listOfCellsToUpdate)
            {
                cell tempCell = (cell)cells[s];

                //if it contains a formula, get the formula and update the value
                if(tempCell.getContents() is Formula)
                {
                    Formula f = (Formula)tempCell.getContents();
                    tempCell.setValue(f.Evaluate(lookupDel));
                    cells[s] = tempCell;
                }
                //The above if statement is necessary since the listOfCellsToUpdate will include the cell that was changed.
                //This cell could contain a double value instead of a formula, thus necessitating the if statement
            }
        }


        //-------------------------Cell Class-------------------------

        /// <summary>
        /// A cell class that repsents one cell on a spreadsheet
        /// </summary>
        class cell
        {
            private String name;
            private object contents; //either a string, a double, or a formula
            private object value; //either a string, a double, or a formula error

            /// <summary>
            /// Creates a new cell
            /// </summary>
            /// <param name="n">name of cell</param>
            /// <param name="o">contents of cell</param>
            public cell(String name, object obj)
            {
                this.name = name;
                contents = obj;
                if (obj is String || obj is double)
                    value = obj;
            }

            /// <summary>
            /// Returns the contents of the cell
            /// </summary>
            /// <returns></returns>
            public object getContents()
            {
                return contents;
            }

            /// <summary>
            /// Returns the value of the cell
            /// </summary>
            /// <returns></returns>
            public object getValue()
            {
                return value;
            }

            /// <summary>
            /// Sets the value of the cell
            /// </summary>
            /// <param name="value"></param>
            public void setValue(object value)
            {
                this.value = value;
            }


        }
    }
}