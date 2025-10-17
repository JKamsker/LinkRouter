 = "https://go.microsoft.com/fwlink/?linkid=2120843"
 = Join-Path :TEMP "winsdk19041.exe"
Invoke-WebRequest -Uri  -OutFile 
Start-Process -FilePath  -ArgumentList '/quiet','/norestart' -Wait
