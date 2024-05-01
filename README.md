## About the project

Web-service which is used in API_Automation_CSharp project  is storage of users and their information:
name, age, sex and zip code.
Application provides: 
•	information about all stored users; 
•	possibility to create, update, delete users; 
•	information about available zip codes; 
•	possibility to add new zip codes. 

To run web-service you need to execute the following in command line: 
docker pull coherentsolutions/rest-training:1.0 
docker run -d -p <your port>:8082 coherentsolutions/rest-training:1.0 
After that open swagger documentation in browser: http://localhost:<your port>/swagger-ui/ 

## Installation

To install the project, follow these steps:
1. Clone the repository using Git: https://github.com/AKasachova/API_Automation_CSharp
2. Open the project in Visual Studio.
3. Build and run the project.

## Usage

To run tests it is needed to set up Client with appropriate parameters in Testsettings.runsettings file:
1. Base URL: "http://localhost:<your port>"
2. Username for authentication: "0oa157tvtugfFXEhU4x7"
3. Password for authentication: "X7eBCXqlFC7x-mjxG5H91IRv_Bqe1oq7ZwXNA8aq"

## Dependencies

The project depends on the following components:

Open NuGet Manager and add required dependencies for this project. Dependencies you will need:
-	Nunit 3+ version (https://github.com/nunit/nunit/releases )
-	Nunit TestAdapter (https://github.com/nunit/nunit3-vs-adapter/releases  )
-	Microsoft.NET.Test.Sdk (https://github.com/microsoft/vstest/ )
-	RestSharp (https://github.com/restsharp/RestSharp )

## Allure report
To generate the report execute in CMD: allure serve allure-results

## Sucessfull workflow run:
https://github.com/AKasachova/API_Automation_CSharp/actions/runs/8866694073
