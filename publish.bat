rmdir /S /Q "publish"
md "publish"
xcopy /E /Y /C /Q ".\resources" ".\publish"
dotnet publish "CrossoutLogViewer.Updater\CrossoutLogViewer.Updater.csproj" -c Release -o "publish" /p:Platform=x64 -r win-x64 --no-self-contained
::dotnet publish "CrossoutLogViewer.Updater\CrossoutLogViewer.Updater.csproj" -c Release -o "publish" /p:Platform=x64 -r win-x64 -p:PublishSingleFile=true --no-self-contained
dotnet publish "CrossoutLogViewer.GUI\CrossoutLogViewer.GUI.csproj" -c Release -o "publish" /p:Platform=x64 -r win-x64 --no-self-contained