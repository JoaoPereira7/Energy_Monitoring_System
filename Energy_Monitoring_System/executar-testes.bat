@echo off
setlocal enabledelayedexpansion

echo.
echo ?? Testes de Performance - Energy Monitoring System
echo ===================================================
echo.

:: Verificar se Node.js está instalado
echo ?? Verificando Node.js...
node --version >nul 2>&1
if errorlevel 1 (
    echo ? Node.js não encontrado. Instale em: https://nodejs.org/
    pause
    exit /b 1
) else (
    echo ? Node.js encontrado
)

:: Verificar se Artillery está instalado
echo ?? Verificando Artillery...
artillery version >nul 2>&1
if errorlevel 1 (
    echo ? Artillery não encontrado. Instalando...
    echo ?? Executando: npm install -g artillery
    npm install -g artillery
    if errorlevel 1 (
        echo ? Falha na instalação do Artillery
        pause
        exit /b 1
    )
    echo ? Artillery instalado com sucesso!
) else (
    echo ? Artillery encontrado
)

:: Verificar se a API está rodando
echo ?? Verificando API...
curl -k -s https://localhost:7283/health >nul 2>&1
if errorlevel 1 (
    echo ??  API não está respondendo em https://localhost:7283
    echo ?? Certifique-se de que a aplicação está rodando
    set /p continuar="Deseja continuar mesmo assim? (y/N): "
    if /i not "!continuar!"=="y" exit /b 1
) else (
    echo ? API está respondendo
)

:: Executar testes
echo.
echo ?? Iniciando testes de performance...
echo ??  Este processo pode levar alguns minutos...
echo.

set timestamp=%date:~-4%%date:~3,2%%date:~0,2%_%time:~0,2%%time:~3,2%%time:~6,2%
set timestamp=%timestamp: =0%
set reportfile=performance-report-%timestamp%.json

artillery run teste-performance-simples.yml --output %reportfile%

if errorlevel 1 (
    echo ? Falha na execução dos testes
    pause
    exit /b 1
) else (
    echo ? Testes executados com sucesso!
    echo ?? Relatório gerado: %reportfile%
    
    :: Gerar relatório HTML
    echo ?? Gerando relatório HTML...
    artillery report %reportfile% --output performance-report-%timestamp%.html
    if not errorlevel 1 (
        echo ? Relatório HTML gerado: performance-report-%timestamp%.html
    )
)

echo.
echo ?? Testes concluídos!
echo.
pause