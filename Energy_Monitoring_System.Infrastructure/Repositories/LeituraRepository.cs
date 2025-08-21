using Energy_Monitoring_System.Domain.Interfaces;
using Energy_Monitoring_System.Domain.Models;
using Energy_Monitoring_System.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Energy_Monitoring_System.Infrastructure.Repositories;

public class LeituraRepository : ILeituraRepository
{
    private readonly ApplicationDbContext _context;

    public LeituraRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Leitura> AddLeituraAsync(Leitura leitura)
    {
        _context.Leituras.Add(leitura);
        await _context.SaveChangesAsync();
        return leitura;
    }

    public async Task<IEnumerable<Leitura>> GetLeiturasAsync(string medidorId, DateTime dataInicio, DateTime dataFim, int limite = 100)
    {
        return await _context.Leituras
            .Where(l => l.MedidorId == medidorId && 
                       l.Timestamp >= dataInicio && 
                       l.Timestamp <= dataFim)
            .OrderByDescending(l => l.Timestamp)
            .Take(Math.Min(limite, 1000))
            .ToListAsync();
    }
}