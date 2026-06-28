// ============================================================
//  CarCare — environment configuration
//  Change PRODUCTION_API to switch between local and production.
//
//  LOCAL DEV  : leave PRODUCTION_API as empty string ""
//               → API calls go to http://127.0.0.1:8000  (start.bat)
//
//  PRODUCTION : Azure App Service URL
// ============================================================

var PRODUCTION_API = "https://carcare-il.azurewebsites.net";   // ← Azure App Service
