import os

class Settings:
    # Shared secret key for communication between internal services
    INTERNAL_API_KEY: str = os.getenv("INTERNAL_API_KEY", "matdansathi-secure-internal-token-2026")
    PORT: int = int(os.getenv("PORT", "8000"))

settings = Settings()
