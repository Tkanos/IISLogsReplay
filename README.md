# IISLogsReplay

I want to create a simple tool that takes IISLogs, Parse it, and then Resend (replay) the requests.
In order to Load Test my servers.

I won't do a big generic project, only a simple project that fits my needs, but if you want to fork it, or even ameliorate it, you're very welcome.

# NOT YET FINISHED, i know i should have done it on a branch but ...

# Application Help

IISLogsReplayConsole.exe -p path -d delimiter -pn pathNb -qsn queryStringNb -vn verbNr -s server [-ft fileType] [-bl beginLine] [-uan userAgentNb] [-H headers] [-C cookies] [-mr matchRequest] [-mp modifyPattern] [-r replacement] 


## Mandatory Parameters 

-p  	 : path where is located the iislogs file/directory

-d  	 : delimiter

-pn 	 : int that locate on each line of iislog the uri-stem

-qsn	 : int that locate on each line of iislog the uri-query

-vn 	 : int that locate on each line of iislog the method (verb)

-s  	 : base uri address

## Optional Parameters **

-ft 	 : if in -p you inform the directory path, ft is needed to inform the filetype (example csv)

-bl 	 : int that tell to the program in which line we begin

-uan	 : int that locate on each line of iislog the user-agent

-H  	 : to specify others (http)Headers

-C  	 : to specify (http)Cookies

-mr 	 : to specify a regexp to execute only the request that match -mr

-mp 	 : to specify a regexp pattern for Replace somthing on path or queryString

-r  	 : if you have specified -mp, -r is to specify by what you want to replace your -mp


##  Example **

```
IISLogsReplayConsole.exe -p "D:\IssLogs\myIIslog.log" -d ' ' -pn 3 -qsn 4 -vn 9 -s http:\mybetaserver.com
```
```
IISLogsReplayConsole.exe -p "D:\IssLogs" -d ' ' -pn 3 -qsn 4 -vn 9 -s http:\mybetaserver.com -ft .log -bl 4
```
```
IISLogsReplayConsole.exe -p "D:\IssLogs" -d ' ' -pn 3 -qsn 4 -vn 9 -s http:\mybetaserver.com -ft .log -bl 4 -uan 2 -H "HeaderName: HeaderValue" -C "cookieName1: CookieValue1; CookieName2:CookieValue2" -mr "v1" -mp "v1" -r "v2" 
```

