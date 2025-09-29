# WindowsGameAutomationTools
A collection of tools to assist automating playing games on Windows.  Screen Scraping, Image Detection, Input Simulation, File/Folder Manipulation, Guassian Blurring, etc.  Currently built on .NET 4.7.2 as it's very Windows-specific.

# Nuget Reminder
To publish:
Update .nuspec file with new version
Build Solution in Release Mode
From command line: nuget pack -p Configuration="Release"
Get API Key (https://www.nuget.org/account/apikeys)
From package manager console: dotnet nuget push WindowsGameAutomationTools.1.0.1.nupkg --source https://api.nuget.org/v3/index.json --api-key