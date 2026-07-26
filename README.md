# Token Analyzer

Aplicacao de console em C# para analisar consumo diario de credits a partir de logs de sessoes do VS Code e enviar um resumo no Slack.

## O que o projeto faz

- Busca pastas chatSessions dentro de:
  - `%AppData%\\Code\\User\\workspaceStorage`
- Le os arquivos encontrados no periodo definido
- Extrai valores de credits do campo details
- Consolida total por dia
- Exibe relatorio no console
- Envia o relatorio por mensagem direta no Slack (quando configurado)
- Pode registrar uma tarefa diaria no Windows Task Scheduler para rodar automaticamente as 18:00

## Requisitos

- .NET SDK 8.0+
- Windows (para registro de tarefa diaria)
- Token de bot no Slack com permissoes para enviar mensagens

## Dependencias principais

- Microsoft.Extensions.Configuration.Json
- Microsoft.Extensions.Configuration.EnvironmentVariables
- SlackNet.AspNetCore
- TaskScheduler

## Configuracao

A aplicacao carrega configuracoes de `appsettings.json` e variaveis de ambiente.

### Opcao 1: Variaveis de ambiente (recomendado)

No Windows PowerShell:

```powershell
$env:Slack__Token="xoxb-seu-token"
$env:Slack__Email="seu-email@dominio.com"
```

### Opcao 2: Arquivo appsettings

Edite `src/appsettings.json`:

```json
{
  "Slack": {
    "Token": "xoxb-seu-token",
    "Email": "seu-email@dominio.com"
  }
}
```

## Como executar

### 1. Restaurar e compilar

```bash
dotnet restore TokenAnalyzer.csproj
dotnet build TokenAnalyzer.csproj
```

### 2. Registrar tarefa diaria (18:00)

Sem argumentos, a aplicacao registra a tarefa no Windows:

```bash
dotnet run --project TokenAnalyzer.csproj
```

### 3. Executar o job manualmente

```bash
dotnet run --project TokenAnalyzer.csproj -- --executar-job
```

## Saida esperada

O relatorio inclui:

- Raiz analisada
- Periodo
- Quantidade de pastas chatSessions encontradas/processadas
- Quantidade de arquivos analisados
- Quantidade de entradas de credits encontradas
- Tabela diaria com:
  - Data
  - Credits
  - Cost (estimado como credits / 100)
- Total consolidado

## Estrutura

```text
src/
  Program.cs
  appsettings.json
  Infrastructure/
    ArgumentParser.cs
    DailyTaskRegistrar.cs
    InputValidator.cs
  Presentation/
    ConsoleReportPrinter.cs
  Services/
    ChatSessionAnalyzer.cs
    SlackNotify.cs
    Interfaces/
      INotify.cs
```

## Observacoes importantes

- O caminho de analise de logs e dinamico por usuario, via `%AppData%`.
- Se Token ou Email do Slack nao estiverem completos, o envio de notificacao e ignorado.
- Para seguranca, evite versionar segredos reais no repositorio.

## Melhorias futuras sugeridas

- Permitir periodo personalizado por argumento de linha de comando.
- Permitir configuracao de horario da tarefa diaria.
- Adicionar testes para parser de credits e datas.
- Adicionar suporte para saida em CSV.
