import os
import sys
import pytest
from fastapi.testclient import TestClient

# Ensure parser-service is in sys.path
sys.path.append(os.path.dirname(os.path.abspath(__file__)))

from main import app
from config import settings
from parser import EPIC_PATTERN, NAME_PATTERN, HOUSE_NO_PATTERN, AGE_PATTERN, GENDER_PATTERN

client = TestClient(app)

def test_health_check():
    response = client.get("/health")
    assert response.status_code == 200
    assert response.json() == {"status": "healthy"}

def test_parse_requires_auth():
    # Attempting to parse a dummy file without API KEY should return 401 Unauthorized
    response = client.post(
        "/api/v1/parser/parse", 
        files={"file": ("dummy.pdf", b"test content", "application/pdf")}
    )
    assert response.status_code == 401
    assert "detail" in response.json()

def test_parse_verifies_valid_auth():
    # Attempting to parse a non-pdf file with valid key should return 400 Bad Request (file type validation)
    headers = {"X-API-KEY": settings.INTERNAL_API_KEY}
    response = client.post(
        "/api/v1/parser/parse", 
        headers=headers,
        files={"file": ("dummy.txt", b"test content", "text/plain")}
    )
    assert response.status_code == 400
    assert "must be a PDF" in response.json()["detail"]

def test_regex_extraction_on_standard_block():
    text_block = """
    ABC1234567
    Name: Ramesh Kumar
    Father's Name: Suresh Kumar
    House No: 42-A/1
    Age: 45 Gender: Male
    """
    
    # Assert EPIC Pattern
    epic_match = EPIC_PATTERN.search(text_block)
    assert epic_match is not None
    assert epic_match.group(0) == "ABC1234567"
    
    # Assert Name Pattern
    name_match = NAME_PATTERN.search(text_block)
    assert name_match is not None
    assert name_match.group(1).strip() == "Ramesh Kumar"
    
    # Assert House No Pattern
    house_match = HOUSE_NO_PATTERN.search(text_block)
    assert house_match is not None
    assert house_match.group(1).strip() == "42-A/1"
    
    # Assert Age Pattern
    age_match = AGE_PATTERN.search(text_block)
    assert age_match is not None
    assert int(age_match.group(1)) == 45
    
    # Assert Gender Pattern
    gender_match = GENDER_PATTERN.search(text_block)
    assert gender_match is not None
    assert gender_match.group(1).strip() == "Male"

def test_regex_extraction_on_hindi_block():
    text_block = """
    XYZ9876543
    नाम : दिनेश सिंह
    पिता का नाम: सुंदर सिंह
    मकान नंबर : 124
    आयु : 32 लिंग : पुरुष
    """
    
    epic_match = EPIC_PATTERN.search(text_block)
    assert epic_match is not None
    assert epic_match.group(0) == "XYZ9876543"
    
    name_match = NAME_PATTERN.search(text_block)
    assert name_match is not None
    assert name_match.group(1).strip() == "दिनेश सिंह"
    
    house_match = HOUSE_NO_PATTERN.search(text_block)
    assert house_match is not None
    assert house_match.group(1).strip() == "124"
    
    age_match = AGE_PATTERN.search(text_block)
    assert age_match is not None
    assert int(age_match.group(1)) == 32
    
    gender_match = GENDER_PATTERN.search(text_block)
    assert gender_match is not None
    assert gender_match.group(1).strip() == "पुरुष"
