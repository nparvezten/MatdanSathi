#!/usr/bin/env bash

# Exit immediately if any command fails
set -e

# ANSI Color Codes for premium terminal formatting
GREEN='\033[0;32m'
RED='\033[0;31m'
CYAN='\033[0;36m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${CYAN}====================================================${NC}"
echo -e "${CYAN}      MatdanSathi Ingestion & Secure Build Pipeline   ${NC}"
echo -e "${CYAN}====================================================${NC}"

# 1. Verification of C# .NET Backend
echo -e "\n${YELLOW}[1/4] Restoring and Building C# .NET API Layer...${NC}"
dotnet build backend/MatdanSathi.sln

echo -e "${YELLOW}[1/4] Running C# static analysis check...${NC}"
dotnet format backend/MatdanSathi.sln --verify-no-changes

echo -e "${YELLOW}[1/4] Running Backend xUnit Tests...${NC}"
dotnet test backend/MatdanSathi.sln --no-build

# 2. Verification of Python Fast API Parser Service
echo -e "\n${YELLOW}[2/4] Running Python FastAPI Parser pytest Suite...${NC}"
if [ -f "parser-service/.venv/bin/pytest" ]; then
    parser-service/.venv/bin/pytest parser-service/
else
    echo -e "${RED}Error: Python virtual environment or pytest not found in parser-service/.venv${NC}"
    exit 1
fi

echo -e "${YELLOW}[2/4] Running Python SAST Bandit Security Scan...${NC}"
parser-service/.venv/bin/bandit -r parser-service/ -x parser-service/.venv/,parser-service/test_parser.py,parser-service/test_security.py

# 3. Compilation of Frontend Angular Dashboard
echo -e "\n${YELLOW}[3/4] Building Frontend Angular Standalone App...${NC}"
npm --prefix frontend run build

# 4. Synchronizing Assets to Native Capacitor Wrappers
echo -e "\n${YELLOW}[4/4] Syncing Assets to Capacitor iOS/Android Wrappers...${NC}"
(cd frontend && npx cap sync)

echo -e "\n${GREEN}✔ Build Verification Pipeline Completed Successfully!${NC}"
echo -e "${CYAN}====================================================${NC}"
