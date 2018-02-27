@echo off
setlocal
set src=\\bsg-ts\Share\Blind Squirrel\Employee List\FaceCardsGame
set desktop=%HOMEDRIVE%%HOMEPATH%\Desktop
set dst=%desktop%\FaceCardsGame
echo.Copying %src% to %dst%

robocopy "%src%" "%dst%" /MIR /NP 

rem mklink %desktop%\PlayFaceCards %dst%\PlayFaceCards.bat
rem Create shortcut on desktop to the PlayFaceCards batch file.
powershell "$s=(New-Object -COM WScript.Shell).CreateShortcut('%desktop%\Play FaceCards.lnk');$s.TargetPath='%dst%\PlayFaceCards.bat';$s.WorkingDirectory='%dst%';$s.IconLocation='%dst%\FaceCards.exe,0';$s.Save()"

%HOMEDRIVE%
cd %dst%
echo FaceCards and a 'Play FaceCards' shortcut are now installed on your Desktop. 
echo FaceCards is ready to launch.
pause

start FaceCards.exe
