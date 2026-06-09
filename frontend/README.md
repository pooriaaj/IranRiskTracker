# Iran Risk Tracker - Frontend (Phase 5.1)

This is a minimal React + TypeScript frontend that calls the backend dashboard summary API.

Development
- Ensure the backend API is running locally on port 5000 (default in this workspace).
- From this `frontend` folder:
  - npm install
  - npm run dev

Build
- npm run build

Notes
- The API base URL is configured via `.env` (VITE_API_BASE_URL). Default: http://localhost:5000
- If the frontend fails to reach the backend due to CORS, enable local dev CORS on the backend for the frontend origin only (http://localhost:5173).
