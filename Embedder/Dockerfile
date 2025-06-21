FROM python:3.10-slim

WORKDIR /app

# Add required system packages (build tools + OpenCV deps)
RUN apt-get update && apt-get install -y \
    build-essential \
    g++ \
    libgl1 \
    libglib2.0-0 \
    && rm -rf /var/lib/apt/lists/*

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY . .

CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8000"]