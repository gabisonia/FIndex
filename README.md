# FIndex

FIndex is a face recognition and search system powered by embeddings, Qdrant vector database, and a lightweight REST API written in .NET 8. It allows you to embed faces from images, store their vectors, and search visually similar faces efficiently.

## Components

### Embedding Service (Python)

- A FastAPI-based microservice that takes an image and returns a 512-dimensional facial embedding.
- Uses InsightFace under the hood.

### Qdrant (Vector DB)

- High-performance vector search engine for similarity search.
- Stores all embedded facial vectors.

### FIndex API (C# / ASP.NET Core)

- Accepts image uploads via REST.
- Sends them to the embedding service, gets vector back.
- Searches similar vectors in Qdrant and returns matches with confidence scores.

### FIndex Console (C#)

- Console app to upload faces and create database.

## Run

```bash
docker-compose up --build
```

## Ports and URLs

This project consists of three main services: the Python-based embedding service, the Qdrant vector database, and the .NET API. Below are the details for accessing each service:

---

| Service           | Host URL                |
| ----------------- | ----------------------- |
| Embedding Service | `http://localhost:8000` |
| Qdrant            | `http://localhost:6333` |
| FIndex API        | `http://localhost:5181` |
