@echo off

dotnet new tool-manifest --force
dotnet tool install inedo.extensionpackager

cd java\InedoExtension
dotnet inedoxpack pack . C:\LocalDev\BuildMaster\Extensions\java.upack --build=Debug -o
cd ..\..