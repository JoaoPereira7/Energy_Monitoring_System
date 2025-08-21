using Energy_Monitoring_System.Domain.Models;

namespace Energy_Monitoring_System.Domain.Interfaces;

public interface ILeituraRepository
{
    Task<Leitura> AddLeituraAsync(Leitura leitura);
    Task<IEnumerable<Leitura>> GetLeiturasAsync(string medidorId, DateTime dataInicio, DateTime dataFim, int limite);
}