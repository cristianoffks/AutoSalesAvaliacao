# AutoSales

API REST para cadastro de pessoas e veículos, com C#, ASP.NET Core 10, Entity Framework Core, SQL Server 2022 e Swagger.

## Execução local

Pré-requisitos: SDK do .NET 10, Docker Desktop com contêineres Linux e, para abrir a solução no ambiente de desenvolvimento, Visual Studio 2026 com a carga de trabalho ASP.NET e desenvolvimento Web.

Na raiz da solução, rode no PowerShell:

```powershell
if (!(Test-Path -LiteralPath .env)) {
    $registryPassword = 'Local!9' + [Guid]::NewGuid().ToString('N')
    Set-Content -LiteralPath .env -Value "MSSQL_SA_PASSWORD=$registryPassword"
}
$registryPassword = (Get-Content -LiteralPath .env -Raw).Trim() -replace '^MSSQL_SA_PASSWORD=', ''
docker compose up -d --wait
dotnet user-secrets set "ConnectionStrings:Registry" "Server=127.0.0.1,14333;Database=AutoSales;User Id=sa;Password=$registryPassword;TrustServerCertificate=True" --project AutoSales.Api
dotnet tool restore
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet ef database update --project AutoSales.Infrastructure --startup-project AutoSales.Api
```

O SQL Server usa a porta local `14333` e guarda os dados em um volume do Docker. Mantenha o mesmo `.env` enquanto usar esse volume. A API lê a conexão dos segredos de usuário em desenvolvimento.

O comando aplica a migração única `20261106130000_InitialCreate`, que cria diretamente as tabelas `People` e `Vehicles`, com marca e modelo no veículo. Esta versão consolidada é destinada a um banco novo; bancos com o histórico antigo não devem receber a migração inicial novamente.

