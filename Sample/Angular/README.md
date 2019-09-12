# Introduction

This is demonstration of basic example of single page angular application and microservice architecture.
Angular application uses adal-angular4 to authenticate Azure Active Directory users. You can use sample users dev0001@quioutlookcom.onmicrosoft.com/HVN@Welc0meHVN@Welc0me or dev0002@quioutlookcom.onmicrosoft.com/HVN@Welc0meHVN@Welc0me to login. 
The backend side, we use simple API Gateway which is implemented by ASP.Net Core 2.2 Web API and there are two small microservices, ReportService and AuthService. 

After login, click on ***Timesheet*** menu to retrieve data from server. Here is the data flow:
angular application -> API Gateway -> ReportService (to retrieve fake report data) -> AuthService (to fullfil user name and email for report data).

The database access and ***Register*** feature to demo auto-register user in Azure Active Directory is comming soon.

The commnunication between microservices is asynchronous HTTP and is handled by https://github.com/quinvit/microcore library. The library covers network call, serialization/deserialization, contract mapping. We only use microservice by using DI to inject service interface to constructor as in-process service injection.

# Contents

1. time-tracker-ui folder contains sample code of angular application.
2. TimeTrackerAPI folder contains sample code of API Gateway
3. Services folder contains sample code of two simple microservices.

The API Gateway and microservices are deployed to Azure Web App (Linux docker container mode) in order we can easily scale out or scale up any service. On the other hand, hosting microservice in Azure Web App is much cheaper than on VM or AKS.

![Sample](sample.png)

[Online demo](https://quinvit.z23.web.core.windows.net)
