using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Energy_Monitoring_Tests;

public class LeituraEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public class LeituraPayload
    {
        public DateTime Timestamp { get; set; }
        public decimal Tensao { get; set; }
        public decimal Corrente { get; set; }
        public decimal PotenciaAtiva { get; set; }
        public decimal PotenciaReativa { get; set; }
        public decimal EnergiaAtivaDireta { get; set; }
        public decimal EnergiaAtivaReversa { get; set; }
        public decimal FatorPotencia { get; set; }
        public decimal Frequencia { get; set; }
    }

    public class ConsultaResposta
    {
        public string MedidorId { get; set; }
        public PeriodoDto Periodo { get; set; }
        public int TotalRegistros { get; set; }
        public LeituraDto[] Leituras { get; set; }
    }
    public class PeriodoDto
    {
        public DateTime Inicio { get; set; }
        public DateTime Fim { get; set; }
    }
    public class LeituraDto
    {
        public DateTime Timestamp { get; set; }
        public decimal Tensao { get; set; }
        public decimal Corrente { get; set; }
        public decimal PotenciaAtiva { get; set; }
        public decimal PotenciaReativa { get; set; }
        public decimal EnergiaAtivaDireta { get; set; }
        public decimal EnergiaAtivaReversa { get; set; }
        public decimal FatorPotencia { get; set; }
        public decimal Frequencia { get; set; }
    }

    [Fact]
    public async Task PostLeitura_DeveRetornar201_QuandoPayloadValido()
    {
        var payload = new LeituraPayload
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
        var response = await _client.PostAsJsonAsync("/api/medidores/MED001/leituras", payload);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<dynamic>();
        ((int)body.id).Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task PostLeitura_DeveRetornar400_QuandoTimestampFuturo()
    {
        var payload = new LeituraPayload
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
        var response = await _client.PostAsJsonAsync("/api/medidores/MED001/leituras", payload);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetLeituras_DeveRetornar200_QuandoExistemLeituras()
    {
        var payload = new LeituraPayload
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
        await _client.PostAsJsonAsync("/api/medidores/MED002/leituras", payload);
        var inicio = DateTime.UtcNow.AddMinutes(-10).ToString("O");
        var fim = DateTime.UtcNow.ToString("O");
        var response = await _client.GetAsync($"/api/medidores/MED002/leituras?dataInicio={inicio}&dataFim={fim}&limite=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ConsultaResposta>();
        result.Should().NotBeNull();
        result!.MedidorId.Should().Be("MED002");
        result.Leituras.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetLeituras_DeveRetornar400_QuandoParametrosInvalidos()
    {
        var response = await _client.GetAsync("/api/medidores/MED003/leituras");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
