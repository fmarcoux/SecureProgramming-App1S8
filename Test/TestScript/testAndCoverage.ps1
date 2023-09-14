#!/bin/sh
echo "Pour executer correctement le script, svp le faire à partie du répertoire Test"
rm -r '.\TestResults\*'
dotnet test --collect:"XPlat Code Coverage"
~\.nuget\packages\reportgenerator\5.1.25\tools\net6.0\ReportGenerator.exe -reports:".\TestResults\*\coverage.cobertura.xml" -targetdir:"./TestCoverage" -reporttypes:Html