# Script de Execu��o dos Testes de Performance
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

# Verificar se Artillery est� instalado
Write-Host "?? Verificando instala��o do Artillery..." -ForegroundColor Yellow

try {
    $artilleryVersion = artillery version
    Write-Host "? Artillery encontrado: $artilleryVersion" -ForegroundColor Green
} catch {
    Write-Host "? Artillery n�o encontrado. Instalando..." -ForegroundColor Red
    Write-Host "?? Executando: npm install -g artillery" -ForegroundColor Yellow
    npm install -g artillery
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "? Falha na instala��o do Artillery. Verifique se o Node.js est� instalado." -ForegroundColor Red
        exit 1
    }
    
    Write-Host "? Artillery instalado com sucesso!" -ForegroundColor Green
}

# Verificar se o arquivo de configura��o existe
if (-not (Test-Path $ConfigFile)) {
    Write-Host "? Arquivo de configura��o n�o encontrado: $ConfigFile" -ForegroundColor Red
    Write-Host "?? Arquivos dispon�veis:" -ForegroundColor Yellow
    Get-ChildItem -Filter "*.yml" | ForEach-Object { Write-Host "   - $($_.Name)" -ForegroundColor White }
    exit 1
}

Write-Host "?? Usando arquivo de configura��o: $ConfigFile" -ForegroundColor Green

# Verificar se a API est� rodando
Write-Host "?? Verificando se a API est� dispon�vel..." -ForegroundColor Yellow

try {
    $response = Invoke-WebRequest -Uri "https://localhost:7283/health" -Method GET -TimeoutSec 10 -SkipCertificateCheck
    if ($response.StatusCode -eq 200) {
        Write-Host "? API est� respondendo na porta 7283" -ForegroundColor Green
    }
} catch {
    Write-Host "??  API n�o est� respondendo em https://localhost:7283" -ForegroundColor Red
    Write-Host "?? Certifique-se de que a aplica��o est� rodando antes de executar os testes" -ForegroundColor Yellow
    $continuar = Read-Host "Deseja continuar mesmo assim? (y/N)"
    if ($continuar -ne "y" -and $continuar -ne "Y") {
        exit 1
    }
}

# Preparar nomes dos arquivos de relat�rio
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$reportFile = "performance-report-$timestamp.json"
$htmlReportFile = "performance-report-$timestamp.html"

Write-Host "?? Iniciando execu��o dos testes..." -ForegroundColor Cyan
Write-Host "??  Este processo pode levar alguns minutos..." -ForegroundColor Yellow

# Executar Artillery
$artilleryCommand = "artillery run $ConfigFile --output $reportFile"

Write-Host "?? Comando: $artilleryCommand" -ForegroundColor Gray

try {
    Invoke-Expression $artilleryCommand
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? Testes executados com sucesso!" -ForegroundColor Green
        
        # Gerar relat�rio HTML se solicitado
        if ($GenerateHtmlReport) {
            Write-Host "?? Gerando relat�rio HTML..." -ForegroundColor Yellow
            artillery report $reportFile --output $htmlReportFile
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "? Relat�rio HTML gerado: $htmlReportFile" -ForegroundColor Green
            } else {
                Write-Host "??  Falha ao gerar relat�rio HTML" -ForegroundColor Yellow
            }
        }
        
        # Exibir resumo dos resultados
        Write-Host "" -ForegroundColor White
        Write-Host "?? RESUMO DOS TESTES" -ForegroundColor Cyan
        Write-Host "===================" -ForegroundColor Cyan
        
        if (Test-Path $reportFile) {
            $report = Get-Content $reportFile | ConvertFrom-Json
            $aggregate = $report.aggregate
            
            Write-Host "?? Requisi��es Totais: $($aggregate.counters.'http.requests')" -ForegroundColor White
            Write-Host "? Respostas 2xx: $($aggregate.counters.'http.responses')" -ForegroundColor Green
            
            if ($aggregate.counters.'http.codes.400') {
                Write-Host "??  Erros 4xx: $($aggregate.counters.'http.codes.400')" -ForegroundColor Yellow
            }
            
            if ($aggregate.counters.'http.codes.500') {
                Write-Host "? Erros 5xx: $($aggregate.counters.'http.codes.500')" -ForegroundColor Red
            }
            
            Write-Host "??  Tempo M�dio: $($aggregate.latency.mean) ms" -ForegroundColor White
            Write-Host "?? P95: $($aggregate.latency.p95) ms" -ForegroundColor White
            Write-Host "?? P99: $($aggregate.latency.p99) ms" -ForegroundColor White
            
            # Verificar crit�rios de aprova��o
            Write-Host "" -ForegroundColor White
            Write-Host "?? CRIT�RIOS DE APROVA��O" -ForegroundColor Cyan
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
            
            # Tempo m�dio dentro dos limites
            if ($aggregate.latency.mean -lt 1000) {
                Write-Host "? Tempo M�dio: $($aggregate.latency.mean) ms (< 1000ms)" -ForegroundColor Green
            } else {
                Write-Host "? Tempo M�dio: $($aggregate.latency.mean) ms (> 1000ms)" -ForegroundColor Red
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
        Write-Host "   ?? Relat�rio JSON: $reportFile" -ForegroundColor White
        if ($GenerateHtmlReport -and (Test-Path $htmlReportFile)) {
            Write-Host "   ?? Relat�rio HTML: $htmlReportFile" -ForegroundColor White
        }
        
    } else {
        Write-Host "? Falha na execu��o dos testes" -ForegroundColor Red
        exit 1
    }
    
} catch {
    Write-Host "? Erro durante a execu��o: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "" -ForegroundColor White
Write-Host "?? Testes de performance conclu�dos!" -ForegroundColor Green