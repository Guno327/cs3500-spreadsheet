using SpreadsheetUtilities;
using SS;
using System.Security.AccessControl;

namespace SpreadsheetGUI;

/// <summary>
/// Backing class for the MainPage GUI.
/// </summary>
public partial class MainPage : ContentPage
{
    //Fields
    private Spreadsheet backing = new(IsValid, s => s.ToUpper(), "ps6");
    private string lastSelected = "";


    /// <summary>
    /// Constructor
    /// </summary>
	public MainPage()
    {
        InitializeComponent();
        spreadsheetGrid.SelectionChanged += displaySelection;
        spreadsheetGrid.SetSelection(0, 0);
    }

    /// <summary>
    /// Event that updates the spreadsheet view to display information
    /// relevant to the cell that has been selected.
    /// </summary>
    /// <param name="grid">The spreadsheet grid that called this event</param>
    private void displaySelection(SpreadsheetGrid grid)
    {
        //First find the selected cell
        spreadsheetGrid.GetSelection(out int col, out int row);

        //Convert to variable form
        string varName = GetVar(col, row);

        //If it is a different cell, we must clear the error message
        if (lastSelected != varName)
            ErrorDisplay.Text = "";

        //Cell
        SelectedCellDisplay.Text = varName;
        
        //Content
        object backingCont = backing.GetCellContents(varName);
        string display = "";
        if(backingCont.GetType() == typeof(Formula))
            display = "=" + backingCont.ToString();
        else
            display = backingCont.ToString();
        SelectedContentEntry.Text = display;

        //Value
        spreadsheetGrid.GetValue(col, row, out string value);
        SelectedValueDisplay.Text = value;

        //Store this as the last cell selected
        lastSelected = varName;
    }

    /// <summary>
    /// Event that runs when the "New" spreadsheet button is selected.
    /// Essential refreshes the spreadsheet to its original state.
    /// </summary>
    /// <param name="sender">Who sent this event</param>
    /// <param name="e">Any arguments associated with this event.</param>
    private async void NewClicked(Object sender, EventArgs e)
    {
        //First make sure the current spreadsheet is saved
        if (backing.Changed)
        {
            bool ans = await DisplayAlert("You Have Unsaved Changes", "Would you like to save them?", "Yes", "No");
            if (ans)
                SaveClicked(sender, e);
        }

        //Then reset the spreadsheet
        spreadsheetGrid.Clear();
        backing = new(IsValid, s => s.ToUpper(), "ps6");
        spreadsheetGrid.SetSelection(0, 0);
        ErrorDisplay.Text = "";
        displaySelection(spreadsheetGrid);
    }

    /// <summary>
    /// Event that runs when the content of a cell is udated.
    /// This method will change the backing SpreadSheet to match this update
    /// and then change the display to match these updates.
    /// </summary>
    /// <param name="sender">Who called this event.</param>
    /// <param name="e">Any arguments associated with this event.</param>
    private void ContentUpdated(object sender, EventArgs e)
    {
        //Get the cell were working with
        spreadsheetGrid.GetSelection(out int col, out int row);
        string cellName = GetVar(col, row);

        //Update the backing spreadsheet
        List<string> toUpdate = new();
        try { toUpdate = backing.SetContentsOfCell(cellName, SelectedContentEntry.Text).ToList<string>(); }
        catch
        {
            ErrorDisplay.Text = "Invalid Entry, Failed to update";
        }
        //Update the display
        UpdateSpreadsheet(toUpdate);

        //Must also update the taskbar display
        displaySelection(spreadsheetGrid);
    }

    /// <summary>
    /// Method that is called when the user selects the option to open a file.
    /// Uses a file picker to allow the user to select the file to open.
    /// Either loads a spreadsheet from the file, or
    /// tells the user there was an error.
    /// </summary>
    /// <param name="sender">Who caused this event</param>
    /// <param name="e">Any event arguments.</param>
    private async void OpenClicked(Object sender, EventArgs e)
    {
        //First make sure the current spreadsheet is saved
        if (backing.Changed)
        {
            bool ans = await DisplayAlert("You Have Unsaved Changes", "Would you like to save them?", "Yes", "No");
            //This is a reuse of code from the save method, Which would normally be bad software design.
            //However, we dont want the open prompt to open before the save prompt is finnished.
            //So it is imperative that we dont call the async SaveClicked method here.
            if (ans)
            {
                //Ask user for path
                string path = await DisplayPromptAsync("Saving", "Enter Full File Path (including name.sprd).");

                //If the field is blank, dont bother trying
                if (path == null)
                    return;

                //Either save or let the user know it failed
                try { backing.Save(path); }
                catch { await DisplayAlert("Error", "Unable to save file.", "OK"); }
            }
        }

        //Then Open the file
        try
        {
            //Select File
            FileResult fileResult = await FilePicker.Default.PickAsync();
            //Make sure they selected a file
            if (fileResult == null)
                throw new ArgumentException("User did not selected a file.");
            //Load File
            backing = new(fileResult.FullPath, IsValid, s => s.ToUpper(), "ps6");
            //Update GUI
            UpdateSpreadsheet(backing.GetNamesOfAllNonemptyCells());
            displaySelection(spreadsheetGrid);
        }
        catch (Exception exp) { await DisplayAlert("Unable to read file.", exp.ToString(), "OK"); }
    }

