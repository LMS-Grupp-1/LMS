@echo off
REM Double-click to run bootstrap.ps1. Bypasses the execution policy for this run only.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0bootstrap.ps1"
pause