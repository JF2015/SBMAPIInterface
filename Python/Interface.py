import requests
import json

serverAdress = "http://servername"
userName = "user"
password = "password"
token = ""

def openConnection() :
    api_url = serverAdress + ":8085/idp/services/rest/TokenService/"
    credentials = {'credentials': { 'username' : userName, 'password': password}}
    headers = {'Content-type': 'application/json'}
    response = requests.get(api_url, data=json.dumps(credentials), headers=headers)
    results = response.json()
    print(results["status"])
    global token
    token = results["token"]["value"]
    print(token)

def getVersion() :
    api_url = serverAdress + "/jsonapi/getversion"
    global token
    headers = {'Content-type': 'application/json', 'alfssoauthntoken' : token}
    response = requests.get(api_url, headers=headers)
    results = response.json()
    return results['version']

openConnection()
version = getVersion()
print(version)