# TB_Back

### First launch

1. Open 'Dev Team' channel in MS Teams -> 'Files' Tab - > Find 'appsettings.Development.json' file. -> Copy it's content to secrets.json (User Secrets) or to your local "appsettings.Development.json" file.
1. If you changed the port/protocol to https for frontend you need to update the "Cors"--"AllowedOrigins" key in secrets.json (appsettings.Development.json) like:

```json
  "Cors": {
    "AllowedOrigins": [ "https://localhost:4173" ]
  },
```

3. Ask somebody from Dev Team to create a user for you in QA/Dev environment.
1. Copy QA/Dev database to your local SQL server. You may do it using 'Export Data-tier Application' or 'Generate scripts' (don't forget to turn on 'Types of data to script' = 'schema and data' in Advanced settings of 'generate scripts'). Update settings to use your local DB.
1. Launch project using port 44368 and https (otherwise auth in swagger won't work, because the port doesn't match the allowed one in Azure)
1. Confirm that the url of opened page (backend API url) matches the port of 'VITE_API_BASE_URI' in '.env.local' in frontend project (example: VITE_API_BASE_URI=https://localhost:44368/api)

