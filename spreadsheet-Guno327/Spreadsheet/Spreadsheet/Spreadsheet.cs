using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpreadsheetUtilities;
using static System.Net.Mime.MediaTypeNames;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SS
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Spreadsheet : AbstractSpreadsheet
    {
        //Fields
        [JsonProperty]
        private Dictionary<string, Cell> cells = new();
        private DependencyGraph dps = new();

        /// <summary>
        /// No parameter constructor for a spreadsheet
        /// Makes the validity delegate always true
        /// Makes the normalize delegate do nothing
        /// Sets the verion to "default"
        /// </summary>
        public Spreadsheet() :
            base(s => true, s => s, "default")
        { Changed = false; }

        /// <summary>
        /// 3 parameter constructor for a spreadsheet
        /// </summary>
        /// <param name="isValid">Validity delegate</param>
        /// <param name="normalize">Name normalizing delegate</param>
        /// <param name="version">What version this spreadsheet is made with</param>
        public Spreadsheet(Func<string, bool> isValid, Func<string, string> normalize, string version) : 
            base(isValid, normalize, version) 
        { Changed = false; }

        /// <summary>
        /// 4 parameter constructor for loading a saved spreadsheet.
        /// </summary>
        /// <param name="filepath">The file path to the spreadsheet we are loading</param>
        /// <param name="isValid">Validity delegate</param>
        /// <param name="normalize">Name normalizing delegate</param>
        /// <param name="version">What version this spreadsheet is made with</param>
        /// <exception cref="NotImplementedException"></exception>
        public Spreadsheet(string filepath, Func<string, bool> isValid, Func<string, string> normalize, string version) :
            base(isValid, normalize, version)
        {
            try
            {
                //Read Spreadsheet
                string json = File.ReadAllText(filepath);
                Spreadsheet? loaded = JsonConvert.DeserializeObject<Spreadsheet>(json);
                //Needed to remove warnings, however it will never be reached.
                if (loaded == null)
                    throw new FormulaFormatException("You cant possibley have achived this error.");
                //Ensure validity of version
                if (loaded.Version != this.Version)
                    throw new SpreadsheetReadWriteException("Versions do not match.");

                //Reconstruct spreadsheet
                try
                {
                    //Add each cell back as to allow the dependency graph to be rebuilt naturally.
                    foreach (KeyValuePair<string, Cell> kp in loaded.cells.ToList())
                    {
                        this.SetContentsOfCell(kp.Key, kp.Value.stringForm);
                    }
                }
                //Convert all possible errors to descriptive Read/Write errors
                catch (InvalidNameException) 
                    { throw new SpreadsheetReadWriteException("Invalid cell name in file."); }
                catch (SpreadsheetUtilities.FormulaFormatException) 
                    { throw new SpreadsheetReadWriteException("Invalid formula stored in file."); }
                catch (CircularException)
                    { throw new SpreadsheetReadWriteException("Circular dependency in stored spreadsheet."); }
            }
            //Pass along the internal error
            catch (SpreadsheetReadWriteException e)
            {
                throw new SpreadsheetReadWriteException(e.ToString());
            }
            //Catch all other errors.
            catch
            {
                throw new SpreadsheetReadWriteException("Error reading file.");
            }

            Changed = false;
        }

        //View AbstractSpreadsheet for docs.
        public override bool Changed { get; protected set; }

        //View AbstractSpreadsheet for docs.
        public override object GetCellContents(string name)
        {
            //First check if the name is valid
            if (!ValidName(Normalize(name)))
                throw new InvalidNameException();
            //Then try and get the value for that cell
            if (cells.TryGetValue(Normalize(name), out Cell? c))
                return c.Contents;
            //If the cell doesnt exsist, its empty
            return "";
        }

        //View AbstractSpreadsheet for docs.
        public override object GetCellValue(string name)
        {
            //First check name
            if (!ValidName(Normalize(name)) || !IsValid(Normalize(name)))
                throw new InvalidNameException();
            //If the cell has been set, return its stored value
            if (cells.TryGetValue(Normalize(name), out Cell? cell))
                return cell.Value;
            //Else return the empty string
            else
                return "";
        }

        //View AbstractSpreadsheet for docs.
        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            foreach(string s in cells.Keys)
            {
                if (cells.TryGetValue(s, out Cell? c))
                {
                    if (c.Contents.GetType() == typeof(string))
                    {
                        if ((string)c.Contents != "")
                            yield return s;
                    }
                    else
                        yield return s;
                      
                }
            }
        }

        //View AbstractSpreadsheet for docs.
        public override void Save(string filename)
        {
            try
            {
                string json = JsonConvert.SerializeObject(this);
                File.WriteAllText(filename, json);
                Changed = false;
            }
            catch
            {
                throw new SpreadsheetReadWriteException("Error writing file.");
            }
        }

        //View AbstractSpreadsheet for docs.
        public override IList<string> SetContentsOfCell(string name, string content)
        {
            //First check if the name is valid
            if (!ValidName(Normalize(name)) || !IsValid(Normalize(name)))
                throw new InvalidNameException();
            //Make list of cells to update
            IList<string> toUpdate;

            //If double
            if (double.TryParse(content, out double d))
               toUpdate = SetCellContents(Normalize(name), d);
            //If formula
            else if (content.Length > 0 && content[0] == '=')
               toUpdate = SetCellContents(Normalize(name), new Formula(content.Substring(1), Normalize, IsValid));
            //If string
            else
                toUpdate = SetCellContents(Normalize(name), content);

            //Update all cells values before returning
            foreach (string s in toUpdate)
                if (cells.TryGetValue(s, out Cell? c) && c.Contents.GetType() == typeof(Formula))
                    UpdateCell(c);

            return toUpdate;
        }

        /// <summary>
        /// Private helper method to update a cells value.
        /// Only works on cells that are storing formulas.
        /// </summary>
        /// <param name="c">The cell whos value is to be updated (if possible)</param>
        private void UpdateCell(Cell c)
        {
            if (c.Contents.GetType() != typeof(Formula))
                return;
            c.Value = ((Formula)c.Contents).Evaluate(Lookup);
        }

        //View AbstractSpreadsheet for docs.
        protected override IList<string> SetCellContents(string name, double number)
        {
            
            //If the cell already exsists, update its contents
            if (cells.TryGetValue(Normalize(name), out Cell? c))
            {
                //If replacing a formula, we must remove old dependencies.
                if (c.Contents.GetType() == typeof(Formula))
                {
                    Formula old = (Formula)c.Contents;
                    foreach (string s in old.GetVariables())
                        dps.RemoveDependency(s, Normalize(name));
                }
                c.Contents = number;
                c.Value = number;
            }
            //Otherwise make a new cell
            else
                cells.Add(Normalize(name), new Cell(number, Normalize(name), number));

            //Make and return the dependency list
            List<string> l = GetCellsToRecalculate(Normalize(name)).ToList();

            //Make sure changed is update
            Changed = true;

            return l;
        }

        //View AbstractSpreadsheet for docs.
        protected override IList<string> SetCellContents(string name, string text)
        {
            //If the cell already exsists, update its contents
            if (cells.TryGetValue(Normalize(name), out Cell? c))
            {
                //If replacing a formula, we must remove old dependencies.
                if (c.Contents.GetType() == typeof(Formula))
                {
                    Formula old = (Formula)c.Contents;
                    foreach (string s in old.GetVariables())
                        dps.RemoveDependency(s, Normalize(name));
                }
                c.Contents = text;
                c.Value = text;
            }
                
            //Otherwise make a new cell
            else
                cells.Add(name, new Cell(text, name, text));

            //Make and return the dependency list
            List<string> l = GetCellsToRecalculate(Normalize(name)).ToList();

            //Make sure changed is update
            Changed = true;

            return l;
        }

        //View AbstractSpreadsheet for docs.
        protected override IList<string> SetCellContents(string name, Formula formula)
        {
            Object oldContents;
            Object oldValue;
            //If the cell already exsists, update its contents
            if (cells.TryGetValue(Normalize(name), out Cell? o))
            {
                //If replacing a formula, we must remove old dependencies.
                if (o.Contents.GetType() == typeof(Formula))
                {
                    Formula old = (Formula)o.Contents;
                    oldContents = old;
                    oldValue = o.Value;
                    foreach (string s in old.GetVariables())
                        dps.RemoveDependency(s, Normalize(name));
                }
                else
                {
                    oldContents = o.Contents;
                    oldValue = o.Value;
                }
                o.Contents = formula;
                o.Value = formula.Evaluate(Lookup);
            }
            //Otherwise make a new cell
            else
            {
                oldContents = "";
                oldValue = "";
                cells.Add(Normalize(name), new Cell(formula, Normalize(name), formula.Evaluate(Lookup)));
            }

            //Update dependency graph since formulas are involved.
            foreach (string s in formula.GetVariables())
                dps.AddDependency(s, Normalize(name));

            //Make and return the dependency list
            try
            {
                List<string> l = GetCellsToRecalculate(Normalize(name)).ToList();
                //Make sure changed is updated, only if no circular exception
                Changed = true;
                return l;
            }
            //If this throws circular exception we must undo what we just did
            catch (CircularException)
            {
                //if was empty cell, get rid of it
                if (oldContents.GetType() == typeof(string) && (string)oldContents == "")
                    cells.Remove(Normalize(name));
                //If it wasnt empty, return the value to what it used to be
                else
                    if (cells.TryGetValue(Normalize(name), out Cell? cell))
                {
                    cell.Contents = oldContents;
                    cell.Value = oldValue;
                }
                //Update dependencies ... again
                foreach (string s in formula.GetVariables())
                    dps.RemoveDependency(s, Normalize(name));
                //If cell was a formula, then we also have to add back the old dependencies
                if (oldContents.GetType() == typeof(Formula))
                    foreach (string s in ((Formula)oldContents).GetVariables())
                        dps.AddDependency(s, Normalize(name));
                throw new CircularException();
            }
        }

        /// <summary>
        /// Lookup to be used to evaluate formulas.
        /// </summary>
        /// <param name="name">Name of the cells value to lookup.</param>
        /// <returns>The cells value as a double, if possible.</returns>
        /// <exception cref="ArgumentException">If the cells value cannot be represented as a double.</exception>
        protected double Lookup(string name)
        {
            //First check the name
            if (!ValidName(Normalize(name)) || !IsValid(Normalize(name)))
                throw new ArgumentException("Invalid variable name.");

            //Now get the cells value
            object val = GetCellValue(Normalize(name));
            //If string, not supported
            if(val.GetType() == typeof(string))
                throw new ArgumentException("Cannot preform operations on a string.");
            //If FormulaError, carry it on
            if (val.GetType() == typeof(FormulaError))
                throw new ArgumentException(((FormulaError)val).ToString());
            //Only other allowed type for value is double.
            return (double)val;
        }

        //View AbstractSpreadsheet for docs.
        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            return dps.GetDependents(Normalize(name));
        }

        /// <summary>
        /// Helper method to determine if a given string is a valid cell name (aka variable).
        /// </summary>
        /// <param name="name">The name to be checked.</param>
        /// <returns>T/F depending on if it is a valid name.</returns>
        private bool ValidName(string name)
        {
            //As a private method there is no need to normalize name as it is insured to be normalized before call.
            Char[] chars = name.ToCharArray();
            //First check that this is indeed at least one letter that ends with a digit
            if (!char.IsLetter(chars[0]))
                return false;
            if (!char.IsDigit(chars[chars.Length - 1]))
                return false;

            //Now make sure it contains only letters and digits
            //and that it is all letters followed by only digits
            bool digitReached = false;
            foreach(char c in chars)
            {
                if (!char.IsLetter(c) && !char.IsDigit(c))
                    return false;
                if (digitReached)
                {
                    if (char.IsLetter(c))
                        return false;
                }
                else
                        if (char.IsDigit(c))
                    digitReached = true;
            }

            //If we make it here, the name is valid
            return true;
        }

        /// <summary>
        /// Class to represent a cell object in this spreadsheet.
        /// </summary>
        [JsonObject(MemberSerialization.OptIn)]
        protected class Cell
        {
            //JSON property for writing cells to files.
            [JsonProperty]
            public string stringForm = "";
            private object content;
            private string name;

            /// <summary>
            /// Constructor for a cell object.
            /// </summary>
            /// <param name="name">The name of this cell.</param>
            /// <param name="contents">The item that this cell contains.</param>
            public Cell(object contents, string name, object value)
            {
                content = contents;
                Contents = contents;
                this.name = name;
                Value = value;
            }

            /// <summary>
            /// Property for the contents of this cell
            /// </summary>
            public Object Contents
            {
                get { return content; }

                set
                {
                    content = value;

                    //Update string form
                    if (value == null)
                        stringForm = "";
                    else if (value.GetType() == typeof(double))
                        stringForm = ((double)value).ToString();
                    else if (value.GetType() == typeof(Formula))
                        stringForm = "=" + ((Formula)value).ToString();
                    else
                        stringForm = (string)value;
                }
            }

            /// <summary>
            /// Property for the value of this cell
            /// </summary>
            public object Value { get; set; }
        }
    }
}
