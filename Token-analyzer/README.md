# Token Analyzer

Console app em C# para varrer uma pasta raiz, localizar qualquer pasta chamada `chatSessions`, ler os arquivos e somar os credits do campo `details` por dia.

Exemplo de campo analisado:

```json
"details":"GPT-5.4 • 11.1 credits"
```

## Requisitos

- .NET SDK 8.0+

## Como executar

Para registrar a tarefa `TokenAnalyzerDailyReport` no Agendador do Windows, configurada para executar diariamente às 18:00, execute sem argumentos no diretório que deverá ser analisado:

```powershell
dotnet run
```

O diretório atual é armazenado na ação da tarefa e será usado como raiz da análise. Se o horário de registro já tiver passado das 18:00, a primeira execução ocorrerá às 18:00 do dia seguinte.

Para executar a análise manualmente:

```powershell
dotnet run -- "C:\caminho\da\raiz"
```

Com período customizado:

```powershell
dotnet run -- "C:\caminho\da\raiz" "2026-06-01" "2026-06-22"
```

Se quiser enviar o relatório para o Slack, defina a variável de ambiente `Slack__Token` e mantenha o e-mail em `src/appsettings.json`.

Formatos de data aceitos:

- `yyyy-MM-dd`
- `dd/MM/yyyy`
- `dd-MM-yyyy`

## Regras de filtro por data

- Data padrão inicial: `01/06` do ano atual.
- Data padrão final: hoje.
- A pasta `chatSessions` só é considerada se ela ou algum arquivo dentro dela estiver no período.
- Os arquivos processados são filtrados pela data de modificação dentro do período.
- Cada entrada de credits usa:
  - `timestamp` da linha, se existir.
  - senão, data de modificação do arquivo.

## Saída

Relatório no console com:

- quantidade de pastas `chatSessions` encontradas
- quantidade de pastas consideradas no período
- quantidade de arquivos analisados
- quantidade de entradas de credits encontradas
- tabela de credits diários
- total geral

## Estrutura do código

- `src/Program.cs` — registra o job diário ou orquestra a execução da análise.
- `src/Infrastructure` — parsing, validação e integração com o Agendador de Tarefas do Windows.
- `src/Presentation` — formatação e impressão do relatório no console.
- `src/Services` — análise das sessões e envio da notificação ao Slack.
