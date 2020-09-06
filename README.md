# Bard.Db

[![Join the chat at https://gitter.im/Bard-NET/Bard.Db](https://badges.gitter.im/Bard-NET/Bard.Db.svg)](https://gitter.im/Bard-NET/Bard.Db?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

Bard.Db is built on top of the Docker.Net project which does most of the hard work. Bard.Db expects that Docker is installed without it it's not going to work.
The idea behind bard.Db is to make it easy to spin up an instance of a database from code especially when we are running integration tests.

## Installation

Before you start, you will need to add a reference to Bard.Db.dll in your project. 
The simplest way to do this is to use either the NuGet package manager, or the dotnet CLI.
Using the NuGet package manager console within Visual Studio run the following command:
Install-Package Bard.db

```
Install-Package Bard.db
```

Or using the .net core CLI from a terminal window:
```
dotnet add package Bard.db
```
## Microsoft SQL

### Creating a SQL Server instance.

Create a new instance of Bard.Db.MsSqlDatabase passing in the database name,  SA Password, port number,  tag version.
```c#
var db = new MsSqlDatabase(
           databaseName: "BardDB_SQL_2017",
           saPassword: "Password1",
           portNumber: "1066",
           tagName: "2017-latest");
```

### Starting up the Database

```c#
var result = db.StartDatabase();
```

This may take a little time the first time as Docker needs to download the image in the background.

### Stopping the Database

To stop the database simply call.

``` c#
db.StopDatabase();
```

## PostgreSQL
``` c#
var db = new PostgresDatabase(
           databaseName: "PostgreSQL_latest",
           postgresUser: "Db_user",
           password: "Password1");

var result = db.StartDatabase();
```
