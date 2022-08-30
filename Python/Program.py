from Interface import *

reportID = 1076
serverAdress = "servername"
userName = "user"
password = "password"

openConnection(serverAdress, userName, password)
version = getVersion()
print(version)
items = readItemsFromReport(1076)
for x in items :
    print(x.Title)