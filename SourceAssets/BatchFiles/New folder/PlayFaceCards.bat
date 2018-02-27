@echo off
setlocal
set src=K:\Blind Squirrel\Employee List\FaceCardsGame
set dst=.

REM Copy all the batch files that do the updating
start robocopy "%src%\*.bat " %dst% /PURGE /NP /XF %~n0.bat *.log >UpdateBatsFullLog.log

echo.Checking for game and content updates...
call UpdateFaceCards.bat
start FaceCards.exe
start copy /Y /Z "%src%\%~n0.bat " %dst%
