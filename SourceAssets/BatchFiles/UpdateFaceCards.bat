@echo off
REM Written by John Alvarado
setlocal
set src="\\bsg-ts\Share\Blind Squirrel\Employee List\FaceCardsGame\ "
set dst=.

robocopy %src% %dst% /MIR /NP /XF UpdateFaces.bat PlayFaceCards.bat TestBatUpdate.bat *.log >UpdatesFullLog.log

findstr ".png .csv .txt .dll .exe" UpdatesFullLog.log >FilesUpdatedLog.log

del UpdatesFullLog.log

set file="FilesUpdatedLog.log"
set maxbytesize=1
FOR /F "usebackq" %%A IN ('%file%') DO set size=%%~zA

if %size% LSS %maxbytesize% (
    echo.You are up to date!
) ELSE (
    echo.Updates applied:
	type %file%
    echo.
    echo.You are now up to date!
)
del %file%

pause