    /// <summary>
    /// Method that is called when the user selects the option to save the spreadsheet.
    /// Uses the backing spreadsheets save method to save the spreadsheet at the user 
    /// specified location.
    /// </summary>
    /// <param name="sender">Who caused this event.</param>
    /// <param name="e">Any even arguments.</param>
    private async void SaveClicked(Object sender, EventArgs e)
    {
        //Ask user for path
        string path = await DisplayPromptAsync("Saving", "Enter Full File Path (including name.sprd).");

        //If the field is blank, dont bother trying
        if (path == null)
            return;

        //Either save or let the user know it failed
        try { backing.Save(path); }
        catch { await DisplayAlert("Error", "Unable to save file.", "OK"); }
    }

    /// <summary>
    /// Method that runs when the help menu is selected,
    /// </summary>
    /// <param name="sender">Who called this event.</param>
    /// <param name="e">Any event args.</param>
    private async void HelpClicked(Object sender, EventArgs e)
    {
        //Allows for the use of a back button to exit from a chapter back to chapter select.
        bool back = true;
        while (back)
        {
            //Chapter Select
            string choice = await DisplayActionSheet("Help", "Exit", null, "Navigation", "Saving and Loading Files", "Display Bar", "Using the Spreadsheet");
            back = false;
            //Display selected chapter
            switch (choice)
            {
                case "Saving and Loading Files":
                    back = await DisplayAlert("Saving and Loading Files", "-To save and load files, as well as make a new spreadsheet please view the file menu. \n" +
                        "-The save menu requires a complete file path with the file name and extension. For Example: C:\\Users\\User\\Documents\\test.sprd \n" +
                        "-Please use the .sprd file extension for all files you save and load. \n" +
                        "-The open file menu will only work with valid .sprd files \n" +
                        "-Selecting new/open before saving a spreadsheet will prompt you to save your work to prevent unintentional loss of data.","Back", "Exit");
                    break;
                case "Using the Spreadsheet":
                    back = await DisplayAlert("Using the Spreadsheet", "-A cell may contain either a number, text, or a formula. \n" +
                        "-All formulas must start with =. IE =A1 would be a formula that is just the value of cell A1. \n" +
                        "-Cell names are not case sensitive. IE A1 = a1. \n" +
                        "-When making formulas it is possible to set invalid values. \n These can take the form of either errors or exceptions. \n" +
                        "Errors are for formulas that are valid but the cell values that they use are not. They will result in the value of a cell displaying ERROR. \n" +
                        "Exceptions mean that it is an invalid formula and will result in a messege being displayed in the Display Bar as well as not updating the contents. \n" +
                        "-Cells have a 10 char limit to their displays. If you exceed this limit the display will be truncated. However, The original value is still held and used for formula purposes.",
                        "Back", "Exit");
                    break;
                case "Display Bar":
                    back = await DisplayAlert("Display Bar", "-The display bar has 4 sections. The Name, Value, Content, and Error sections \n" +
                        "-The Name section displays the name of the cell you currently have selected. \n" +
                        "-The Value section displays what will be displayed in the actual spreadsheet cell. \n" +
                        "-The Content section is what you will edit and hold the unevaluated form of the Value section. \n" +
                        "-The Error section is hidden by default, but will display messegaes related to content exceptions.",
                        "Back", "Exit");
                    break;
                case "Navigation":
                    back = await DisplayAlert("Navigation", "-You navigate the spreadsheet with your mouse. \n" +
                        "-Click on a cell to select it. \n" +
                        "-On the Display Bar (tm) you can edit the cells contents in the entry. \n" +
                        "-You may use the enter button or the enter key on your keyboard to update values. \n" +
                        "-The menu bar at the top of the page contains file and help options. \n" +
                        "-You may scroll vertically and horizontally on the spreadsheet.",
                        "Back", "Exit");
                    break;
            }
        }
        
    }

