import json

class WorkItem :
    #Fixed fields
    ID = 0
    Title = ""
    Description = ""
    Submitter = ""
    Owner = ""
    State = ""
    Type = ""
    Project = ""
    SubmitDate = ""
    Link = ""
    IsActive = False

    #Custom fields
    Severity = ""
    SoftwareEngineer = ""
    QAEngineer = ""
    CloseDate = ""

    def ParseFromJson(self, json, parseCustomFields) :
        self.ID = int(json['id']['itemId'])
        self.Link = json['id']['url']
        self.Title = json['fields']['TITLE']['value']
        self.Submitter = json['fields']['SUBMITTER']['name']
        self.Description = json['fields']['DESCRIPTION']['value']
        self.State = json['fields']['STATE']['value']
        self.IsActive = json['fields']['ACTIVEINACTIVE']['name'] == 'Inactive'
        self.Type = json['fields']['ISSUETYPE']['name']
        self.Project = json['fields']['PROJECTID']['name']
        self.Owner = json['fields']['OWNER']['name']
        try :
            self.SubmitDate = json['fields']['SUBMITDATE']['svalue']
        except :
            self.SubmitDate = ""
        
        if (parseCustomFields) :
            self.Severity = json['fields']['SEVERITY']['name']
            self.SoftwareEngineer = json['fields']['SWE']['name']
            self.QAEngineer = json['fields']['SQA']['name']
            try :
                self.CloseDate = json['fields']['CLOSEDATE']['svalue']
            except :
                self.CloseDate = ""
