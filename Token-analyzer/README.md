# Token Analyzer

Console app em C# para varrer uma pasta raiz, localizar qualquer pasta chamada `chatSessions`, ler os arquivos e somar os credits do campo `details` por dia.

Exemplo de campo analisado:

```json
"details":"GPT-5.4 • 11.1 credits"
```

## Requisitos

- .NET SDK 8.0+

## Como executar

No diretório do projeto:

```powershell
dotnet run -- "C:\caminho\da\raiz"
```

Com período customizado:

```powershell
dotnet run -- "C:\caminho\da\raiz" "2026-06-01" "2026-06-22"
```

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
