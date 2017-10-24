@echo off
setlocal
set src="K:\Blind Squirrel\Employee List\FaceCardsGame\FaceCards_Data\StreamingAssets\Faces\ "
set dst=FaceCards_Data\StreamingAssets\Faces\

robocopy %src% %dst% /MIR >UpdatesFullLog.txt

findstr .png UpdatesFullLog.txt >FilesUpdatedLog.txt
del UpdatesFullLog.txt

set file="FilesUpdatedLog.txt"
set maxbytesize=1
FOR /F "usebackq" %%A IN ('%file%') DO set size=%%~zA

if %size% LSS %maxbytesize% (
    echo.No Photo changes.
) ELSE (
    echo.Updates:
	type %file%
)
del %file%

pause
