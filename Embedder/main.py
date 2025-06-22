from fastapi import FastAPI, File, UploadFile, HTTPException
from fastapi.responses import JSONResponse
import numpy as np
import cv2
import insightface
from insightface.app import FaceAnalysis

app = FastAPI()

model = FaceAnalysis(name='buffalo_l', providers=['CPUExecutionProvider'])
model.prepare(ctx_id=-1)

@app.post("/embed")
async def embed_image(file: UploadFile = File(...)):
    try:
        img_bytes = await file.read()
        np_img = np.frombuffer(img_bytes, np.uint8)
        img = cv2.imdecode(np_img, cv2.IMREAD_COLOR)

        if img is None:
            raise HTTPException(status_code=400, detail="Invalid image")

        faces = model.get(img)
        if not faces:
            raise HTTPException(status_code=404, detail="No face found")

        emb = faces[0].embedding / np.linalg.norm(faces[0].embedding)
        return JSONResponse(content={"embedding": emb.tolist()})
    
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))