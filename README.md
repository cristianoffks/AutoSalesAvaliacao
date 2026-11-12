# AutoSales Avaliação

Cadastro de pessoas e veículos com API em .NET 10, SQL Server e interface em Angular 22.

## Tecnologias utilizadas

C#, ASP.NET Core 10, Entity Framework Core, SQL Server 2022, Angular 22, MediatR, FluentValidation, Serilog e Swagger. Os testes usam xUnit e Moq. O Docker é opcional para executar o banco localmente.

## Pré-requisitos

- SDK do .NET 10.
- SQL Server ou Docker Desktop com contêineres Linux.
- Node.js compatível com Angular 22 e npm.
- Visual Studio 2026, caso prefira executar pela IDE.

## Configuração do banco

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

## Decisões arquiteturais

- API, aplicação e infraestrutura em projetos separados para organizar as responsabilidades. A interface Angular fica na pasta `web`.
- Um handler por operação, chamado pelo controller através do MediatR. Os repositórios concentram o acesso ao banco com Entity Framework Core.
- FluentValidation para validar as entradas. Falhas esperadas retornam `Result`, e exceções são tratadas pelo middleware. Os handlers registram avisos e erros com Serilog.
- Cada veículo pertence a uma pessoa. Marca e modelo ficam no próprio veículo, como campos de texto. CPF e placa têm índices únicos no banco.

## Melhorias futuras

Adicionar paginação, autenticação e testes de integração. Levar o filtro de ano e a associação dos resultados de pessoas e veículos para a API.
