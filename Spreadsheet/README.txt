Author: John Stevens, CS3500 FA2021

General use instructions:
1. When you first open a spreadsheet, a cell will be highlighted but not really "selected" so you will have to click on one to begin editing/using the arrow keys to navigate.
2. Once you start typing in the cell contents text box, the arrow keys will be disabled for traversing the spreadsheet and instead revert to normal arrow key functionality for editing text.
   When you hit enter, the arrow keys will again be used for traversing the spreadsheet.
3. If you click on the Show Dependents button and then click on a cell, it will highlight any dependents of that cell in red. To get rid of the highlights, simply click on another cell.
4. Other than these few things it functions like a regular spreadsheet.
5. Also, buttons for creating new spreadsheets, closing, saving, and opening saved spreadsheets are in the top left hand corner under the File tab.





---------------------------------------------DEVELOPMENT LOG------------------------------------------------
10/18/2021
I really didn't get very far today. I got the basic project setup as well as added some text boxes but thats it.

10/19/2021
I got a lot of help from the help session today. Methods cellCoordsToName and cellNameToCoords should be accredited to the TA who ran that session.
I also got the backing spreadsheet to interface and display its values on the UI spreadsheet and added use of the arrow keys to navigate the spreadhsheet.
Semi-important design decision: the arrow keys are detected through ContentsTextBox, which means that it has to be "in focus" for you to navigate.
This isn't a huge deal and the implementation is pretty seamless, but it is something to note.

10/20/2021
I implemented the special feature: visualize dependents. It has a button that if you push it and then select a cell, it will highlight all the cells that depend on it.
To do this, I had to add some helper methods to the SpreadsheetPanel.cs file that modify the code that paints the screen. I borrowed some code for opening the file browser from
microsoft's website (OpenFileDialog Class page).

10/21/2021
I finished implementing the open file feature. Also implemented the save feature. I actually made the code for saving a helper method so that both the "Open" and "Save" buttons can easily access
it. This way I can have a popup asking if the user wants to save before they open a new spreadsheet if they've made any changes to the current one. I added a few try/catch blocks for things
such as loading the wrong file type, circular exceptions, inputing invalid arguments, etc.

10/22/2021
Added the help button. Have begun commenting everything in addition to comments that were made along the way. Fixed some issues with saving dialogue boxes, added some finishing touches and submitted.
