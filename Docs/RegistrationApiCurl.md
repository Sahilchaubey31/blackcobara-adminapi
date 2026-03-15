# Registration API — curl examples (for React frontend)

Base URL
- Replace baseUrl with your running API address. Example used below:
  - https://localhost:7271

Notes
- Steps match the 5-step UI. Use the step endpoints for interactive flows; use `/api/Registration/complete` to submit everything in one call.
- Step endpoints (2–5) require header `X-Username` (username created in step 1).
- Do NOT ship CVV or ATM PIN to production—tokenize via a PCI provider. These examples are for local/dev testing only.
- If using a self-signed dev certificate, add `-k` to curl to ignore TLS errors.

Quick conversion tips for React (fetch)
- Replace `{baseUrl}` with your API URL and convert curl JSON body to `body: JSON.stringify(payload)` and headers as shown in the examples.
- Example:


1) Step 1 — User Details (create or upsert partial registration)
- POST /api/Registration/step1/user-details

curl -X POST "{baseUrl}/api/Registration/step1/user-details" 
-H "Content-Type: application/json" 
-d '{ "username": "alice", "password": "P@ssw0rd!", "mobileNumber": "+911234567890" }'

2) Step 2 — Personal & Bank Info (requires X-Username)
- POST /api/Registration/step2/personal-bank-info

3) Step 3 — Profile Setup (profile password + DOB)
- POST /api/Registration/step3/profile-setup

4) Step 4 — Card Verification (only send test data in dev)
- POST /api/Registration/step4/card-verification

- Reminder: CVV and ATM PIN must never be persisted in plain text in production.

5) Step 5 — OTP Verification (complete registration)
- POST /api/Registration/step5/otp-verification

6) Single-call — Complete registration (one payload)
- POST /api/Registration/complete

If you want, I can:
- Provide a Postman collection or ready-made fetch/axios helper functions for integration with your React app.
- Update Swagger to include the `X-Username` header description so frontend teams see it automatically.