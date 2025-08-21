using System.Net;
using System.Net.Http.Json;
using Energy_Monitoring_System.Domain.DTOs;
using Energy_Monitoring_System.Tests.Fixtures;
using FluentAssertions;
using Xunit;

namespace Energy_Monitoring_System.Tests.Integration;

public class LeituraEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public LeituraEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostLeitura_DeveRetornar201_QuandoPayloadValido()
    {
        // Arrange
        var payload = new LeituraInputDto
        {
            Timestamp = DateTime.UtcNow.AddMinutes(-1),
            Tensao = 220.5m,
            Corrente = 15.2m,
            PotenciaAtiva = 3351.6m,
            PotenciaReativa = 892.4m,
            EnergiaAtivaDireta = 1250.75m,
            EnergiaAtivaReversa = 32.18m,
            FatorPotencia = 0.97m,
            Frequencia = 60.1m
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/medidores/MED001/leituras", payload);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var leitura = await response.Content.ReadFromJsonAsync<LeituraDto>();
        leitura.Should().NotBeNull();
        leitura!.Tensao.Should().Be(payload.Tensao);
    }

    [Fact]
    public async Task GetLeituras_DeveRetornar200_QuandoExistemLeituras()
    {
        // Arrange
        var leitura = new LeituraInputDto
        {
            Timestamp = DateTime.UtcNow.AddMinutes(-2),
            Tensao = 220,
            Corrente = 10,
            PotenciaAtiva = 1000,
            PotenciaReativa = 500,
            EnergiaAtivaDireta = 500,
            EnergiaAtivaReversa = 20,
            FatorPotencia = 0.95m,
            Frequencia = 60
        };

        await _client.PostAsJsonAsync("/api/medidores/MED002/leituras", leitura);

        var request = new LeituraRequestDto
        {
            MedidorId = "MED002",
            Periodo = new PeriodoDto
            {
                Inicio = DateTime.UtcNow.AddMinutes(-10),
                Fim = DateTime.UtcNow
            },
            Limite = 10
        };

        // Act
        var response = await _client.GetFromJsonAsync<LeituraResponseDto>($"/api/medidores/{request.MedidorId}/leituras?dataInicio={request.Periodo.Inicio:o}&dataFim={request.Periodo.Fim:o}&limite={request.Limite}");

        // Assert
        response.Should().NotBeNull();
        response!.MedidorId.Should().Be("MED002");
        response.Leituras.Should().NotBeEmpty();
    }

    [Fact]
    public async Task PostLeitura_DeveRetornar400_QuandoTimestampFuturo()
    {
        // Arrange
        var payload = new LeituraInputDto
        {
            Timestamp = DateTime.UtcNow.AddMinutes(5),
            Tensao = 220,
            Corrente = 10,
            PotenciaAtiva = 1000,
            PotenciaReativa = 500,
            EnergiaAtivaDireta = 500,
            EnergiaAtivaReversa = 20,
            FatorPotencia = 0.95m,
            Frequencia = 60
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/medidores/MED001/leituras", payload);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
