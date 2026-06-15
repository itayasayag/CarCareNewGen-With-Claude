#!/usr/bin/env bash
# ============================================================
#  CarCare — Start both servers
#  Run from the repo root:  bash start.sh
# ============================================================

BACKEND_PORT=8000
FRONTEND_PORT=5500

GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo ""
echo -e "${BLUE}🚗  CarCare — Starting servers...${NC}"
echo ""

# ---- Convert WSL/Bash path to Windows path ----
WIN_PROJECT=$(pwd | sed 's|^/mnt/||' | sed 's|^\(.\)/|\1:\\|' | sed 's|/|\\|g')
echo -e "   Path: ${WIN_PROJECT}"
echo ""

# ---- Write a .bat launcher and run it ----
BAT_FILE="${WIN_PROJECT}\\__carcare_start__.bat"
BAT_BASH="$(pwd)/__carcare_start__.bat"

cat > "$BAT_BASH" << BATEOF
@echo off
start "CarCare Backend"  cmd /k "cd /d "${WIN_PROJECT}\server"  && py -3.11 -m uvicorn main:app --reload --port ${BACKEND_PORT} --host 0.0.0.0"
timeout /t 5 /nobreak >nul
start "CarCare Frontend" cmd /k "cd /d "${WIN_PROJECT}\client" && py -3.11 -m http.server ${FRONTEND_PORT}"
timeout /t 2 /nobreak >nul
start http://localhost:${FRONTEND_PORT}/Pages/SignUpPage.html
BATEOF

echo -e "${YELLOW}▶  Launching via .bat file...${NC}"
cmd.exe /c "$BAT_FILE"

echo ""
echo -e "${GREEN}✅  Both servers launched in separate windows!${NC}"
echo ""
echo -e "   App      : http://localhost:${FRONTEND_PORT}/Pages/SignUpPage.html"
echo -e "   Backend  : http://127.0.0.1:${BACKEND_PORT}"
echo -e "   API Docs : http://127.0.0.1:${BACKEND_PORT}/docs"
echo ""
echo -e "   ${YELLOW}Close the two CMD windows to stop the servers.${NC}"

# Clean up the temp bat file
rm -f "$BAT_BASH"
