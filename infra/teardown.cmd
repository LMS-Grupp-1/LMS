@echo off
REM Double-click to run teardown.ps1. Bypasses the execution policy for this run only.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0teardown.ps1"
pause