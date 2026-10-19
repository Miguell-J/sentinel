# Primeiros passos

## Pré-requisitos

Docker + Compose v2, Git e (opcionalmente) .NET SDK 10 para trabalhar fora do Docker.

## Subir em ambiente local

1. Clone o repositório e copie .env.example para .env.
2. Defina dois segredos **diferentes**, aleatórios e longos em POSTGRES_PASSWORD e SENTINEL_INGEST_TOKEN.
3. Execute docker compose up --build -d.
4. Acesse http://localhost:8080/health/live e http://localhost:8080/health/ready.

O arquivo .env não é versionado. A API é exposta apenas em 127.0.0.1 por padrão.

## Criar um incidente de exemplo

No Bash:

~~~bash
curl -X POST http://localhost:8080/api/v1/incidents \
  -H "Authorization: Bearer SEU_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"source":"manual","repository":"sandbox/api","fingerprint":"case-001","title":"Exemplo de incidente"}'
~~~

No PowerShell:

~~~powershell
$headers = @{ Authorization = "Bearer SEU_TOKEN" }
$payload = @{ source = 'manual'; repository = 'sandbox/api'; fingerprint = 'case-001'; title = 'Exemplo' } | ConvertTo-Json
Invoke-RestMethod -Method Post -Uri 'http://localhost:8080/api/v1/incidents' -Headers $headers -ContentType 'application/json' -Body $payload
~~~

O resultado retorna um ID e o status Detected. Enviar o mesmo source/repository/fingerprint não cria um segundo registro.

## Verificações locais

~~~bash
dotnet restore Sentinel.slnx
dotnet build Sentinel.slnx -c Release
dotnet run --project tests/Sentinel.Domain.Tests/Sentinel.Domain.Tests.csproj -c Release
~~~

## Restrições

O webhook do WhatsApp é intencionalmente inerte mesmo após autenticação. As integrações Claudear, Codex, Cordis e CI/CD de produção ainda não estão conectadas.

O script SQL em db/init é aplicado somente em um volume PostgreSQL **novo**. Nunca apague volumes com dados importantes; antes de evoluir o esquema, crie migrations versionadas.
