import requests
import json
import os

API_VERSION = "2020-06-30"
SEARCH_SERVICE_NAME = os.environ.get('SEARCH_SERVICE_NAME')
ADMIN_KEY = os.environ.get('ADMIN_KEY_COGNITIVE')

with open("./global-search/configs/indexSchema.json", "r") as data:
  datadict = json.load(data)
  for item in datadict:
    INDEX_NAME = item["name"]
    print(INDEX_NAME)
    URL = f"https://{SEARCH_SERVICE_NAME}.search.windows.net/indexes/{INDEX_NAME}?api-version={API_VERSION}&allowIndexDowntime=true"
    REQUEST_BODY = json.dumps(item)

    headers = {
        "Content-Type": "application/json",
        "api-key": ADMIN_KEY
    }
    response = requests.put(URL, headers=headers, data=REQUEST_BODY)
    print(response.status_code)
    print(response._content)
    if response.status_code > 204:
      raise Exception("Failed to update global search index.")
