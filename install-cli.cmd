@ECHO OFF
PUSHD %~dp0
CALL pack.cmd
dotnet tool install -g "JEFTDotNet.CLI" --add-source "%~dp0bin\Release\JEFTDotNet.CLI\\"
POPD
