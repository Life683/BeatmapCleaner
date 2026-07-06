@ECHO OFF
call dotnet publish -c Release --runtime win-x64 --self-contained 
call dotnet publish -c Release --runtime linux-x64 --self-contained 
call dotnet publish -c Release --runtime osx-x64 --self-contained 
call dotnet publish -c Release --runtime osx-arm64 --self-contained 

mkdir BuildOutput

echo f | xcopy /Y "BeatmapExporterGUI.Desktop\bin\Release\net10.0\win-x64\publish\BeatmapExporterGUI.Desktop.exe" "BuildOutput\BeatmapCleaner.exe"
echo f | xcopy /Y "BeatmapExporterGUI.Desktop\bin\Release\net10.0\osx-x64\publish\BeatmapExporterGUI.Desktop" "BuildOutput\x86-BeatmapCleaner.app"
echo f | xcopy /Y "BeatmapExporterGUI.Desktop\bin\Release\net10.0\osx-arm64\publish\BeatmapExporterGUI.Desktop" "BuildOutput\arm64-BeatmapCleaner.app"
echo f | xcopy /Y "BeatmapExporterGUI.Desktop\bin\Release\net10.0\linux-x64\publish\BeatmapExporterGUI.Desktop" "BuildOutput\linux-BeatmapCleaner"

cd BuildOutput
tar -avcf "mac-x86-BeatmapCleaner.zip" "x86-BeatmapCleaner.app" --transform 's/x86-//'
tar -avcf "mac-arm64-BeatmapCleaner.zip" "arm64-BeatmapCleaner.app" --transform 's/arm64-//'
del x86-BeatmapCleaner.app
del arm64-BeatmapCleaner.app

pause