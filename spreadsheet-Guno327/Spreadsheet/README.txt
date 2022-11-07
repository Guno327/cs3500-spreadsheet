Spreadsheet Project for CS3500 2022 Fall Semester.
Coded by Gunnar Hovik.

About:
This project is essentially a ghetto version of excel that is written in C# using MAUI for the GUI.

Features:
Has all the basic features of excel. That included a display of cells (in our case A-Z & 1-99) that can have their contents edited.
The spreadsheet has support for cells to store text, numbers, or formulas.
Formulas are evaluated in infix notation with the ability to refrence other cells using the form of one letter (case doesnt matter)
	followed by 1-2 digits with the first digit being limited to 1-9.

About Using the Spreadsheet (Can also be found in the help menu in application):
Saving and Loading Files:
	-To save and load files, as well as make a new spreadsheet please view the file menu.
    -The save menu requires a complete file path with the file name and extension. For Example: C:\\Users\\User\\Documents\\test.sprd
    -Please use the .sprd file extension for all files you save and load.
    -The open file menu will only work with valid .sprd files.
    -Selecting new/open before saving a spreadsheet will prompt you to save your work to prevent unintentional loss of data.
Using the Spreadsheet:
    -A cell may contain either a number, text, or a formula.
    -All formulas must start with =. IE =A1 would be a formula that is just the value of cell A1.
    -Cell names are not case sensitive. IE A1 = a1.
    -When making formulas it is possible to set invalid values. \n These can take the form of either errors or exceptions.
     Errors are for formulas that are valid but the cell values that they use are not. 
     They will result in the value of a cell displaying ERROR.
     Exceptions mean that it is an invalid formula and will result in a messege being displayed in the Display Bar as 
     well as not updating the contents.
    -Cells have a 10 char limit to their displays. If you exceed this limit the display will be truncated.
     However, The original value is still held and used for formula purposes.
Display Bar:
    -The display bar has 4 sections. The Name, Value, Content, and Error sections.
    -The Name section displays the name of the cell you currently have selected.
    -The Value section displays what will be displayed in the actual spreadsheet cell.
    -The Content section is what you will edit and hold the unevaluated form of the Value section.
    -The Error section is hidden by default, but will display messegaes related to content exceptions.
Navigation:
    -You navigate the spreadsheet with your mouse.
    -Click on a cell to select it.
    -On the Display Bar (tm) you can edit the cells contents in the entry.
    -You may use the enter button or the enter key on your keyboard to update values.
    -The menu bar at the top of the page contains file and help options.
    -You may scroll vertically and horizontally on the spreadsheet.

Additonal Features:
One of the requirments for this assignment was to add additional feature(s) not required by the rubric.
This section is for TA refrence to see what my additonal features are:
Truncation of Values:
    Normally since cells don't resize themselves if you were to input a very long string or number the value
    would flow out of its cell and overlap with the next one. This is an issue that I have fixed by causing my spreadsheet
    to truncate the values that are displayed in the cells. This does not actually effect the backing content of value of the cell
    only what is displayed on the screen. It works by calling a truncate method whenever it detects the string to be displayed to be
    longer that 10 characters in length. If the original value was a number it then converts this number into scientific notation with
    a percision of 3 decimal places. If the original value was a string it displays the first 10 characters of the string followed by
    "..." to let the user know that not the whole value is being displayed.
Dark Mode:
    In almost any application that is being used today they offer a "Dark Mode" that allows the user to swap the UI color
    pallete for one that is less taxing on the eyes. I have added that functionality through the use of a drop down menu.
    The dark mode toggle can be found right next to the help and file menus. It works by changing the colors that the backing SpreadsheetGrid
    uses to draw itself.

Shortcomings:
This spreadsheet is essentially fully funcitonal, however there are still some little annoyances that exsist.
These include the fact that I was unable to get the content entry box to autoselect itself when selecting a new cell.
This means that you have to click on the new cell and then move your mouse all the way to the top of the window in order
to select the entry box to update the content. Another annoyance is the fact that navigation is only possible with the mouse.
I wanted to implement keyboard only navigation, but with the first issue that became impossible. There is also the fact that I
would have to use another library to deal with key input events, which would not be allowed for this assignment.
