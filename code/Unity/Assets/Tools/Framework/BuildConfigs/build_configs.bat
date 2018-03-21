@echo off
cd /d "%1\..\..\..\config\client"
python jsonbuild.py
xcopy "%cd%\json\*.json" "%1\" /s /y
pause
