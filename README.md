Required Changes in appsettings.json
Before running the project, you must update these settings in appsettings.json to match your local environment or server:

 {
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=(localdb)\\ProjectModels;Initial Catalog=LibSys_DB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False"
  },
  "UseDapper": true // >> // Set to 'false' to use ADO.NET instead
}
