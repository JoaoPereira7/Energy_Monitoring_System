# ?? Testes de Performance - Energy Monitoring System

Este documento cont�m as instru��es para executar os testes de performance usando Artillery.

## ?? Pr�-requisitos

### 1. Node.js e npm
```bash
# Verificar se Node.js est� instalado
node --version
npm --version

# Se n�o estiver instalado, baixe em: https://nodejs.org/
```

### 2. Artillery
```bash
# Instalar Artillery globalmente
npm install -g artillery

# Verificar instala��o
artillery version
```

### 3. API em execu��o
Certifique-se de que a API est� rodando em `https://localhost:7283`

```bash
# Executar a API
dotnet run

# Verificar se est� respondendo
curl https://localhost:7283/health
```

## ?? Execu��o dos Testes

### Op��o 1: Script PowerShell (Recomendado)
```powershell
# Execu��o b�sica
.\executar-testes-performance.ps1

# Com relat�rio HTML
.\executar-testes-performance.ps1 -GenerateHtmlReport

# Usando arquivo espec�fico
.\executar-testes-performance.ps1 -ConfigFile "teste-performance-simples.yml"
```

### Op��o 2: Comando Artillery Direto
```bash
# Execu��o b�sica
artillery run teste-performance.yml

# Com arquivo de sa�da
artillery run teste-performance.yml --output performance-results.json

# Gerar relat�rio HTML
artillery report performance-results.json --output performance-report.html
```

## ?? Arquivos de Configura��o

### `teste-performance.yml`
- **Arquivo principal** com cen�rios completos
- Inclui fun��es JavaScript para dados din�micos
- Configura��es avan�adas de performance

### `teste-performance-simples.yml`
- **Vers�o simplificada** sem depend�ncias externas
- Dados est�ticos para testes b�sicos
- Ideal para troubleshooting

## ?? Crit�rios de Aprova��o

| Crit�rio | Meta | Descri��o |
|----------|------|-----------|
| **Taxa de Sucesso** | > 99% | Menos de 1% de erros |
| **Tempo M�dio** | < 1000ms | Resposta m�dia abaixo de 1 segundo |
| **P95** | < 1500ms | 95% das requisi��es em menos de 1.5s |
| **P99** | < 2000ms | 99% das requisi��es em menos de 2s |
| **Erros 5xx** | = 0 | Zero erros de servidor |

## ?? Cen�rios de Teste

### 1. Inser��o de Leituras (70% do tr�fego)
- **Endpoint**: `POST /api/medidores/{id}/leituras`
- **Objetivo**: Simular inser��o cont�nua de dados de medidores
- **Payload**: Dados de leitura com valores realistas

### 2. Consulta Recente (25% do tr�fego)
- **Endpoint**: `GET /api/medidores/{id}/leituras`
- **Per�odo**: �ltima hora
- **Limite**: 50 registros

### 3. Consulta Hist�rica (5% do tr�fego)
- **Endpoint**: `GET /api/medidores/{id}/leituras`
- **Per�odo**: �ltimos 7 dias
- **Limite**: 100 registros

## ?? Fases de Carga

1. **Aquecimento** (30s): 5 req/s
2. **Crescimento** (2min): 10 ? 50 req/s
3. **Sustenta��o** (3min): 50 req/s constante
4. **Pico** (1min): 100 req/s

**Total**: ~6.5 minutos de teste

## ?? Exemplo de Payload

```json
{
  "timestamp": "2024-01-15T10:30:00.000Z",
  "tensao": 220.5,
  "corrente": 15.2,
  "potenciaAtiva": 3351.6,
  "potenciaReativa": 892.4,
  "energiaAtivaDireta": 1250.75,
  "energiaAtivaReversa": 32.18,
  "fatorPotencia": 0.97,
  "frequencia": 60.1
}
```

## ??? Troubleshooting

### Erro de Certificado SSL
```bash
# Adicionar ao in�cio do teste (n�o recomendado para produ��o)
export NODE_TLS_REJECT_UNAUTHORIZED=0
```

### API n�o responde
1. Verificar se a aplica��o est� rodando
2. Confirmar a porta (padr�o: 7283)
3. Testar endpoint de health: `/health`

### Erro de mem�ria
```bash
# Aumentar limite de mem�ria do Node.js
export NODE_OPTIONS="--max-old-space-size=4096"
```

### Performance baixa
1. Verificar recursos do sistema (CPU, RAM)
2. Reduzir `arrivalRate` nos cen�rios
3. Aumentar `pool` nas configura��es HTTP
4. Verificar configura��o do banco de dados

## ?? Interpreta��o dos Resultados

### M�tricas Importantes
- **RPS**: Requisi��es por segundo processadas
- **Latency**: Tempo de resposta
- **Error Rate**: Taxa de erro
- **Throughput**: Volume de dados processado

### Alertas
- ?? **P95 > 1500ms**: Performance degradando
- ? **Error Rate > 1%**: Problemas de estabilidade
- ?? **CPU > 80%**: Poss�vel gargalo de processamento

## ?? Customiza��o

### Ajustar Carga
```yaml
phases:
  - duration: 60      # Dura��o em segundos
    arrivalRate: 10   # Requisi��es por segundo
    rampTo: 50        # Crescer at� N req/s
```

### Adicionar Cen�rios
```yaml
scenarios:
  - weight: 30        # 30% do tr�fego
    name: "Novo Cen�rio"
    flow:
      - get:
          url: "/novo-endpoint"
```

### Configurar Timeouts
```yaml
http:
  timeout: 20    # Timeout em segundos
  pool: 100      # Pool de conex�es
```

## ?? Suporte

Para problemas ou d�vidas:
1. Verificar logs da aplica��o
2. Consultar documenta��o do Artillery: https://artillery.io/docs/
3. Revisar configura��es de infraestrutura