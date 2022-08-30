import requests
import json
from WorkItem import *

serverAdress = ""
userName = ""
password = ""
token = ""
repeatKey = 1312321

def openConnection(server, user, passWord) :
    global serverAdress, password, userName
    serverAdress = server
    password = passWord
    userName = user
    api_url = serverAdress + ":8085/idp/services/rest/TokenService/"
    credentials = {'credentials': { 'username' : userName, 'password': password}}
    headers = {'Content-type': 'application/json'}
    response = requests.get(api_url, data=json.dumps(credentials), headers=headers)
    results = response.json()
    global token
    token = results["token"]["value"]

def getVersion() :
    api_url = serverAdress + "/jsonapi/getversion"
    global token
    headers = {'Content-type': 'application/json', 'alfssoauthntoken' : token}
    response = requests.get(api_url, headers=headers)
    results = response.json()
    return results['version']

def readItemsFromReport(reportID) :
    startID = 0
    increment = 100
    global repeatKey
    repeatKey = repeatKey + 1
    items = []
    while(True) :
        api_url = serverAdress + "/jsonapi/getitemsbylistingreport/" + str(reportID) + "?pagesize=" + str(increment) + "&rptkey=" + str(repeatKey) + "&recno=" + str(startID)
        global token
        headers = {'Content-type': 'application/json', 'alfssoauthntoken' : token}
        data = {"fixedFields": False, "includeNotes": True}
        response = requests.post(api_url, headers=headers, json=data)
        results = response.json()
        for x in results['items']:
            item = WorkItem()
            item.ParseFromJson(x, True)
            items.append(item)
        startID = startID + increment
        if (len(results['items']) < increment) : 
            break
    return items