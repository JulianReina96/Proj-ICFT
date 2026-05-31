using System;
using System.Collections.Generic;
using Proj_ICFT.ModelsNew;

namespace Proj_ICFT.Models.ViewModels;

public class EvolucaoListagemViewModel
{
    public int Id { get; set; }
    public DateTime DataConsulta { get; set; }
    public StatusEvolucao Status { get; set; }
    public string StatusLabel { get; set; } = "";
    public int? ReceitaID { get; set; }
    public double? IctDaReceita { get; set; }
    public bool? ReceitaAdesao { get; set; }
    public string? CidCodigo { get; set; }
    public string? CidTitulo { get; set; }
}

public class EvolucaoDetalheViewModel : EvolucaoListagemViewModel
{
    public string? Subjetivo { get; set; }
    public string? Avaliacao { get; set; }
    public string? Plano { get; set; }
    public string? ObservacaoObjetivo { get; set; }
    public int? CategoriaCID_ID { get; set; }

    public int? PaSistolica { get; set; }
    public int? PaDiastolica { get; set; }
    public int? FrequenciaCardiaca { get; set; }
    public double? Peso { get; set; }
    public int? Spo2 { get; set; }
    public double? Glicemia { get; set; }
    public double? Hba1c { get; set; }
    public double? Creatinina { get; set; }
    public int EventosAdversos { get; set; }
    public int Hospitalizacoes { get; set; }
    public int IdasEmergencia { get; set; }
}

public class CIDOpcaoViewModel
{
    public int Id { get; set; }
    public string Codigo { get; set; } = "";
    public string Titulo { get; set; } = "";
}

public class ReceitaOpcaoViewModel
{
    public int Id { get; set; }
    public DateTime DataCriacao { get; set; }
    public double Ict { get; set; }
}

public class SerieTemporalViewModel
{
    public List<DateTime> Datas { get; set; } = new();
    public List<double?> IctPorEvolucao { get; set; } = new();
    public List<int> StatusPorEvolucao { get; set; } = new();

    public List<int?> PaSistolica { get; set; } = new();
    public List<int?> PaDiastolica { get; set; } = new();
    public List<int?> FrequenciaCardiaca { get; set; } = new();
    public List<double?> Peso { get; set; } = new();
    public List<int?> Spo2 { get; set; } = new();

    public List<double?> Glicemia { get; set; } = new();
    public List<double?> Hba1c { get; set; } = new();
    public List<double?> Creatinina { get; set; } = new();

    public List<int> EventosAdversos { get; set; } = new();
    public List<int> Hospitalizacoes { get; set; } = new();
    public List<int> IdasEmergencia { get; set; } = new();
}
