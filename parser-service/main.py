import json
from fastapi import FastAPI, UploadFile, File, Header, HTTPException, status
from fastapi.responses import StreamingResponse
from config import settings
from parser import parse_electoral_roll

app = FastAPI(title="MatdanSathi Electoral Roll Parsing Service", version="1.0.0")

def verify_api_key(api_key: str = Header(None, alias="X-API-KEY")):
    """Verifies that requests originate from authorized internal services."""
    if not api_key or api_key != settings.INTERNAL_API_KEY:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Invalid or missing X-API-KEY header."
        )

@app.get("/health")
def health_check():
    return {"status": "healthy"}

@app.post("/api/v1/parser/parse")
async def parse_pdf(
    file: UploadFile = File(...),
    api_key: str = Header(None, alias="X-API-KEY")
):
    # Verify secure internal headers
    verify_api_key(api_key)
    
    if not file.filename.endswith(".pdf"):
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Uploaded file must be a PDF."
        )
        
    file_bytes = await file.read()
    
    def generator():
        try:
            # Yield records immediately as newline-delimited JSON lines
            for record in parse_electoral_roll(file_bytes):
                yield json.dumps(record) + "\n"
        except Exception as e:
            yield json.dumps({"error": f"Parsing failed: {str(e)}"}) + "\n"

    return StreamingResponse(
        generator(),
        media_type="application/x-ndjson"
    )

if __name__ == "__main__":
    import uvicorn
    uvicorn.run("main:app", host="0.0.0.0", port=settings.PORT, reload=True)  # nosec B104
