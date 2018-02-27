@echo off
REM Written by John Alvarado
setlocal
set src=\\bsg-ts\Share\Blind Squirrel\Employee List\FaceCardsGame
set dst=.
set tempBat=temp.bat

REM Update the batch files.
REM The first time this batch file is executed (without parameters) it creates and launches a temp batch file to update all the batch file from the network (including this one).
REM That temp batch file, after upating the bats, runs this batch file again but with a command line param 'NOUPDATE' to skip this update step.  
REM The temp batch file is then removed when the content update occurs, so you will never see it (unless you add temp.bat the /XF list in UpdateFaceCards.bat robocopy command)
if '%1'=='NOUPDATE' goto SkipUpdatingThisfile
echo:copy /Y /Z "%src%\*.bat " %dst%>%tempBat%
echo:start %~n0.bat NOUPDATE>>%tempBat%
%tempBat%
exit
:SkipUpdatingThisfile


REM Update the game executable and content files
echo.Checking for game and content updates...
call UpdateFaceCards.bat "%src%"


REM Launch the game
start FaceCards.exe
exit

