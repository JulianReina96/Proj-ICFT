using System;
using Proj_ICFT.ModelsNew;

namespace Proj_ICFT.Models.Request;

public record SalvarEvolucaoRequest(
    int PacienteID,
    int? PrescricaoID,
    int? CategoriaCID_ID,
    DateTime DataConsulta,
    StatusEvolucao Status,

    string? Subjetivo,
    string? Avaliacao,
    string? Plano,
    string? ObservacaoObjetivo,

    int? PaSistolica,
    int? PaDiastolica,
    int? FrequenciaCardiaca,
    double? Peso,
    int? Spo2,

    double? Glicemia,
    double? Hba1c,
    double? Creatinina,

    int EventosAdversos,
    int Hospitalizacoes,
    int IdasEmergencia
);
