# AutoSales Avaliação

Cadastro de pessoas e veículos com API em .NET 10, SQL Server e interface em Angular 22.

## Pré-requisitos

- SDK do .NET 10.
- SQL Server ou Docker Desktop com contêineres Linux.
- Node.js compatível com Angular 22 e npm.
- Visual Studio 2026, caso prefira executar pela IDE.

## Configuração do banco

O arquivo `AutoSales.Api/appsettings.Local.json` contém uma configuração simplificada de exemplo. Substitua servidor, usuário e senha pelos dados do seu ambiente:

```json
{
  "ConnectionStrings": {
    "Registry": "Server=SEU_SERVIDOR;Database=AutoSales;User Id=SEU_USUARIO;Password=SUA_SENHA;TrustServerCertificate=True"
  }
}
```

Esse arquivo é usado na execução em Debug e está ignorado pelo Git. Se não estiver presente após clonar, crie-o com a estrutura acima.

### SQL Server no Docker

Se já possui um SQL Server, pode pular esta etapa. Na raiz da solução, execute no PowerShell:

```powershell
if (!(Test-Path -LiteralPath .env)) {
    $registryPassword = 'Local!9' + [Guid]::NewGuid().ToString('N')
    Set-Content -LiteralPath .env -Value "MSSQL_SA_PASSWORD=$registryPassword"
}
$registryPassword = (Get-Content -LiteralPath .env -Raw).Trim() -replace '^MSSQL_SA_PASSWORD=', ''

docker compose up -d --wait

dotnet user-secrets set "ConnectionStrings:Registry" "Server=127.0.0.1,14333;Database=AutoSales;User Id=sa;Password=$registryPassword;TrustServerCertificate=True" --project AutoSales.Api
```

A instância usa a porta `14333`. Mantenha o `.env`, pois ele contém a senha usada pelo banco. A conexão configurada pelos segredos de usuário tem preferência sobre o arquivo local em desenvolvimento.

### Criação do banco

O script está em [database/CreateSchema.sql](database/CreateSchema.sql). Abra no SQL Server Management Studio e execute o arquivo completo na instância configurada para a API. Ele cria o banco `AutoSales` e suas tabelas.

Se preferir usar a migração, execute na raiz da solução:

```powershell
dotnet tool restore
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet ef database update --project AutoSales.Infrastructure --startup-project AutoSales.Api
```

Escolha o script ou a migração. Esta versão utiliza uma migração inicial consolidada e deve ser aplicada em um banco novo.

## Execução da API

Abra `AutoSales.sln`, selecione `AutoSales.Api` como projeto de inicialização e execute com F5. Pelo terminal:

```powershell
dotnet run --project AutoSales.Api --launch-profile AutoSales.Api
```

Swagger: [http://localhost:5080/swagger](http://localhost:5080/swagger).

## Interface Angular

Com a API em execução na porta `5080`, abra outro terminal:

```powershell
cd web
npm.cmd ci
npm.cmd start
```

Acesse [http://localhost:4200](http://localhost:4200). O menu dá acesso aos cadastros e às pesquisas de pessoas e veículos.

Para gerar a compilação, execute `npm.cmd run build`. Antes de enviar o projeto em ZIP, pare o Angular com Ctrl+C e, dentro de `web`, execute:

```powershell
npm.cmd run clean
```

Isso remove as dependências, o cache e a compilação. Para rodar novamente, execute `npm.cmd ci` e `npm.cmd start`.

## Testes

Pelo Gerenciador de Testes do Visual Studio ou, na raiz da solução:

```powershell
dotnet test AutoSales.Application.Tests/AutoSales.Application.Tests.csproj
```
