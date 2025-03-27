$sourceFolder = "C:\Users\david\Documents\GitHub\pizza-ai"
$composeFile = "$sourceFolder\docker-compose.yml"
$secretsFolder = "$sourceFolder\secrets" # Dossier pour les secrets
$tempFile = "$secretsFolder\nonce.txt"   # Fichier temporaire pour le nonce

# Générer un nonce cryptographiquement sûr
$randomBytes = New-Object byte[] 16
$randomNumberProvider = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$randomNumberProvider.GetBytes($randomBytes)
$cspNonce = [Convert]::ToBase64String($randomBytes)

# Stocker le nonce dans un fichier temporaire
$cspNonce | Out-File -FilePath $tempFile -Encoding UTF8

# Définir la variable d'environnement dans l'environnement PowerShell
$env:CSP_NONCE = $cspNonce

# Construire la commande docker compose
$buildClientCommand = "cd `"$sourceFolder`"; docker compose -f `"$composeFile`" down -v --rmi all --remove-orphans"
Invoke-Expression $buildClientCommand

# Lancer docker compose
$buildClientCommand = "cd `"$sourceFolder`"; docker compose -f `"$composeFile`" up -d"
Invoke-Expression $buildClientCommand

Write-Host "Nonce généré, stocké dans $tempFile et utilisé: $cspNonce"
