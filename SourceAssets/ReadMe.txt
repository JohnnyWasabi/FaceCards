FaceCards is a flash-card style game for learning people's names.  Written by John Alvarado.

Installation/Running:
--------------------
Copy the whole "FaceCardsGame" folder to your destktop. Open the folder on your desktop and double click the "FaceCards" application. Choose at least 1600x1024 when prompted for resolution. It can be played at lower resolution, but the Yearbook mode currently does not scale to fit and will go off the bottom at lower resolutions.

Updating Pictures:
-----------------
There is a batch file called UpdateFaces.bat that you can launch to update your local store of pictures from the network with any new people that have joined or to remove people that have left the company.  It will show you what files were added or removed. 

Update and Play:
----------------
There is a batch file called UpdateAndPlay.bat that you can launch to update the face photos and then play.

Play Instructions:
-----------------
Type the full name of the person shown. 
Correct letters are added as you type; wrong letters appear in red to the right of the cursor and make a hangman progress 1 step closer to his gruesome death.  
The name turns green when complete.
Press Enter key to advance to next person. 
Press Right-Arrow to reveal the next letter, but it costs you 3 hangman parts!
Pressing Enter before the name is complete reveals the full name (each revealed letter costs 3 hangman parts).

Goal:
----
Collect (turn over) all the face cards by typing their names in correctly. If the hangman dies you don't collect the card. But don't fret! The missed card will come around again.  See how quickly and accurately you can collect all the face cards to achieve the highest score.

Stats Display:
-------------
Time				= A timer displays elapsed game time. E.g. 00:44
Collected Counter	= Displays number of cards collected over total cards. E.g. 3/97
Accuracy %			= Displays percentage accuracy (Correctly typed letters divided by total letters typed). E.g. 96%
Score				= At the end of a full game (All Depts. and First & Last names) you get a score result from an formulat that incorporates the total characters in all the names, completion time, and percent accuracy.

Special Keys:
------------
Enter Key 	= Reveal full name or Advance to next person (if full name already displayed).
Righ Arrow 	= Hint: Reveals next letter. Costs 3 hangman steps.
Left Arrow	= Erase last letter from name entry. In case you want to back up and retype for practice.
Backspace 	= Effectively nothing, except clear the display of the last wrong letter you typed. 
PageUp 		= Show previous person
PageDown 	= Show next person.

Buttons:
--------
Mode		= This is a selector that lets you choose different modes of the app:
			Memory Game: The memory game described above where you type in the name when shown a face.
			Flash Cards: All faces are shown without names. Click on a face to enlarge it and see their name info.
			Yearbook:    All faces are shown with their names and department below their picture.	
			Map:         Displays faces on a blueprint of the office to show where people sit.
Department	= This is a selector that lets you filter to show a single department (or All Departments).
Guess		= This is a selector of what name you have to enter to collect the card in the Memory Game (absent in other modes); it determines sorting order in Flash Cards and Yearbook modes: 
			First & Last 
			First 
			Last 
			Department 
Sort by* 	= This is a selector that determines sorting order in Flash Cards and Yearbook modes (absent in Memory Game): 
			First & Last 
			First 
			Last 
			Department 
			Tenure Most
			Tenure Least	
Find by		= Map mode only. This is a selector that determines what to type in to highlight faces in Map mode:
			First & Last 
			First 
			Last 
			Department 
			("Tenure Most" and "Tenure Least" appear but are not compatible with Map mode)
Tenure Filter	= This is a selector that lets you show only the N most or least tenured people by adjusting a slider to adjust N
			All 		(shows all people regardless of tenure and hides slider)
			Most 		(shows N most tenured people and reveals slider to control N)
			Least 		(shows N least tenured people and reveals slider to control N)
			OGs*		(shortcut to select "Most" and set N to 10% of total people, and in non-Memory-Game modes sets "Sort by*" selector to "Tenure Most")
			Newbies*	(shortcut to select "Least" and set N to 10% of total people, and in non-Memory-Game modes sets "Sort by*" selector to "Tenure Least")
Case-sensitive	= Toggle between case-sensitive or case-insensitive name-entry. When case-insensitive, wrong case still counts against accuracy, but not for hangman.
Shuffle		= Shuffles the cards and redeals the 1st card (does not restart game).
Restart		= Shuffles the cards and restarts the game.
Exit		= Quits the app without a confirmation (because this is a deadly game).
Cards/Faces	= Click on any card in the grid to display it large/featured.  Click on the large/featured card to send it back to the grid.
Background	= Right-click with mouse on background to access color-picker dialog for changing the background color. Color is saved in player prefs.

Note: Play at minimum of 1024x768 resolution.  For Yearbook display at least 1600x1024. However, in windowed mood you can resize the window as needed during the game.

Version History
----------------
2.2	Added margins on top and sides of screen so cards don't rest against edges of screen.
2.3	Added Tenure Filter with slider. Moved "Case Sensitive" toggle to right side and slid Selector buttons left. Made the Guess/Sort selector change based on mode between "Guess" and "Sort by". Added "Tenure Least" to sort order options.
2.4	Added "OGs" and "Newbies" shortcuts to Tenure filter. These set up the Tenure filter (and "Sort By" setting if not in Memory Game mode) to show the 10% of employees with most or least tenure.
3.0	Added "Map" mode, which shows faces displayed on a blueprint of the office to show where people sit.
3.1	Updates for changes to user_locations file format (quotes and multiple seats separated by /). Shows number of matches in Map mode. Show projects in Map mode if full name selected. Show possible next letters to type in Map mode.
