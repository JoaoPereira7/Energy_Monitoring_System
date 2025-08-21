# Energy Monitoring System

API para monitoramento de leituras de energia elétrica, desenvolvida em .NET 8, com arquitetura em camadas, validação robusta e testes automatizados.

## Sumário
- [Descrição](#descrição)
- [Funcionalidades](#funcionalidades)
- [Arquitetura](#arquitetura)
- [Como Executar](#como-executar)
- [Testes](#testes)
- [Exemplos de Uso](#exemplos-de-uso)
- [Documentação OpenAPI/Swagger](#documentação-openapiswagger)
- [Configurações](#configurações)
- [Tecnologias Utilizadas](#tecnologias-utilizadas)
- [Estrutura de Pastas](#estrutura-de-pastas)

---

## Descrição

O Energy Monitoring System é uma API RESTful para registrar e consultar leituras de medidores de energia elétrica, com validações de domínio e persistência em banco de dados relacional.

## Funcionalidades

- **Registrar leitura**: Adiciona uma nova leitura para um medidor.
- **Consultar leituras**: Busca leituras de um medidor em um período.
- **Validação de dados**: Garante integridade dos dados conforme regras de negócio.
- **Health check**: Endpoint para verificação de saúde da aplicação.

## Arquitetura

- **API**: Controllers e endpoints REST.
- **Application**: Validações e regras de negócio.
- **Domain**: Entidades, DTOs e interfaces.
- **Infrastructure**: Persistência de dados (Entity Framework Core).
- **Tests**: Testes unitários e de integração (xUnit, Moq, FluentAssertions).

## Como Executar

1. **Pré-requisitos**:
   - .NET 8 SDK
   - SQL Server (ou use o banco em memória para testes)

2. **Configuração**:
   - Ajuste a connection string em `appsettings.json` se necessário.

3. **Executando a API**:
   ```sh
   dotnet build
   dotnet run --project Energy_Monitoring_System.API
   ```
   Acesse: [http://localhost:5097/swagger](http://localhost:5097/swagger)

## Testes

Execute todos os testes automatizados com:
```sh
dotnet test
```
Os testes cobrem cenários de sucesso, validação e falhas dos endpoints principais.

## Exemplos de Uso

### Registrar leitura

```
POST /api/medidores/{medidorId}/leituras
Content-Type: application/json

{
  "timestamp": "2024-06-01T12:00:00Z",
  "tensao": 220,
  "corrente": 10,
  "potenciaAtiva": 1000,
  "potenciaReativa": 500,
  "energiaAtivaDireta": 500,
  "energiaAtivaReversa": 20,
  "fatorPotencia": 0.95,
  "frequencia": 60
}
```

### Consultar leituras

```
GET /api/medidores/{medidorId}/leituras?dataInicio=2024-06-01T00:00:00Z&dataFim=2024-06-02T00:00:00Z&limite=100
```

## Documentação OpenAPI/Swagger

Acesse a documentação interativa em `/swagger` após rodar a aplicação.

## Configurações

- **Banco de dados**: Configure a string de conexão em `appsettings.json`.
- **Logs**: Logs diários em arquivo via Serilog (`Logs/log-*.txt`).
- **Health check**: `/health`

## Tecnologias Utilizadas

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server (ou InMemory para testes)
- Serilog (logging)
- FluentValidation
- xUnit, Moq, FluentAssertions (testes)

## Estrutura de Pastas

```
Energy_Monitoring_System/
├── Energy_Monitoring_System.API/           # API e controllers
├── Energy_Monitoring_System.Application/   # Validadores e regras de negócio
├── Energy_Monitoring_System.Domain/        # Entidades, DTOs, interfaces
├── Energy_Monitoring_System.Infrastructure/# Persistência e repositórios
├── Energy_Monitoring_System.Tests/         # Testes automatizados
└── swagger.yaml                            # Especificação OpenAPI
```

---

Sinta-se à vontade para adaptar este README.md conforme necessidades específicas do seu projeto!