Abra `AutoSales.sln`, selecione `AutoSales.Api` como projeto de inicialização e execute com F5. Para iniciar pelo terminal, rode `dotnet run --project AutoSales.Api`. A interface fica em [http://localhost:5080/swagger](http://localhost:5080/swagger).

## Criação do banco por script SQL

Como alternativa ao `dotnet ef database update`, abra `database/CreateSchema.sql` no SQL Server Management Studio, conecte-se ao SQL Server e execute o arquivo completo. No Docker local, o servidor é `127.0.0.1,14333`, com usuário `sa` e a senha configurada no `.env`.

O script cria o banco `AutoSales` diretamente na estrutura atual, com as tabelas de pessoas (`People`) e veículos (`Vehicles`), índices e chave estrangeira do proprietário. Marca e modelo são campos de texto do veículo. O histórico equivalente das migrações fica registrado em `__EFMigrationsHistory`. Pode ser executado novamente na estrutura atual sem apagar os registros.

O único script SQL é `database/CreateSchema.sql`. Ele não atualiza bancos com a estrutura ou o histórico antigo e interrompe a execução nesses casos, sem apagar os dados.

Escolha o script SQL ou a migração para criar o banco. A conexão da API continua sendo configurada conforme a seção de execução local.

Cada veículo exige uma pessoa existente por meio de `Vehicles.PersonId`, que é obrigatório e referencia `People.Id`. Uma pessoa pode ter vários veículos. Ao excluir uma pessoa pela API, seus veículos são excluídos na mesma transação; se qualquer etapa falhar, toda a operação é desfeita. A interface pede confirmação e avisa sobre a exclusão dos veículos antes de enviar a requisição.

## Interface Angular

Com a API na porta `5080`, abra outro terminal e execute:

```powershell
cd web
npm.cmd ci
npm.cmd start
```

Abra [http://localhost:4200](http://localhost:4200). A primeira tela permite pesquisar pessoas por nome e CPF. Informe um filtro e clique em Buscar para carregar os resultados. O componente usa `ngModel` para ler os filtros. O serviço chama `GET /api/people` pelo intermediário de requisições configurado para desenvolvimento.

Para preparar a interface para envio por arquivo ZIP, pare a execução com Ctrl+C e, dentro da pasta `web`, execute:

```powershell
npm.cmd run clean
```

Esse comando remove as dependências baixadas, o cache e os arquivos de compilação. Para executar a interface novamente, use `npm.cmd ci` e `npm.cmd start`.

## API

`/api/people` e `/api/vehicles` aceitam GET para listar e POST para criar. Acrescente `/{id}` para consultar (GET), atualizar (PUT) ou excluir (DELETE). O campo `personId` vincula o veículo a uma pessoa existente na criação. A atualização permite alterar apenas os dados do veículo, mantendo o proprietário. A exclusão de pessoa pela API remove seus veículos antes de remover a pessoa, na mesma transação.

Nas listagens, use `GET /api/people?name=Ana` ou `GET /api/people?cpf=123.456.789-09` para buscar pessoas. Para veículos, use `GET /api/vehicles?search=Honda%20Civ` ou `GET /api/vehicles?plate=ABC-1D23`. Nome, marca e modelo aceitam trechos. O filtro `search` pesquisa a marca e o modelo juntos. CPF e placa usam correspondência exata e aceitam valores com ou sem máscara. Quando ambos os filtros da mesma listagem são informados, o resultado precisa atender aos dois.

Marca e modelo são informados manualmente e gravados diretamente no veículo, sem cadastros ou tabelas separados.

Sem filtros, `GET /api/people` e `GET /api/vehicles` retornam todos os registros. Quando não há registros, retornam uma lista vazia com HTTP 200.

## Arquitetura

- `AutoSales.Api` contém os controladores e o componente de tratamento de erros (`middleware`). Os controladores recebem `ISender` por injeção de dependência e enviam as operações pelo MediatR com `Send`.
- `AutoSales.Application` contém um responsável (`handler`) para cada operação: listar, consultar, cadastrar, atualizar e excluir pessoas e veículos. Os arquivos ficam separados nas pastas `Handlers/People` e `Handlers/Vehicles`. Cada classe implementa `IRequestHandler` e expõe o método `Handle`. Os modelos de entrada, comandos e consultas ficam em `Requests`, e as regras do FluentValidation em `Validators`. Essa camada valida as entradas e executa as operações por meio das interfaces de repositório.
- `AutoSales.Application` também reúne as entidades em `Entities`, as interfaces dos repositórios em `Repositories` e as exceções em `Exceptions`.
- `AutoSales.Infrastructure` implementa os repositórios com EF Core e mantém o mapeamento e as migrações do SQL Server.

Os índices únicos no banco garantem a unicidade mesmo quando duas requisições tentam gravar ao mesmo tempo. A camada de aplicação padroniza CPF e placa antes da gravação. Erros de validação, registros ausentes e conflitos retornam respostas estruturadas no formato Problem Details, com mensagens em português.

O fluxo de uma operação é: controlador → MediatR → handler → repositório → banco. Comandos (`Command`) representam cadastro, atualização ou exclusão; consultas (`Query`) representam leitura. Por exemplo, o controlador envia `GetPersonRequest` e o MediatR executa `GetPersonHandler`. As validações continuam explícitas dentro dos handlers. O registro no `Program.cs` procura os handlers no projeto Application automaticamente.

No cadastro e na atualização, a própria entrada (`CreatePersonRequest`, `UpdatePersonRequest`, `CreateVehicleRequest` ou `UpdateVehicleRequest`) implementa `IRequest` e é enviada diretamente com `_mediator.Send(request, cancellationToken)`. Não há um comando adicional envolvendo esses dados. Na atualização, o controlador atribui o ID recebido na rota antes do envio; a propriedade `Id` é ignorada no corpo JSON. Na exclusão, `DeletePersonRequest` e `DeleteVehicleRequest` recebem o identificador diretamente da rota com `[FromRoute]` e também são enviadas ao MediatR sem criar outro objeto no controlador. As consultas por ID também recebem `GetPersonRequest` ou `GetVehicleRequest` com `[FromRoute]` e enviam a request diretamente ao MediatR. As listagens usam mensagens próprias para carregar os filtros.

Quando uma pessoa, um veículo ou o proprietário informado não existe, o handler registra um aviso e retorna `Result<T>` com `ErrorType.NotFound`. O controlador responde HTTP 404 sem lançar exceção. As listagens sem resultados retornam uma lista vazia com HTTP 200.

Antes de cadastrar uma pessoa ou salvar um veículo, o handler consulta a existência do CPF ou da placa. Uma duplicidade encontrada nessa consulta é retornada como `Result<T>`, sem lançar exceção. O controlador transforma a falha em HTTP 409 e mantém a mensagem e o campo no formato lido pela interface. Os índices únicos continuam protegendo contra requisições simultâneas. Se houver conflito entre a consulta e a gravação, o banco bloqueia a operação e a falha segue para o tratamento geral de erros de salvamento, sem classificação por códigos do SQL Server. As exceções de validação e falhas inesperadas seguem para o middleware; registros não encontrados são tratados pelo resultado do handler.

O projeto utiliza MediatR 12.5.0. A referência da biblioteca está disponível em [MediatR no NuGet](https://www.nuget.org/packages/MediatR/12.5.0).

O Serilog está configurado na API para registrar avisos e erros no console. Cada handler recebe `ILogger` por um construtor tradicional e usa `LogError` no bloco `catch`, preservando a exceção com `throw`. Para registrar um aviso em uma operação, pode-se usar `LogWarning` na própria classe. Sucessos e falhas retornadas como `Result<T>` não geram logs automaticamente. O middleware monta a resposta HTTP sem registrar novamente a mesma exceção. Os dados completos das requisições não são incluídos no log.

## Testes unitários

O projeto `AutoSales.Application.Tests` usa xUnit e Moq para testar os handlers sem acessar o banco. Pode ser executado pelo Gerenciador de Testes do Visual Studio ou pelo terminal:

```powershell
dotnet test AutoSales.Application.Tests/AutoSales.Application.Tests.csproj
```

## Melhorias futuras

Adicionar paginação às listagens, autenticação e testes de integração automatizados.
