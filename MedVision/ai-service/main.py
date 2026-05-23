from fastapi import FastAPI, UploadFile, File
from fastapi.middleware.cors import CORSMiddleware
import random

app = FastAPI(title="MedVision AI Service")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

@app.get("/")
def health_check():
    return {
        "message": "MedVision AI Service is running"
    }

@app.post("/predict")
async def predict(file: UploadFile = File(...)):
    probability = round(random.uniform(50, 99), 2)

    if probability > 70:
        result_class = "Pathology detected"
        conclusion = "На знімку виявлено ознаки можливої патології легень."
    else:
        result_class = "Normal"
        conclusion = "Ознак патології легень не виявлено."

    return {
        "resultClass": result_class,
        "probability": probability,
        "conclusion": conclusion
    }