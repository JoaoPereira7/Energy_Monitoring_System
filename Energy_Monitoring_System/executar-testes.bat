@echo off
setlocal enabledelayedexpansion

echo.
echo ?? Testes de Performance - Energy Monitoring System
echo ===================================================
echo.

:: Verificar se Node.js est� instalado
echo ?? Verificando Node.js...
node --version >nul 2>&1
if errorlevel 1 (
    echo ? Node.js n�o encontrado. Instale em: https://nodejs.org/
    pause
    exit /b 1
) else (
    echo ? Node.js encontrado
)

:: Verificar se Artillery est� instalado
echo ?? Verificando Artillery...
artillery version >nul 2>&1
if errorlevel 1 (
    echo ? Artillery n�o encontrado. Instalando...
    echo ?? Executando: npm install -g artillery
    npm install -g artillery
    if errorlevel 1 (
        echo ? Falha na instala��o do Artillery
        pause
        exit /b 1
    )
    echo ? Artillery instalado com sucesso!
) else (
    echo ? Artillery encontrado
)

:: Verificar se a API est� rodando
echo ?? Verificando API...
curl -k -s https://localhost:7283/health >nul 2>&1
if errorlevel 1 (
    echo ??  API n�o est� respondendo em https://localhost:7283
    echo ?? Certifique-se de que a aplica��o est� rodando
    set /p continuar="Deseja continuar mesmo assim? (y/N): "
    if /i not "!continuar!"=="y" exit /b 1
) else (
    echo ? API est� respondendo
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
    echo ? Falha na execu��o dos testes
    pause
    exit /b 1
) else (
    echo ? Testes executados com sucesso!
    echo ?? Relat�rio gerado: %reportfile%
    
    :: Gerar relat�rio HTML
    echo ?? Gerando relat�rio HTML...
    artillery report %reportfile% --output performance-report-%timestamp%.html
    if not errorlevel 1 (
        echo ? Relat�rio HTML gerado: performance-report-%timestamp%.html
    )
)

echo.
echo ?? Testes conclu�dos!
echo.
pause