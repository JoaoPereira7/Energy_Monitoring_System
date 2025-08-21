using Energy_Monitoring_System.Application.Validators;
using Energy_Monitoring_System.Controllers;
using Energy_Monitoring_System.Domain.DTOs;
using Energy_Monitoring_System.Domain.Interfaces;
using Energy_Monitoring_System.Domain.Models;
using FluentAssertions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Energy_Monitoring_System.Tests.Controllers;

public class LeituraControllerTests
{
    private readonly Mock<ILeituraRepository> _repositoryMock;
    private readonly LeituraInputDtoValidator _validator;
    private readonly Mock<ILogger<LeituraController>> _loggerMock;
    private readonly LeituraController _controller;

    public LeituraControllerTests()
    {
        _repositoryMock = new Mock<ILeituraRepository>();
        _validator = new LeituraInputDtoValidator();
        _loggerMock = new Mock<ILogger<LeituraController>>();
        _controller = new LeituraController(_repositoryMock.Object, _validator, _loggerMock.Object);
    }

    [Fact]
    public async Task PostLeitura_QuandoDadosValidos_DeveRetornarCreated()
    {
        // Arrange
        var medidorId = "MED001";
        var leituraDto = new LeituraInputDto
        {
            Timestamp = DateTime.UtcNow.AddMinutes(-1),
            Tensao = 220,
            Corrente = 10,
            PotenciaAtiva = 2200,
            PotenciaReativa = 0,
            EnergiaAtivaDireta = 1000,
            EnergiaAtivaReversa = 0,
            FatorPotencia = 1,
            Frequencia = 60
        };

        _repositoryMock.Setup(r => r.AddLeituraAsync(It.IsAny<Leitura>()))
            .ReturnsAsync((Leitura l) => l);

        // Act
        var result = await _controller.PostLeitura(medidorId, leituraDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        createdResult.ActionName.Should().Be(nameof(LeituraController.GetLeituras));
        createdResult.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task PostLeitura_QuandoDadosInvalidos_DeveRetornarBadRequest()
    {
        // Arrange
        var medidorId = "MED001";
        var leituraDto = new LeituraInputDto(); // Todos os campos default, inválido

        // Act
        var result = await _controller.PostLeitura(medidorId, leituraDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task GetLeituras_QuandoExistemDados_DeveRetornarOk()
    {
        // Arrange
        var request = new LeituraRequestDto
        {
            MedidorId = "MED001",
            Periodo = new PeriodoDto
            {
                Inicio = DateTime.UtcNow.AddHours(-1),
                Fim = DateTime.UtcNow
            },
            Limite = 10
        };

        var leituras = new List<Leitura>
        {
            new Leitura
            {
                Id = 1,
                MedidorId = "MED001",
                Timestamp = DateTime.UtcNow.AddMinutes(-30),
                Tensao = 220,
                Corrente = 10
            }
        };

        _repositoryMock.Setup(r => r.GetLeiturasAsync(
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<int>()))
            .ReturnsAsync(leituras);

        // Act
        var result = await _controller.GetLeituras(request);

        // Assert
        var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<LeituraResponseDto>(actionResult.Value);
        response.TotalRegistros.Should().Be(1);
        response.MedidorId.Should().Be("MED001");
    }

    [Fact]
    public async Task GetLeituras_QuandoPeriodoInvalido_DeveRetornarBadRequest()
    {
        // Arrange
        var request = new LeituraRequestDto
        {
            MedidorId = "MED001",
            Periodo = new PeriodoDto
            {
                Inicio = DateTime.UtcNow,
                Fim = DateTime.UtcNow.AddHours(-1) // Fim antes do início
            },
            Limite = 10
        };

        // Act
        var result = await _controller.GetLeituras(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        badRequestResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task GetLeituras_QuandoRepositoryLancaExcecao_DeveRetornarInternalServerError()
    {
        // Arrange
        var request = new LeituraRequestDto
        {
            MedidorId = "MED001",
            Periodo = new PeriodoDto
            {
                Inicio = DateTime.UtcNow.AddHours(-1),
                Fim = DateTime.UtcNow
            },
            Limite = 10
        };

        _repositoryMock.Setup(r => r.GetLeiturasAsync(
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<int>()))
            .ThrowsAsync(new Exception("Erro simulado"));

        // Act
        var result = await _controller.GetLeituras(request);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        statusCodeResult.StatusCode.Should().Be(500);
    }
}