


# Bank API — Banco do Caminhoneiro

API REST para gestão de contas e transações financeiras voltada a motoristas autônomos e transportadoras, construída em .NET com arquitetura em camadas.

---
## Teste API REST
https://www.youtube.com/watch?v=jqJTyTjhUQE


## Sobre o projeto

O setor de frete tem uma característica própria: o motorista recebe por viagem, muitas vezes de várias fontes diferentes, e precisa controlar entradas, saídas e adiantamentos de forma simples. A Bank API é o back-end desse controle — cadastro de contas, registro de transações e consulta de saldo e extrato.

Este é um projeto de estudo, escrito para exercitar arquitetura em camadas, separação de responsabilidades e testes em um domínio com regra de negócio real (dinheiro não admite inconsistência).

## Arquitetura

O projeto é dividido em quatro camadas, com a dependência apontando sempre para dentro:

```
PXbankAPI.API              → controllers, configuração, injeção de dependência
PXbankAPI.Application      → casos de uso, DTOs, validações
PXbankAPI.Domain           → entidades e interfaces (não depende de ninguém)
PXbankAPI.Infrastructure   → EF Core, DbContext, implementação dos repositórios
```

Regra que o projeto respeita:

| Camada | Pode referenciar |
|---|---|
| Domain | nada |
| Application | Domain |
| Infrastructure | Domain |
| API | Application e Infrastructure |

O ponto central: **Domain não conhece Entity Framework.** Ele declara a interface (`IContaRepository`), e a Infrastructure implementa. Isso permite trocar o banco ou testar a regra de negócio sem subir infraestrutura nenhuma.

## Stack

- .NET {{8}} / C#
- ASP.NET Core Web API
- Entity Framework Core
- {{SQL Server / PostgreSQL / SQLite}} <!-- AJUSTE: qual você usou -->
- xUnit para testes
- Docker
- Swagger para documentação dos endpoints

## Funcionalidades

- [ ] Cadastro e consulta de conta
- [x] Registro de transação (crédito e débito)
- [x] Consulta de saldo
- [] Extrato por período
- [x] Validação de saldo insuficiente
- [ ] Autenticação com JWT

## Como rodar

```bash
git clone https://github.com/gitflavius/banco-do-caminhoneiro.git
cd banco-do-caminhoneiro

# restaurar e compilar
dotnet restore
dotnet build

# aplicar as migrations
dotnet ef database update --project PXbankAPI.Infrastructure --startup-project PXbankAPI.API

# subir a API
dotnet run --project PXbankAPI.API
```

A documentação interativa fica em `https://localhost:{{5001}}/swagger`.

### Com Docker

```bash
docker compose up --build
```

## Endpoints

<!-- AJUSTE: mantenha só os que existem de verdade -->

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/api/contas` | Cria uma conta |
| `GET` | `/api/contas/{id}` | Consulta uma conta |
| `GET` | `/api/contas/{id}/saldo` | Retorna o saldo atual |
| `GET` | `/api/contas/{id}/extrato` | Extrato por período |
| `POST` | `/api/transacoes` | Registra crédito ou débito |

## Testes

```bash
dotnet test
```

Os testes cobrem as regras de negócio do Domain (saldo insuficiente, valor inválido, conta inexistente) e um teste de integração de endpoint com `WebApplicationFactory`.

## Decisões técnicas

Esta seção existe porque a decisão importa mais que o código.

**Por que quatro camadas em vez de um projeto único.**
Num CRUD pequeno, projeto único é mais rápido. Separei porque o objetivo aqui era justamente praticar o isolamento do domínio: com `Domain` sem referência a EF Core, a regra "não pode debitar mais que o saldo" é testável sem banco, sem mock de contexto e sem subir a aplicação. O custo é mais arquivo e mais cerimônia — vale quando a regra de negócio é o que importa, e não vale quando o sistema é só entrada e saída de dados.

**Por que repositório em vez de usar o DbContext direto no controller.**
Chamar o DbContext do controller acopla a regra de negócio ao Entity Framework: qualquer troca de ORM ou de banco vira reescrita. Com a interface no Domain, a Application depende de um contrato, não de uma implementação. Reconheço que em projeto pequeno isso é discutível — repository sobre um ORM que já é um repository é redundância. Mantive pelo valor de aprendizado e pela testabilidade.

**Por que {{SQL Server}} e o que eu mudaria em produção.**
{{Escolhi pela facilidade de rodar local}}. Com connection string vindo de variável de ambiente e não do appsettings, migrations aplicadas pelo pipeline e não pela aplicação subindo, e índice na coluna de data das transações — o extrato por período é a consulta mais frequente e sem índice ela degrada rápido conforme a tabela cresce.

**Sobre consistência de valores monetários.**
Uso `decimal` e não `double` ou `float`. Ponto flutuante binário não representa valores decimais com exatidão, e em soma de transações o erro acumula. Em contexto financeiro isso não é detalhe.

**O que eu faria diferente com mais tempo.**
{{Escreva com sinceridade — 2 ou 3 itens}}. Por exemplo: transação de banco de dados envolvendo débito e crédito numa transferência, para que uma falha no meio não deixe o dinheiro sumir; log estruturado; e paginação no extrato, que hoje retorna tudo de uma vez.

## Próximos passos

- [ ] Autenticação e autorização por conta
- [ ] Paginação no extrato
- [ ] Pipeline de CI rodando os testes a cada push
- [x] Cobertura de testes acima de {{70}}%

---

Desenvolvido por Flávio Augusto Rodrigues — [LinkedIn](https://www.linkedin.com/in/flávio-augusto-rodrigues/) · flavius.rodrigues@gmail.com
