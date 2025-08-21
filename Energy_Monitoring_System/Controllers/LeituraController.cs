using Azure.Core;
using Energy_Monitoring_System.Application.Validators;
using Energy_Monitoring_System.Domain.DTOs;
using Energy_Monitoring_System.Domain.Interfaces;
using Energy_Monitoring_System.Domain.Models;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Net;

namespace Energy_Monitoring_System.Controllers;

[ApiController]
[Route("api/medidores")]
public class LeituraController : ControllerBase
{
    private readonly ILeituraRepository _repository;
    private readonly LeituraInputDtoValidator _validator;
    private readonly ILogger<LeituraController> _logger;

    public LeituraController(
        ILeituraRepository repository,
        LeituraInputDtoValidator validator,
        ILogger<LeituraController> logger)
    {
        _repository = repository;
        _validator = validator;
        _logger = logger;
    }

    [HttpPost("{medidorId}/leituras")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PostLeitura(string medidorId, LeituraInputDto leituraDto)
    {
        try
        {
            _logger.LogInformation("Iniciando processamento de nova leitura para o medidor {MedidorId}", medidorId);

            ValidationResult validationResult = await _validator.ValidateAsync(leituraDto);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validação falhou para leitura do medidor {MedidorId}. Erros: {@Errors}", 
                    medidorId, validationResult.Errors);
                return BadRequest(validationResult.Errors);
            }

            var leitura = new Leitura
            {
                MedidorId = medidorId,
                Timestamp = leituraDto.Timestamp,
                Tensao = leituraDto.Tensao,
                Corrente = leituraDto.Corrente,
                PotenciaAtiva = leituraDto.PotenciaAtiva,
                PotenciaReativa = leituraDto.PotenciaReativa,
                EnergiaAtivaDireta = leituraDto.EnergiaAtivaDireta,
                EnergiaAtivaReversa = leituraDto.EnergiaAtivaReversa,
                FatorPotencia = leituraDto.FatorPotencia,
                Frequencia = leituraDto.Frequencia
            };

            await _repository.AddLeituraAsync(leitura);
            _logger.LogInformation("Leitura registrada com sucesso para o medidor {MedidorId}", medidorId);

            return CreatedAtAction(nameof(GetLeituras), new { medidorId }, leitura);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar leitura para o medidor {MedidorId}", medidorId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao processar a leitura");
        }
    }

    [HttpGet("{medidorId}/leituras")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LeituraResponseDto>> GetLeituras([FromBody] LeituraRequestDto leituraRequestDto)
    {
        try
        {
            _logger.LogInformation("Iniciando busca de leituras para o medidor {MedidorId}. Período: {DataInicio} - {DataFim}", 
                leituraRequestDto.MedidorId, leituraRequestDto.Periodo.Inicio, leituraRequestDto.Periodo.Fim);

            if (leituraRequestDto.Periodo.Inicio > leituraRequestDto.Periodo.Fim)
            {
                _logger.LogWarning("Requisição inválida: Data início maior que data fim. MedidorId: {MedidorId}", 
                    leituraRequestDto.MedidorId);
                return BadRequest("Data início deve ser menor ou igual à data fim");
            }

            if (leituraRequestDto.Limite <= 0 || leituraRequestDto.Limite > 1000)
            {
                _logger.LogInformation("Ajustando limite de registros para medidor {MedidorId}. Valor original: {LimiteOriginal}", 
                    leituraRequestDto.MedidorId, leituraRequestDto.Limite);
                leituraRequestDto.Limite = leituraRequestDto.Limite <= 0 ? 100 : 1000;
            }

            var leituras = await _repository.GetLeiturasAsync(
                leituraRequestDto.MedidorId, 
                leituraRequestDto.Periodo.Inicio, 
                leituraRequestDto.Periodo.Fim, 
                leituraRequestDto.Limite);
            
            var leiturasList = leituras.ToList();

            var response = new LeituraResponseDto
            {
                MedidorId = leituraRequestDto.MedidorId,
                Periodo = new PeriodoDto
                {
                    Inicio = leituraRequestDto.Periodo.Inicio,
                    Fim = leituraRequestDto.Periodo.Fim
                },
                TotalRegistros = leiturasList.Count,
                Leituras = leiturasList.Select(l => new LeituraDto
                {
                    Timestamp = l.Timestamp,
                    Tensao = l.Tensao,
                    Corrente = l.Corrente,
                    PotenciaAtiva = l.PotenciaAtiva,
                    PotenciaReativa = l.PotenciaReativa,
                    EnergiaAtivaDireta = l.EnergiaAtivaDireta,
                    EnergiaAtivaReversa = l.EnergiaAtivaReversa,
                    FatorPotencia = l.FatorPotencia,
                    Frequencia = l.Frequencia
                })
            };

            _logger.LogInformation("Busca concluída para o medidor {MedidorId}. Total de registros: {TotalRegistros}", 
                leituraRequestDto.MedidorId, response.TotalRegistros);

            return CreatedAtAction(nameof(GetLeituras), new { leituraRequestDto.MedidorId }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar leituras para o medidor {MedidorId}", leituraRequestDto.MedidorId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Ocorreu um erro ao buscar as leituras");
        }
    }
}