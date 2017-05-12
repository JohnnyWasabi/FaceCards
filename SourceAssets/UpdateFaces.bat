@echo off
setlocal
set src=G:\temp\Faces\
set dst=FaceCards_Data\StreamingAssets\Faces\
robocopy %src% %dst% /MIR >UpdatesFullLog.txt
findstr .png UpdatesFullLog.txt >FileChangesLog.txt
del UpdatesFullLog.txt

set file="FileChangesLog.txt"
set maxbytesize=1

FOR /F "usebackq" %%A IN ('%file%') DO set size=%%~zA

if %size% LSS %maxbytesize% (
    echo.No changes.
) ELSE (
    echo.Changes:
	type FileChangesLog.txt
)
del FileChangesLog.txt