    /// <summary>
    /// Private helper method that determines if a given variable name is valid for our spreadsheet.
    /// </summary>
    /// <param name="s">The name to be checked</param>
    /// <returns>T/F depending on if it is valid or not</returns>
    static private bool IsValid(string s)
    {
        //Check Length
        if (s.Length != 2 && s.Length != 3)
            return false;

        //Well need the char array for the next checks
        char[] c = s.ToCharArray();

        //First char must be a letter
        if (!char.IsLetter(c[0]))
            return false;

        //Second char must be number 1-9
        if (int.TryParse("" + c[1], out int i))
        {
            if (i == 0)
                return false;
        }
        else
            return false;

        //Third char (if it exsists) must be number 0-9
        if (s.Length == 3 && !char.IsDigit(c[2]))
            return false;

        //If we make it here, it is a valid name
        return true;
    }

    /// <summary>
    /// Private helper method that converts the row and col values
    /// of a cell to its name in variable form.
    /// IE (0,0) == "A1"
    /// </summary>
    /// <param name="col"></param>
    /// <param name="row"></param>
    static private string GetVar(int col, int row)
    {
        return (char)(col + 65) + (row + 1).ToString();
    }

    /// <summary>
    /// Private helper method that converts a variable name into its row and col values
    /// IE "A1" == (0,0)
    /// </summary>
    /// <param name="name">The variable name to be converted</param>
    /// <param name="col">Out parameter to hold the column number</param>
    /// <param name="row">Out parameter to hold the row number</param>
    /// <exception cref="ArgumentException">Throws if the given name is not a valid cell</exception>
    static private void GetLocation(string name, out int col, out int row)
    {
        if(name.Length != 2 && name.Length != 3)
            throw new ArgumentException("Invalid name length.");

        char[] c = name.ToCharArray();
        //Convert the letter to a position
        col = c[0] - 'A';
        //Convert the number to a position
        if (name.Length == 2)
            row = c[1] - '1';
        else
        {
            row = 0;
            row += (c[1] - '0') * 10;
            row += c[2] - '1';
        } 
    }

    /// <summary>
    /// Updates the GUI to reflect the backing spreadsheet.
    /// It does this by updating all the cells given to the function
    /// in the names IEnumerable, in their var form IE A1.
    /// </summary>
    /// <param name="names">Cells to have their display updated</param>
    private void UpdateSpreadsheet(IEnumerable<string> names)
    {
        foreach (string s in names)
        {
            try
            {
                //Convert to location form var
                GetLocation(s, out int c, out int r);
                //Find the "new" value to be displayed
                object backingVal = backing.GetCellValue(s);
                string display = "";
                if (backingVal.GetType() == typeof(SpreadsheetUtilities.FormulaError))
                    display = "ERROR";
                else
                    display = backingVal.ToString();
                if (display.Length > 10)
                    display = Truncate(backingVal);
                //Display the value
                spreadsheetGrid.SetValue(c, r, display);
            }
            catch (ArgumentException mess) { Console.WriteLine(mess.ToString()); }
        }
    }

    /// <summary>
    /// ADDITIONAL FEATURE: Any value that is going to be displayed that is longer than 10 characters
    /// will be truncated by this method. Must pass an object thats string form is > 10 chars.
    /// Rules:
    /// -If it is a number, it will be return in scientific notation with 3 decimal points of precision
    /// -If it is a string, it will return the first 10 characters followed by "..."
    /// </summary>
    /// <param name="val">object to be converted</param>
    /// <returns>Truncated string version</returns>
    private string Truncate(object val)
    {
        if (val.GetType() == typeof(double))
            return ((double)val).ToString("E3");
        else
            return (val.ToString()).Substring(0, 10) + "...";
    }

    /// <summary>
    /// ADDITONAL FEATURE: Method that is called when the option to enable dark mode is clicked.
    /// Enables the spreadsheetGrids DarkMode flag and changes some of the color pallete and then 
    /// forces the view to redraw itself.
    /// </summary>
    /// <param name="sender">Who caused this event</param>
    /// <param name="e">Any event args</param>
    private void EnableDarkMode(Object sender, EventArgs e)
    {
        spreadsheetGrid.DarkMode = true;
        spreadsheetGrid.BackgroundColor = Colors.Black;
        //Will force view to redraw itself
        spreadsheetGrid.GetSelection(out int col, out int row);
        spreadsheetGrid.SetSelection(col, row);

    }

    /// <summary>
    /// ADDITONAL FEATURE: Method that is called when the option to disable dark mode is clicked.
    /// Disables the spreadsheetGrids DarkMode flag and changes some of the color pallete and then 
    /// forces the view to redraw itself.
    /// </summary>
    /// <param name="sender">Who caused this event</param>
    /// <param name="e">Any event args</param>
    private void DisableDarkMode(Object sender, EventArgs e)
    {
        spreadsheetGrid.DarkMode = false;
        spreadsheetGrid.BackgroundColor = Colors.LightGray;
        //Will force view to redraw itself
        spreadsheetGrid.GetSelection(out int col, out int row);
        spreadsheetGrid.SetSelection(col, row);
    }
}
