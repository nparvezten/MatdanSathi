import re
from typing import Generator, Dict, Any
import fitz  # PyMuPDF

# Optimized regex patterns
EPIC_PATTERN = re.compile(r'\b[A-Z]{3}\d{7}\b|\b[A-Z]{3}/\d{2}/\d{3}/\d{6}\b')
NAME_PATTERN = re.compile(r'(?:Name|नाम)\s*[:\.\s\-]*\s*([A-Za-z \t\.\u0900-\u097F]+)', re.IGNORECASE)
HOUSE_NO_PATTERN = re.compile(r'(?:House\s*No\.?|House\s*Number|गृह\s*संख्या|मकान\s*नंबर)\s*[:\.\s\-]+\s*([A-Za-z0-9/\- \t]+)', re.IGNORECASE)
AGE_PATTERN = re.compile(r'(?:Age|आयु)\s*[:\.\s\-]+\s*(\d+)', re.IGNORECASE)
GENDER_PATTERN = re.compile(r'(?:Sex|Gender|लिंग)\s*[:\.\s\-]+\s*(Male|Female|Others|Third\s*Gender|पुरुष|महिला|अन्य)', re.IGNORECASE)

def parse_electoral_roll(file_bytes: bytes) -> Generator[Dict[str, Any], None, None]:
    """
    High-performance stream-based parser for Indian Electoral Rolls.
    Processes pages sequentially using PyMuPDF to keep memory footprint minimal.
    """
    # Open PDF stream
    doc = fitz.open(stream=file_bytes, filetype="pdf")
    
    for page_num in range(len(doc)):
        page = doc.load_page(page_num)
        text = page.get_text("text")
        
        # Parse page metadata or constituency details from headers/footers (mock values for now)
        assembly_constituency = "Constituency 1"
        part_number = f"Part-{page_num + 1}"
        section_number = "Section-1"
        polling_station_name = "Government Primary School"
        polling_station_location = "Room 1"
        
        # Find all EPIC card number matches and their positions in the text
        epic_matches = list(EPIC_PATTERN.finditer(text))
        if not epic_matches:
            continue
            
        # Parse by chunking text between consecutive EPIC number matches
        for i in range(len(epic_matches)):
            start_idx = epic_matches[i].start()
            end_idx = epic_matches[i+1].start() if i + 1 < len(epic_matches) else len(text)
            
            chunk = text[start_idx:end_idx]
            
            # Extract fields using regex patterns
            epic_number = epic_matches[i].group(0)
            
            name_match = NAME_PATTERN.search(chunk)
            name = name_match.group(1).strip() if name_match else "UNKNOWN"
            
            house_match = HOUSE_NO_PATTERN.search(chunk)
            house_no = house_match.group(1).strip() if house_match else "N/A"
            
            age_match = AGE_PATTERN.search(chunk)
            age = int(age_match.group(1)) if age_match else 0
            
            gender_match = GENDER_PATTERN.search(chunk)
            gender = gender_match.group(1).strip() if gender_match else "UNKNOWN"
            
            # Map Hindi gender designations to standard English values
            if gender in ["पुरुष", "Male"]:
                gender = "Male"
            elif gender in ["महिला", "Female"]:
                gender = "Female"
            
            # Yield record immediately as a dictionary
            yield {
                "epicNumber": epic_number,
                "fullName": name,
                "age": age,
                "dateOfBirth": f"01/01/{2026 - age}" if age > 0 else "01/01/2000",
                "gender": gender,
                "assemblyConstituency": assembly_constituency,
                "partNumber": part_number,
                "sectionNumber": section_number,
                "serialNumber": i + 1,
                "pollingStationName": polling_station_name,
                "pollingStationLocation": polling_station_location,
                "bloName": "TBD",
                "bloContact": "0000000000"
            }
            
    doc.close()
