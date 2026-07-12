namespace Proj_ICFT.Models.ViewModels;

public class RelatorioFiltroViewModel
{
    public DateTime? De { get; set; }
    public DateTime? Ate { get; set; }
}

public class RelatorioDataViewModel
{
    // KPIs
    public double AdesaoGeralPct { get; set; }
    public double IctMedio { get; set; }
    public int TotalHospitalizacoes { get; set; }
    public int TotalPacientes { get; set; }

    // Relatórios
    public List<AdesaoPacienteRow> AdesaoPorPaciente { get; set; } = new();
    public List<ComplexidadePacienteRow> ComplexidadePorPaciente { get; set; } = new();
    public List<DistribuicaoCategoriaRow> DistribuicaoPorCategoria { get; set; } = new();
    public List<PrescricoesPorPeriodoRow> PrescricoesPorPeriodo { get; set; } = new();
    public List<DiagnosticoFrequenteRow> DiagnosticosFrequentes { get; set; } = new();
    public List<CorrelacaoICTStatusRow> CorrelacaoICTStatus { get; set; } = new();
    public List<HospitalizacaoPacienteRow> HospitalizacoesPorPaciente { get; set; } = new();
    public List<EventoAdversoFaixaRow> EventosAdversosPorFaixa { get; set; } = new();
}

public record AdesaoPacienteRow(string Nome, int Total, int Aderiu, double Percentual);
public record ComplexidadePacienteRow(string Nome, double IctMedio, double IctMaximo, int TotalPrescricoes);
public record DistribuicaoCategoriaRow(string Categoria, int Quantidade);
public record PrescricoesPorPeriodoRow(string Periodo, int TotalPrescricoes, double IctMedio);
public record DiagnosticoFrequenteRow(string Codigo, string Titulo, int Ocorrencias);
public record CorrelacaoICTStatusRow(string Status, double IctMedio, int TotalEvolucoes);
public record HospitalizacaoPacienteRow(string Nome, int TotalHospitalizacoes, int TotalConsultas);
public record EventoAdversoFaixaRow(string Faixa, int TotalEventos, int TotalPrescricoes);
