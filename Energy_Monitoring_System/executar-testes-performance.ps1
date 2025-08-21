# Script de Execução dos Testes de Performance
# Artillery Performance Testing Script

param(
    [Parameter(Mandatory=$false)]
    [string]$ConfigFile = "teste-performance.yml",
    
    [Parameter(Mandatory=$false)]
    [string]$ReportFormat = "json",
    
    [Parameter(Mandatory=$false)]
    [switch]$GenerateHtmlReport = $false
)

Write-Host "?? Iniciando Testes de Performance com Artillery" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan

# Verificar se Artillery está instalado
Write-Host "?? Verificando instalação do Artillery..." -ForegroundColor Yellow

try {
    $artilleryVersion = artillery version
    Write-Host "? Artillery encontrado: $artilleryVersion" -ForegroundColor Green
} catch {
    Write-Host "? Artillery não encontrado. Instalando..." -ForegroundColor Red
    Write-Host "?? Executando: npm install -g artillery" -ForegroundColor Yellow
    npm install -g artillery
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "? Falha na instalação do Artillery. Verifique se o Node.js está instalado." -ForegroundColor Red
        exit 1
    }
    
    Write-Host "? Artillery instalado com sucesso!" -ForegroundColor Green
}

# Verificar se o arquivo de configuração existe
if (-not (Test-Path $ConfigFile)) {
    Write-Host "? Arquivo de configuração não encontrado: $ConfigFile" -ForegroundColor Red
    Write-Host "?? Arquivos disponíveis:" -ForegroundColor Yellow
    Get-ChildItem -Filter "*.yml" | ForEach-Object { Write-Host "   - $($_.Name)" -ForegroundColor White }
    exit 1
}

Write-Host "?? Usando arquivo de configuração: $ConfigFile" -ForegroundColor Green

# Verificar se a API está rodando
Write-Host "?? Verificando se a API está disponível..." -ForegroundColor Yellow

try {
    $response = Invoke-WebRequest -Uri "https://localhost:7283/health" -Method GET -TimeoutSec 10 -SkipCertificateCheck
    if ($response.StatusCode -eq 200) {
        Write-Host "? API está respondendo na porta 7283" -ForegroundColor Green
    }
} catch {
    Write-Host "??  API não está respondendo em https://localhost:7283" -ForegroundColor Red
    Write-Host "?? Certifique-se de que a aplicação está rodando antes de executar os testes" -ForegroundColor Yellow
    $continuar = Read-Host "Deseja continuar mesmo assim? (y/N)"
    if ($continuar -ne "y" -and $continuar -ne "Y") {
        exit 1
    }
}

# Preparar nomes dos arquivos de relatório
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$reportFile = "performance-report-$timestamp.json"
$htmlReportFile = "performance-report-$timestamp.html"

Write-Host "?? Iniciando execução dos testes..." -ForegroundColor Cyan
Write-Host "??  Este processo pode levar alguns minutos..." -ForegroundColor Yellow

# Executar Artillery
$artilleryCommand = "artillery run $ConfigFile --output $reportFile"

Write-Host "?? Comando: $artilleryCommand" -ForegroundColor Gray

try {
    Invoke-Expression $artilleryCommand
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? Testes executados com sucesso!" -ForegroundColor Green
        
        # Gerar relatório HTML se solicitado
        if ($GenerateHtmlReport) {
            Write-Host "?? Gerando relatório HTML..." -ForegroundColor Yellow
            artillery report $reportFile --output $htmlReportFile
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "? Relatório HTML gerado: $htmlReportFile" -ForegroundColor Green
            } else {
                Write-Host "??  Falha ao gerar relatório HTML" -ForegroundColor Yellow
            }
        }
        
        # Exibir resumo dos resultados
        Write-Host "" -ForegroundColor White
        Write-Host "?? RESUMO DOS TESTES" -ForegroundColor Cyan
        Write-Host "===================" -ForegroundColor Cyan
        
        if (Test-Path $reportFile) {
            $report = Get-Content $reportFile | ConvertFrom-Json
            $aggregate = $report.aggregate
            
            Write-Host "?? Requisições Totais: $($aggregate.counters.'http.requests')" -ForegroundColor White
            Write-Host "? Respostas 2xx: $($aggregate.counters.'http.responses')" -ForegroundColor Green
            
            if ($aggregate.counters.'http.codes.400') {
                Write-Host "??  Erros 4xx: $($aggregate.counters.'http.codes.400')" -ForegroundColor Yellow
            }
            
            if ($aggregate.counters.'http.codes.500') {
                Write-Host "? Erros 5xx: $($aggregate.counters.'http.codes.500')" -ForegroundColor Red
            }
            
            Write-Host "??  Tempo Médio: $($aggregate.latency.mean) ms" -ForegroundColor White
            Write-Host "?? P95: $($aggregate.latency.p95) ms" -ForegroundColor White
            Write-Host "?? P99: $($aggregate.latency.p99) ms" -ForegroundColor White
            
            # Verificar critérios de aprovação
            Write-Host "" -ForegroundColor White
            Write-Host "?? CRITÉRIOS DE APROVAÇÃO" -ForegroundColor Cyan
            Write-Host "========================" -ForegroundColor Cyan
            
            $totalRequests = $aggregate.counters.'http.requests'
            $successfulRequests = $aggregate.counters.'http.responses'
            $successRate = ($successfulRequests / $totalRequests) * 100
            
            # Taxa de sucesso > 99%
            if ($successRate -gt 99) {
                Write-Host "? Taxa de Sucesso: $($successRate.ToString('F2'))% (> 99%)" -ForegroundColor Green
            } else {
                Write-Host "? Taxa de Sucesso: $($successRate.ToString('F2'))% (< 99%)" -ForegroundColor Red
            }
            
            # Tempo médio dentro dos limites
            if ($aggregate.latency.mean -lt 1000) {
                Write-Host "? Tempo Médio: $($aggregate.latency.mean) ms (< 1000ms)" -ForegroundColor Green
            } else {
                Write-Host "? Tempo Médio: $($aggregate.latency.mean) ms (> 1000ms)" -ForegroundColor Red
            }
            
            # Zero erros 5xx
            $errors5xx = if ($aggregate.counters.'http.codes.500') { $aggregate.counters.'http.codes.500' } else { 0 }
            if ($errors5xx -eq 0) {
                Write-Host "? Erros 5xx: $errors5xx (Zero erros)" -ForegroundColor Green
            } else {
                Write-Host "? Erros 5xx: $errors5xx (> 0)" -ForegroundColor Red
            }
        }
        
        Write-Host "" -ForegroundColor White
        Write-Host "?? Arquivos gerados:" -ForegroundColor Cyan
        Write-Host "   ?? Relatório JSON: $reportFile" -ForegroundColor White
        if ($GenerateHtmlReport -and (Test-Path $htmlReportFile)) {
            Write-Host "   ?? Relatório HTML: $htmlReportFile" -ForegroundColor White
        }
        
    } else {
        Write-Host "? Falha na execução dos testes" -ForegroundColor Red
        exit 1
    }
    
} catch {
    Write-Host "? Erro durante a execução: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "" -ForegroundColor White
Write-Host "?? Testes de performance concluídos!" -ForegroundColor Green