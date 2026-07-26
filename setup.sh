#!/usr/bin/env bash

# Exit immediately if a command fails
set -e

# Colors
GREEN='\033[0;32m'
CYAN='\033[0;36m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${CYAN}====================================================${NC}"
echo -e "${CYAN}      MatdarSathi Open-Source Developer Bootstrap    ${NC}"
echo -e "${CYAN}====================================================${NC}"

# 1. Check .NET SDK & Build C# API
echo -e "\n${YELLOW}[1/3] Checking .NET SDK & Building C# API Solution...${NC}"
if command -v dotnet &> /dev/null; then
    dotnet build backend/MatdanSathi.sln
    echo -e "${GREEN}✔ .NET API Solution built successfully!${NC}"
else
    echo -e "${YELLOW}⚠️ .NET SDK not detected. Please install .NET 9.0/10 SDK from https://dotnet.microsoft.com/${NC}"
fi

# 2. Install Frontend Node Dependencies
echo -e "\n${YELLOW}[2/3] Installing Frontend Node Dependencies...${NC}"
if command -v npm &> /dev/null; then
    npm --prefix frontend install
    echo -e "${GREEN}✔ Frontend dependencies installed successfully!${NC}"
else
    echo -e "${YELLOW}⚠️ Node.js/npm not detected. Please install Node.js 20+ from https://nodejs.org/${NC}"
fi

# 3. Setup Python Virtual Environment for PDF Parser
echo -e "\n${YELLOW}[3/3] Setting up Python Virtual Environment for PDF Parser...${NC}"
if command -v python3 &> /dev/null; then
    if [ ! -d "parser-service/.venv" ]; then
        python3 -m venv parser-service/.venv
    fi
    parser-service/.venv/bin/pip install -q -r parser-service/requirements.txt
    echo -e "${GREEN}✔ Python Parser virtual environment configured!${NC}"
else
    echo -e "${YELLOW}⚠️ Python3 not detected. Please install Python 3.11+ from https://python.org/${NC}"
fi

echo -e "\n${GREEN}====================================================${NC}"
echo -e "${GREEN}🎉 MatdarSathi Developer Environment Ready!${NC}"
echo -e "${CYAN}To run full build verification:  ./run-pipeline.sh${NC}"
echo -e "${CYAN}To launch using Docker:          docker compose up --build${NC}"
echo -e "${GREEN}====================================================${NC}"
