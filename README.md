# Trust-Us
This repository is for a school project and is not used in the real world.
## Getting started
Get source with git by typing command ```git clone https://github.com/blackout1471/trust-us-school.git```

## Requirements
* .Net 6
* Asp net core
* Mssql
* Visual studio

## Optional requirements
* Elk stack (can be dockerised) https://github.com/deviantony/docker-elk - This can be used to test the logging mechanism. Can also be seen i console.
* Docker (integration tests)
* K6 (stress tests)

## Opening projects
Each project has it's own .sln file for visual studio except the javascript library.

The projects can be located as such

    .
    ├── Source
    │   ├── IdentityApi - The rest api folder
    │   │   ├── IdentityApi.sln
    │   ├── MessageService - The message service library used to send email etc.
    │   │   ├── MessageService.sln
    │   ├── Webclient - The prototype frontend.
    │   │   ├── TrustLogin - The javascript communication library folder.
    │   ├── Stress-Test-Web - Stress test folder
    │   │   ├── *.js - K6 tests, base url etc... can also be found here


## Setting up local environment
1. Pull repository.
2. Install mssql.
3. Run db scripts except DbClean.sql in the desired mssql instance.
4. (optional) Install elk.
5. Edit appsettings.json or appsettings.development.json in rest-api to point at the correct services.
6. Run IdentityApi with desired profile.
7. Edit javascript library environment file to point at restapi service.
8. Open webclient login.html or register.html

(*) Mail server has to be configures in appsettings.json.
Docker image with mailhog can be set up when testing.

## Finding environment settings

    .
    ├── Source
    │   ├── IdentityApi - The rest api folder
    │   │   ├── IdentityApi
    │   │   │   ├── appsettings.json
    │   │   │   ├── appsettings.development.json
    │   ├── Webclient - The prototype frontend.
    │   │   ├── TrustLogin - The javascript communication library folder.
    │   │   │   ├── Environment.js

## Tests
The tests are contained in each solution respectively.
For Rest api there is unit test and integration test projects.
The message service also has it's own unit test project.

To be able to run the integrations tests, docker is required. This is because the tests themselves will create the necessary containers with database and etc...

## Stress tests
To use stress tests, K6 is required. The docker image can be pulled by command `docker pull grafana/k6` in cmd.

1. Run `Run-login-load.bat`

## Documents
The folder called `Docs` is used to contain all the documentation for the project such as *class diagrams*, *flowcharts*, *SD diagram*, *ERD diagrams* and images used in the report.

## Scripts
All the database scripts are in the folder called `Scripts` these can be used to create the database with the necessary tables, stored procedure etc...
