# Mudança no Cálculo do ICT

## A fórmula correta — MRCI (Medication Regimen Complexity Index)

O ICT implementado neste sistema segue a definição do **MRCI**, que divide o cálculo em três seções com regras distintas de contagem:

| Componente       | Regra de contagem                                              |
|------------------|----------------------------------------------------------------|
| **Tipo/Forma**   | Uma vez por **forma distinta** em toda a receita               |
| **Frequência**   | Uma vez por **medicamento**                                    |
| **Instruções**   | Uma vez por **instrução por medicamento**                      |

```
ICT = Σ(Tipo.Peso por forma distinta na receita)
    + Σ(Frequencia.Peso por medicamento)
    + Σ(Σ Instrucao.Peso por medicamento)
```

A regra do Tipo é intencional: dois comprimidos orais diferentes não duplicam o peso da forma — a presença daquela forma na receita é o que importa, não quantos medicamentos a compartilham.

---

## Versão 1 — `ExportarDados` (código original, nunca persistia)

```csharp
// HomeController.cs — ExportarDados
var remedios  = await _formularioServices.converterMedicamentos(dados.medicamentos);

double pesoTotal = remedios.Sum(r => r.PesoTotal);          // (A)

var subcategorias = remedios
    .Select(r => r.subcategoria)
    .DistinctBy(c => c.id)                                  // (B) DistinctBy intencional
    .ToList();

pesoTotal = pesoTotal + subcategorias.Sum(s => s.Peso);     // (C)
```

```csharp
// RemedioViewModel.cs
public double PesoTotal => frequencia.Peso + instrucoes.Sum(i => i.Peso);
// ↑ Frequência + Instruções por medicamento — sem Tipo.Peso
```

### O que estava certo

- **(B)** `DistinctBy(tipo.id)` estava **correto** segundo o MRCI: conta o Tipo uma vez por forma distinta.
- **(A)** Frequência e Instruções somadas por medicamento via `RemedioViewModel.PesoTotal` — também correto.

### O que estava errado

- **Nunca persistia no banco.** O método retornava `Ok(pesoTotal)` e descartava tudo.
- `MedicamentoID` nunca foi enviado ao backend — o DTO não tinha o campo, então não era possível gravar `ReceitaMed.MedicamentoID`.
- A arquitetura misturava responsabilidades: Controller calculava ICT em vez do Service.

---

## Versão 2 — `SalvarReceita` implementação inicial (introduziu bug)

Na refatoração para implementar a persistência, a regra do Tipo foi reescrita erroneamente:

```csharp
// FormularioServices.cs — SalvarReceita (versão com bug)
foreach (var med in request.Medicamentos)
{
    tipos.TryGetValue(med.SubcategoriaId, out var tipo);
    frequencias.TryGetValue(med.FrequenciaId, out var freq);
    // ...

    // BUG: soma Tipo.Peso por medicamento, não por forma distinta
    ict += (tipo?.Peso ?? 0) + (freq?.Peso ?? 0.0) + instrsDoMed.Sum(i => i.Peso);
}
```

### O que estava errado

O `DistinctBy` foi removido com a justificativa de "corrigir" a deduplicação, mas a deduplicação era **intencional** pela definição do MRCI. Isso gerava ICT **inflado** quando a receita tinha dois ou mais medicamentos com a mesma forma de dosagem.

### Exemplo do erro

Receita com Losartana (comprimido oral) + Atorvastatina (comprimido oral), sem instruções:

| Componente        | MRCI correto | Versão 2 (com bug) |
|-------------------|--------------|--------------------|
| Tipo "comp. oral" (Peso = 1) | +1 (uma vez) | +2 (por med) |
| Freq. Losartana (Peso = 2)   | +2           | +2           |
| Freq. Atorvastatina (Peso = 1) | +1         | +1           |
| **ICT total**     | **4**        | **5** ← errado     |

---

## Versão 3 — `SalvarReceita` corrigida (atual)

```csharp
// FormularioServices.cs — SalvarReceita (versão atual)

// Tipo/Forma: Distinct() garante que cada forma seja somada uma única vez,
// independentemente de quantos medicamentos a compartilhem.
double ict = request.Medicamentos
    .Select(m => m.SubcategoriaId)
    .Distinct()
    .Sum(id => tipos.TryGetValue(id, out var t) ? (double)t.Peso : 0);

// Frequência e Instruções: somadas por medicamento.
foreach (var med in request.Medicamentos)
{
    frequencias.TryGetValue(med.FrequenciaId, out var freq);

    var instrsDoMed = med.InstrucoesAdicionais
        .Where(instrucoes.ContainsKey)
        .Select(id => instrucoes[id])
        .ToList();

    ict += (freq?.Peso ?? 0.0) + instrsDoMed.Sum(i => i.Peso);
}
```

### O que mudou em relação à Versão 1

| Aspecto                        | Versão 1 (original)                    | Versão 3 (atual)                              |
|-------------------------------|----------------------------------------|-----------------------------------------------|
| **Tipo.Peso**                 | DistinctBy correto, mas fora do Service| `Distinct()` no Service, mesmo comportamento  |
| **Frequência + Instruções**   | Por medicamento via ViewModel          | Por medicamento diretamente no Service        |
| **Persistência**              | Nenhuma                                | `Receitum`, `ReceitaMed`, `InstrucoesMed`, `ReceitaCID` |
| **MedicamentoID gravado**     | Não (campo ausente no DTO)             | Sim (`ReceitaMed.MedicamentoID`)              |
| **Arquitetura**               | Cálculo no Controller                  | Cálculo encapsulado no Service                |

### Exemplo com a versão corrigida

Mesma receita: Losartana (comprimido, Peso=1, Freq=2) + Atorvastatina (comprimido, Peso=1, Freq=1):

```
Tipo distinto "comprimido oral" → +1
Frequência Losartana            → +2
Frequência Atorvastatina        → +1
─────────────────────────────────────
ICT = 4  ✓
```

---

## Resumo das três versões

| Versão | Tipo correto? | Freq/Instrucoes correto? | Persiste? |
|--------|:---:|:---:|:---:|
| 1 — `ExportarDados` (original) | ✅ | ✅ | ❌ |
| 2 — `SalvarReceita` (bug introduzido) | ❌ | ✅ | ✅ |
| 3 — `SalvarReceita` (atual) | ✅ | ✅ | ✅ |
