# Wide82.DbLoggingProvider.Pomelo

`Wide82.DbLoggingProvider.Pomelo` provide to extend .NET Core logging with easy and simply. All application's logs will store into chosen MySQL Schema.
You can also use Wide Logs Analyzer to check, read and analyze issues of your applications in centralized mode.

## Compatibility

### Packages

* [Pomelo.EntityFrameworkCore.MySql](https://www.nuget.org/packages/Pomelo.EntityFrameworkCore.MySql/)

### Supported Database Servers and Versions

`Wide82.DbLoggingProvider.Pomelo` is tested against all actively maintained versions of `MySQL` and `MariaDB`. Older versions (e.g. MySQL 5.6) and other server implementations (e.g. Amazon Aurora) are usually compatible to a high degree as well, but are not tested as part of our CI.

Officially supported versions are:

- MySQL 8.0
- MySQL 5.7
- MariaDB 10.9
- MariaDB 10.8
- MariaDB 10.7
- MariaDB 10.6
- MariaDB 10.5
- MariaDB 10.4
- MariaDB 10.3

## Getting Started

### 1. Project Configuration

Ensure that your `.csproj` file contains the following reference:

```xml
<PackageReference Include="Wide82.DbLoggingProvider.Pomelo" Version="6.0.1" />
```
### 2. Configure Logging

Add `Wide82.DbLoggingProvider.Pomelo` to logging configuration in your the `Program.cs` file of your ASP.NET Core project:

```c#
public class Program 
{
    public static IWebHost BuildWebHost(string[] args, IConfiguration conf) {
        return WebHost.CreateDefaultBuilder()
            .ConfigureLogging(builder => 
                builder.AddDbLogger(x => {
                    conf.GetSection("Logging").GetSection("Database").GetSection("Options").Bind(x); 
                })
            ).Build();
    }
}
```
### 3. Services Configuration

Add `Wide82.DbLoggingProvider.Pomelo` to the services configuration in your the `Startup.cs` file of your ASP.NET Core project:

```c#
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Other service configuration
        
        // Add initializtion of DbLogger Options by reading Logging section into appsetting.json
        services.Configure<DbLoggerOptions>(Configuration.GetSection("Logging").GetSection("Database").GetSection("Options"));
        
    }
}
```

### 4. Application configuration

Add `Wide82.DbLoggingProvider.Pomelo` to the application configuration in your the `appsetting.json` file of your ASP.NET Core project:

```json
{
    "Logging": {
        // your logging configuration
        
        "Database": {
            "Options": {
                "ApplicationName": "APP_NAME",
                "ConnectionString": "server=localhost;port=3306; Database=db_schema; uid=user; pwd=password;SslMode=none",
                "AsyncWrite": true,
                "LogPath": "/var/log/appName"
            },
            "LogLevel": {
                "Default": "Information",
                "Microsoft.AspNetCore.Mvc.Razor.Internal": "Warning", 
                "Microsoft.AspNetCore.Mvc.Razor.Razor": "Warning",
                "Microsoft.AspNetCore.Mvc.Razor": "Warning",
                "Microsoft.EntityFrameworkCore.Database.Command": "Information",
                "TransitPointManager.ServiceLayer": "Information"
            }
        }
    }
}
```
Configuration parameters:

- ApplicationName: name of your application or component
- ConnectionString: parameters for connect to MySQL Schema
- AsyncWrite: if true, the log entry will be written asynchronously, otherwise it is written immediately into the database
- LogPath: path where will be written all log entries that had a problem during insert into database

### 5. Create Table and Procedure into DB Schema

```sql
CREATE TABLE `Log` (
  `LogID` varchar(45) NOT NULL,
  `EventID` int(11) DEFAULT NULL,
  `Priority` int(11) NOT NULL,
  `Severity` varchar(32) NOT NULL,
  `Title` varchar(256) NOT NULL,
  `Timestamp` datetime NOT NULL,
  `MachineName` varchar(50) NOT NULL,
  `AppDomainName` varchar(512) NOT NULL,
  `ProcessID` varchar(256) NOT NULL,
  `ProcessName` varchar(512) NOT NULL,
  `ThreadName` varchar(512) DEFAULT NULL,
  `Win32ThreadId` varchar(128) DEFAULT NULL,
  `Message` text,
  PRIMARY KEY (`LogID`),
  KEY `IDX_Timestamp` (`Timestamp`),
  KEY `IDX_MachineName` (`MachineName`),
  KEY `IDX_Title` (`Title`),
  KEY `IDX_Severity` (`Title`,`MachineName`,`Timestamp`)
) ENGINE=MyISAM;

DELIMITER $$
CREATE PROCEDURE `WriteLog`( 
	IN LogID varchar(45),
	IN EventID int,
	IN Priority int,
	IN Severity varchar(32),
	IN Title varchar(256),
	IN Timestamp datetime,
	IN MachineName varchar(32),
	IN AppDomainName varchar(512),
	IN ProcessID varchar(256),
	IN ProcessName varchar(512),
	IN ThreadName varchar(512), 
	IN Win32ThreadId varchar(128),
	IN Message text )
BEGIN

INSERT INTO `Log` (
		LogID,
		EventID,
		Priority,
		Severity,
		Title,
		Timestamp,
		MachineName,
		AppDomainName,
		ProcessID,
		ProcessName,
		ThreadName,
		Win32ThreadId,
		Message
	)
	VALUES (
		LogID,
		EventID,
		Priority,
		Severity,
		Title,
		Timestamp,
		MachineName,
		AppDomainName,
		ProcessID,
		ProcessName,
		ThreadName,
		Win32ThreadId,
		Message );
END$$
DELIMITER ;


```


## License

[MIT](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql/blob/master/LICENSE)
