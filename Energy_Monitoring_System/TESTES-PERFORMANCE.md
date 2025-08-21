# ?? Testes de Performance - Energy Monitoring System

Este documento contém as instruções para executar os testes de performance usando Artillery.

## ?? Pré-requisitos

### 1. Node.js e npm
```bash
# Verificar se Node.js está instalado
node --version
npm --version

# Se não estiver instalado, baixe em: https://nodejs.org/
```

### 2. Artillery
```bash
# Instalar Artillery globalmente
npm install -g artillery

# Verificar instalação
artillery version
```

### 3. API em execução
Certifique-se de que a API está rodando em `https://localhost:7283`

```bash
# Executar a API
dotnet run

# Verificar se está respondendo
curl https://localhost:7283/health
```

## ?? Execução dos Testes

### Opção 1: Script PowerShell (Recomendado)
```powershell
# Execução básica
.\executar-testes-performance.ps1

# Com relatório HTML
.\executar-testes-performance.ps1 -GenerateHtmlReport

# Usando arquivo específico
.\executar-testes-performance.ps1 -ConfigFile "teste-performance-simples.yml"
```

### Opção 2: Comando Artillery Direto
```bash
# Execução básica
artillery run teste-performance.yml

# Com arquivo de saída
artillery run teste-performance.yml --output performance-results.json

# Gerar relatório HTML
artillery report performance-results.json --output performance-report.html
```

## ?? Arquivos de Configuração

### `teste-performance.yml`
- **Arquivo principal** com cenários completos
- Inclui funções JavaScript para dados dinâmicos
- Configurações avançadas de performance

### `teste-performance-simples.yml`
- **Versão simplificada** sem dependências externas
- Dados estáticos para testes básicos
- Ideal para troubleshooting

## ?? Critérios de Aprovação

| Critério | Meta | Descrição |
|----------|------|-----------|
| **Taxa de Sucesso** | > 99% | Menos de 1% de erros |
| **Tempo Médio** | < 1000ms | Resposta média abaixo de 1 segundo |
| **P95** | < 1500ms | 95% das requisições em menos de 1.5s |
| **P99** | < 2000ms | 99% das requisições em menos de 2s |
| **Erros 5xx** | = 0 | Zero erros de servidor |

## ?? Cenários de Teste

### 1. Inserção de Leituras (70% do tráfego)
- **Endpoint**: `POST /api/medidores/{id}/leituras`
- **Objetivo**: Simular inserção contínua de dados de medidores
- **Payload**: Dados de leitura com valores realistas

### 2. Consulta Recente (25% do tráfego)
- **Endpoint**: `GET /api/medidores/{id}/leituras`
- **Período**: Última hora
- **Limite**: 50 registros

### 3. Consulta Histórica (5% do tráfego)
- **Endpoint**: `GET /api/medidores/{id}/leituras`
- **Período**: Últimos 7 dias
- **Limite**: 100 registros

## ?? Fases de Carga

1. **Aquecimento** (30s): 5 req/s
2. **Crescimento** (2min): 10 ? 50 req/s
3. **Sustentação** (3min): 50 req/s constante
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
# Adicionar ao início do teste (não recomendado para produção)
export NODE_TLS_REJECT_UNAUTHORIZED=0
```

### API não responde
1. Verificar se a aplicação está rodando
2. Confirmar a porta (padrão: 7283)
3. Testar endpoint de health: `/health`

### Erro de memória
```bash
# Aumentar limite de memória do Node.js
export NODE_OPTIONS="--max-old-space-size=4096"
```

### Performance baixa
1. Verificar recursos do sistema (CPU, RAM)
2. Reduzir `arrivalRate` nos cenários
3. Aumentar `pool` nas configurações HTTP
4. Verificar configuração do banco de dados

## ?? Interpretação dos Resultados

### Métricas Importantes
- **RPS**: Requisições por segundo processadas
- **Latency**: Tempo de resposta
- **Error Rate**: Taxa de erro
- **Throughput**: Volume de dados processado

### Alertas
- ?? **P95 > 1500ms**: Performance degradando
- ? **Error Rate > 1%**: Problemas de estabilidade
- ?? **CPU > 80%**: Possível gargalo de processamento

## ?? Customização

### Ajustar Carga
```yaml
phases:
  - duration: 60      # Duração em segundos
    arrivalRate: 10   # Requisições por segundo
    rampTo: 50        # Crescer até N req/s
```

### Adicionar Cenários
```yaml
scenarios:
  - weight: 30        # 30% do tráfego
    name: "Novo Cenário"
    flow:
      - get:
          url: "/novo-endpoint"
```

### Configurar Timeouts
```yaml
http:
  timeout: 20    # Timeout em segundos
  pool: 100      # Pool de conexões
```

## ?? Suporte

Para problemas ou dúvidas:
1. Verificar logs da aplicação
2. Consultar documentação do Artillery: https://artillery.io/docs/
3. Revisar configurações de infraestrutura