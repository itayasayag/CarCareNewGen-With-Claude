// ============================================================
//  CarCare — environment configuration
//  Change PRODUCTION_API to switch between local and production.
//
//  LOCAL DEV  : leave PRODUCTION_API as empty string ""
//               → API calls go to http://127.0.0.1:8000  (start.bat)
//
//  PRODUCTION : set PRODUCTION_API to your Railway backend URL
//               e.g. "https://carcare-production.up.railway.app"
//               → all API calls go there regardless of where
//                 the frontend is hosted
// ============================================================

var PRODUCTION_API = "https://carcarenewgen-with-claude-production.up.railway.app";   // ← change this when going live
