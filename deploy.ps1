$sourceFolder = "C:\Users\david\Documents\GitHub\pizza-ai"

$buildClientCommand = "cd `"$sourceFolder`"; docker compose -f docker-compose.yml down -v --rmi all --remove-orphans"
Invoke-Expression $buildClientCommand

$buildClientCommand = "cd `"$sourceFolder`"; docker-compose -f docker-compose.yml up -d"
Invoke-Expression $buildClientCommand