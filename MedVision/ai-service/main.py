from fastapi import FastAPI, UploadFile, File
from fastapi.middleware.cors import CORSMiddleware
from fastapi.staticfiles import StaticFiles
from fastapi.responses import FileResponse
from ultralytics import YOLO

import os
import uuid
import shutil
import cv2
import pydicom
import numpy as np

app = FastAPI(title="MedVision AI Service")


# =========================
# CORS
# =========================

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


# =========================
# LOAD YOLO MODEL
# =========================

model = YOLO("models/medvision_yolo.pt")


CONFIDENCE_THRESHOLD = 0.5
DICOM_CONFIDENCE_THRESHOLD = 0.3

PATHOLOGY_INFO = {
    "Cardiomegaly": "Збільшення серця. Може свідчити про серцеву недостатність.",
    "Aortic enlargement": "Розширення аорти. Потребує додаткового обстеження.",
    "Pleural effusion": "Наявність рідини у плевральній порожнині.",
    "Nodule/Mass": "Можливе новоутворення в легенях.",
    "Pneumothorax": "Наявність повітря у плевральній порожнині.",
    "Lung Opacity": "Виявлено затемнення легеневої тканини.",
    "Consolidation": "Можлива пневмонія або запальний процес.",
    "Infiltration": "Інфільтративні зміни у легенях.",
    "Pulmonary fibrosis": "Фіброзні зміни легеневої тканини.",
    "Pleural thickening": "Потовщення плеври.",
    "Calcification": "Кальциновані ділянки у тканинах.",
    "Atelectasis": "Часткове спадіння легені.",
    "ILD": "Інтерстиціальне захворювання легень.",
    "Other lesion": "Інші патологічні зміни.",
}


# =========================
# CREATE OUTPUT FOLDER
# =========================

OUTPUT_FOLDER = "predictions"

os.makedirs(OUTPUT_FOLDER, exist_ok=True)

app.mount(
    "/predictions",
    StaticFiles(directory=OUTPUT_FOLDER),
    name="predictions",
)


# =========================
# HELPERS
# =========================


def convert_dicom_to_png(file_path: str) -> str:

    dicom = pydicom.dcmread(file_path)

    image = dicom.pixel_array.astype(np.float32)

    # =========================
    # NORMALIZATION
    # =========================

    image = image - np.min(image)

    image = image / np.max(image)

    image = (image * 255).astype(np.uint8)

    # =========================
    # CLAHE CONTRAST ENHANCEMENT
    # =========================

    clahe = cv2.createCLAHE(clipLimit=2.0, tileGridSize=(8, 8))

    image = clahe.apply(image)

    # =========================
    # GAUSSIAN DENOISING
    # =========================

    image = cv2.GaussianBlur(image, (3, 3), 0)

    converted_path = os.path.splitext(file_path)[0] + ".png"

    cv2.imwrite(converted_path, image)

    return converted_path


# =========================
# ROUTES
# =========================


@app.get("/")
def health_check():
    return {"message": "MedVision YOLO AI Service is running"}


@app.post("/preview")
async def preview(file: UploadFile = File(...)):

    extension = os.path.splitext(file.filename)[1].lower()

    unique_filename = f"preview_{uuid.uuid4()}{extension}"

    temp_path = os.path.join(OUTPUT_FOLDER, unique_filename)

    with open(temp_path, "wb") as buffer:
        shutil.copyfileobj(file.file, buffer)

    if extension in [".dcm", ".dicom"]:

        png_path = convert_dicom_to_png(temp_path)

        return FileResponse(png_path, media_type="image/png")

    return FileResponse(temp_path)


@app.post("/predict")
async def predict(file: UploadFile = File(...)):
    extension = os.path.splitext(file.filename)[1].lower()

    unique_filename = f"{uuid.uuid4()}{extension}"

    temp_path = os.path.join(OUTPUT_FOLDER, unique_filename)

    with open(temp_path, "wb") as buffer:
        shutil.copyfileobj(file.file, buffer)

    is_dicom = extension in [".dcm", ".dicom"]

    if is_dicom:
        temp_path = convert_dicom_to_png(temp_path)

        preview_path = None

        preview_path = f"/predictions/{os.path.basename(temp_path)}"

    current_threshold = DICOM_CONFIDENCE_THRESHOLD if is_dicom else CONFIDENCE_THRESHOLD

    results = model(temp_path, conf=current_threshold)

    result = results[0]

    plotted_image = result.plot()

    predicted_filename = f"pred_{uuid.uuid4()}.jpg"

    predicted_path = os.path.join(OUTPUT_FOLDER, predicted_filename)

    cv2.imwrite(predicted_path, plotted_image)

    detections = []

    boxes = result.boxes

    if boxes is not None:
        for box in boxes:
            class_id = int(box.cls[0])
            confidence = float(box.conf[0])

            if confidence < current_threshold:
                continue

            class_name = model.names[class_id]

            detections.append(
                {
                    "className": class_name,
                    "confidence": round(confidence * 100, 2),
                    "description": PATHOLOGY_INFO.get(
                        class_name,
                        "Опис патології відсутній.",
                    ),
                }
            )

    return {
        "resultClass": "Abnormal" if len(detections) > 0 else "Normal",
        "probability": max(
            [d["confidence"] for d in detections],
            default=99.0,
        ),
        "conclusion": (
            "Виявлено патології." if len(detections) > 0 else "Патологій не виявлено."
        ),
        "detections": detections,
        "heatmapPath": f"/predictions/{predicted_filename}",
        "previewPath": f"/predictions/{os.path.basename(temp_path)}",
        "previewPath": preview_path,
    }
