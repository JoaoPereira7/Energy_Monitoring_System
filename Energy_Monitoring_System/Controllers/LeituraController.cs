using Azure.Core;
using Energy_Monitoring_System.Application.Validators;
using Energy_Monitoring_System.Domain.DTOs;
using Energy_Monitoring_System.Domain.Interfaces;
using Energy_Monitoring_System.Domain.Models;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

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
    public async Task<IActionResult> PostLeitura(string medidorId, LeituraInputDto leituraDto)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(leituraDto);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

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
        return CreatedAtAction(nameof(GetLeituras), new { medidorId }, leitura);
    }

    [HttpGet("{medidorId}/leituras")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LeituraResponseDto>> GetLeituras([FromBody] LeituraRequestDto leituraRequestDto)
    {
        if (leituraRequestDto.Periodo.Inicio > leituraRequestDto.Periodo.Fim)
            return BadRequest("Data início deve ser menor ou igual à data fim");

        if (leituraRequestDto.Limite <= 0 || leituraRequestDto.Limite > 1000)
            leituraRequestDto.Limite = leituraRequestDto.Limite <= 0 ? 100 : 1000;

        var leituras = await _repository.GetLeiturasAsync(leituraRequestDto.MedidorId, leituraRequestDto.Periodo.Inicio, leituraRequestDto.Periodo.Fim, leituraRequestDto.Limite);
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

        return CreatedAtAction(nameof(GetLeituras), new { leituraRequestDto.MedidorId }, response);
    }
}