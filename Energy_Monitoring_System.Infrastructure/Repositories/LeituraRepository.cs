using Energy_Monitoring_System.Domain.Interfaces;
using Energy_Monitoring_System.Domain.Models;
using Energy_Monitoring_System.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Energy_Monitoring_System.Infrastructure.Repositories;

public class LeituraRepository : ILeituraRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LeituraRepository> _logger;

    public LeituraRepository(ApplicationDbContext context, ILogger<LeituraRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Leitura> AddLeituraAsync(Leitura leitura)
    {
        try
        {
            _context.Leituras.Add(leitura);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Leitura adicionada com sucesso. MedidorId: {MedidorId}, Timestamp: {Timestamp}",
                leitura.MedidorId, leitura.Timestamp);
            return leitura;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao salvar leitura no banco de dados. MedidorId: {MedidorId}, Timestamp: {Timestamp}",
                leitura.MedidorId, leitura.Timestamp);
            throw new Exception("Erro ao salvar leitura no banco de dados", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao adicionar leitura. MedidorId: {MedidorId}, Timestamp: {Timestamp}",
                leitura.MedidorId, leitura.Timestamp);
            throw;
        }
    }

    public async Task<IEnumerable<Leitura>> GetLeiturasAsync(string medidorId, DateTime dataInicio, DateTime dataFim, int limite = 100)
    {
        try
        {
            var leituras = await _context.Leituras
                .Where(l => l.MedidorId == medidorId &&
                           l.Timestamp >= dataInicio &&
                           l.Timestamp <= dataFim)
                .OrderByDescending(l => l.Timestamp)
                .Take(Math.Min(limite, 1000))
                .ToListAsync();

            _logger.LogInformation("Leituras recuperadas com sucesso. MedidorId: {MedidorId}, Período: {DataInicio} - {DataFim}, Quantidade: {Quantidade}",
                medidorId, dataInicio, dataFim, leituras.Count);

            return leituras;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao recuperar leituras. MedidorId: {MedidorId}, Período: {DataInicio} - {DataFim}",
                medidorId, dataInicio, dataFim);
            throw;
        }
    }
}