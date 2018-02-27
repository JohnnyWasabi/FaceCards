@echo off
setlocal

set src=C:\Users\jalvarado\Documents\GitHub\FaceCards
set dst=\\bsg-ts\Share\Blind Squirrel\Employee List\FaceCardsGame

REM Executable build
robocopy "%src%\Builds\FaceCards" "%dst%" /MIR /XD "%src%\Builds\FaceCards\FaceCards_Data\StreamingAssets\Faces" "%src%\Builds\FaceCards\FaceCards_Data\StreamingAssets\Output" /XF *.csv


REM Readme file
copy "%src%\SourceAssets\readme.txt" "%dst%" 

REM Play and Update batch files
copy UpdateFaceCards.bat "%dst%" 
copy PlayFaceCards.bat "%dst%" 

dir "%dst%"
echo.Publish is complete
pause
