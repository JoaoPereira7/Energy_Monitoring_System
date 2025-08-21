namespace Energy_Monitoring_System.Domain.DTOs;

public class LeituraInputDto
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

public class LeituraResponseDto
{
    public string MedidorId { get; set; } = null!;
    public PeriodoDto Periodo { get; set; } = null!;
    public int TotalRegistros { get; set; }
    public IEnumerable<LeituraDto> Leituras { get; set; } = null!;
}

public class LeituraRequestDto
{
    public string MedidorId { get; set; }
    public PeriodoDto Periodo { get; set; }
    public int Limite { get; set; }
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