using System.ComponentModel.DataAnnotations;

namespace Energy_Monitoring_System.Domain.Models;

public class Leitura
{
    public int Id { get; set; }
    public string MedidorId { get; set; } = null!;
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