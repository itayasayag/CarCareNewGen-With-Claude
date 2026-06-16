@echo off
chcp 65001 >nul
title CarCare Launcher

REM ============================================================
REM  CarCare - double-click to start both servers
REM  Place this file in the repo root (next to server\ client\)
REM ============================================================

REM This .bat sits in the repo root, so %~dp0 is the project folder
set "PROJECT=%~dp0"

echo.
echo   CarCare - Starting servers...
echo.

REM ---- Backend (port 8000) ----
echo   Backend  - http://127.0.0.1:8000
start "CarCare Backend" cmd /k "cd /d "%PROJECT%server" && py -3.11 -m uvicorn main:app --reload --port 8000 --host 0.0.0.0"

REM ---- Wait a few seconds for backend to boot ----
timeout /t 5 /nobreak >nul

REM ---- Frontend (port 5500) ----
echo   Frontend - http://localhost:5500/Pages/SignUpPage.html
start "CarCare Frontend" cmd /k "cd /d "%PROJECT%client" && py -3.11 -m http.server 5500"

REM ---- Open browser ----
timeout /t 2 /nobreak >nul
start http://localhost:5500/Pages/SignUpPage.html

echo.
echo   Both servers launched in separate windows.
echo   Close those two windows to stop the servers.
echo.
echo   App      : http://localhost:5500/Pages/SignUpPage.html
echo   Backend  : http://127.0.0.1:8000
echo   API Docs : http://127.0.0.1:8000/docs
echo.
pause
