using Energy_Monitoring_System.Domain.DTOs;
using FluentValidation;

namespace Energy_Monitoring_System.Application.Validators;

public class LeituraInputDtoValidator : AbstractValidator<LeituraInputDto>
{
    public LeituraInputDtoValidator()
    {
        RuleFor(x => x.Timestamp)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Timestamp não pode ser no futuro");

        RuleFor(x => x.Tensao)
            .InclusiveBetween(0m, 1000m)
            .WithMessage("Tensão deve estar entre 0 e 1000V");

        RuleFor(x => x.Corrente)
            .InclusiveBetween(0m, 1000m)
            .WithMessage("Corrente deve estar entre 0 e 1000A");

        RuleFor(x => x.PotenciaAtiva)
            .InclusiveBetween(0m, 100000m)
            .WithMessage("Potência Ativa deve estar entre 0 e 100kW");

        RuleFor(x => x.PotenciaReativa)
            .InclusiveBetween(0m, 50000m)
            .WithMessage("Potência Reativa deve estar entre 0 e 50kVAr");

        RuleFor(x => x.EnergiaAtivaDireta)
            .GreaterThanOrEqualTo(0m)
            .LessThanOrEqualTo(999999999.99m)
            .WithMessage("Energia Ativa Direta deve estar entre 0 e 999.999.999,99 kWh");

        RuleFor(x => x.EnergiaAtivaReversa)
            .GreaterThanOrEqualTo(0m)
            .LessThanOrEqualTo(999999999.99m)
            .WithMessage("Energia Ativa Reversa deve estar entre 0 e 999.999.999,99 kWh");

        RuleFor(x => x.FatorPotencia)
            .InclusiveBetween(0m, 1m)
            .WithMessage("Fator de Potência deve estar entre 0 e 1");

        RuleFor(x => x.Frequencia)
            .InclusiveBetween(58m, 62m)
            .WithMessage("Frequência deve estar entre 58 e 62Hz");
    }
}