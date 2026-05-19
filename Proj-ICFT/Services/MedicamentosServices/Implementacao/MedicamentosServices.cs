using Microsoft.EntityFrameworkCore;
using Proj_ICFT.DataNew;
using Proj_ICFT.ModelsNew;
using Proj_ICFT.Services.MedicamentosServices.Interface;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Proj_ICFT.MedicamentosServices.Implementacao
{
    public class MedicamentosServices : IMedicamentoService
    {
        private static readonly string[] StatusValidos = ["ATIVO", "VÁLIDO"];
        private static readonly Regex RegraSeparadorPlus = new(@"\s*\+\s*", RegexOptions.Compiled);
        private static readonly Regex RegraEspacos = new(@"\s+", RegexOptions.Compiled);

        private readonly AppDbContextNew _appDbContextNew;

        public MedicamentosServices(AppDbContextNew appDbContextNew)
        {
            _appDbContextNew = appDbContextNew;
        }

        // Normaliza para deduplicação: uppercase, sem acentos, espaços consistentes, + padronizado.
        // Uso de Min(Id) no agrupamento garante que a mesma grafia normalizada sempre resolve
        // para o mesmo registro do banco — nunca troca o Id entre consultas.
        private static string NormalizarNome(string nome)
        {
            var upper = nome.ToUpperInvariant();

            var decomposto = upper.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(decomposto.Length);
            foreach (var c in decomposto)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            var semAcento = sb.ToString().Normalize(NormalizationForm.FormC);
            var comSeparador = RegraSeparadorPlus.Replace(semAcento, " + ");
            return RegraEspacos.Replace(comSeparador, " ").Trim();
        }

        public async Task<List<Medicamento>> listarMedicamentos()
        {
            return await _appDbContextNew.Medicamentos.ToListAsync();
        }

        public async Task<List<Medicamento>> ListarMedicamentosUnique()
        {
            return await _appDbContextNew.Medicamentos
                .Where(x => !string.IsNullOrWhiteSpace(x.NOME_PRODUTO)
                         && x.SITUACAO_REGISTRO != null
                         && StatusValidos.Contains(x.SITUACAO_REGISTRO.ToUpper()))
                .OrderBy(x => x.NOME_PRODUTO)
                .GroupBy(x => x.NOME_PRODUTO.Trim().ToUpper())
                .Select(g => g.FirstOrDefault())
                .ToListAsync();
        }

        public async Task<(List<Medicamento> Items, bool HasMore)> BuscarMedicamentos(string? q, int page, int pageSize)
        {
            // Variável local garante captura correta pelo EF Core como parâmetro SQL.
            var statusValidos = new[] { "ATIVO", "VÁLIDO" };

            var query = _appDbContextNew.Medicamentos
                .Where(x => x.NOME_PRODUTO != null
                         && x.SITUACAO_REGISTRO != null
                         && statusValidos.Contains(x.SITUACAO_REGISTRO.ToUpper()));

            if (!string.IsNullOrWhiteSpace(q))
            {
                var pattern = $"%{q.ToUpperInvariant()}%";
                query = query.Where(x => EF.Functions.Like(x.NOME_PRODUTO!, pattern));
            }

            // Carrega apenas os campos necessários para a normalização em memória.
            // O filtro q (nome ou princípio ativo) limita o volume antes de carregar.
            var registros = await query
                .Select(x => new { x.Id, x.NOME_PRODUTO, x.PRINCIPIO_ATIVO, x.CATEGORIA_REGULATORIA })
                .ToListAsync();

            // Chave de agrupamento: NOME_PRODUTO normalizado + CATEGORIA_REGULATORIA normalizada.
            // Preserva o nome comercial (ACEFLOR ≠ ACECLOFENACO) e distingue Genérico de Similar.
            // Min(Id) garante que a mesma chave sempre resolve para o mesmo Id entre consultas.
            var unicos = registros
                .GroupBy(x =>
                    NormalizarNome(x.NOME_PRODUTO!) + "|" +
                    NormalizarNome(x.CATEGORIA_REGULATORIA ?? ""))
                .Where(g => !string.IsNullOrWhiteSpace(g.Key))
                .Select(g =>
                {
                    var rep = g.MinBy(x => x.Id)!;
                    var nomeNorm = NormalizarNome(rep.NOME_PRODUTO!);
                    var catNorm  = NormalizarNome(rep.CATEGORIA_REGULATORIA ?? "");
                    var display  = string.IsNullOrWhiteSpace(catNorm)
                        ? nomeNorm
                        : $"{nomeNorm} ({catNorm})";

                    return new Medicamento { Id = rep.Id, NOME_PRODUTO = display };
                })
                .OrderBy(x => x.NOME_PRODUTO)
                .ToList();

            bool hasMore = (page * pageSize) < unicos.Count;
            var items = unicos
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (items, hasMore);
        }

        //public async Task<List<Categoria>> listarCategorias()
        //{

        //    return await _categoriaDbContext.Categoria.Include(x => x.tipos).OrderBy(c => c.Name).ToListAsync();

        //}

        //public async Task<List<Frequencia>> listarFrequencias()
        //{
        //    return await _frequenciaDbContext.Frequencia.OrderBy(f => f.Name).ToListAsync();
        //}

        //public async Task<List<InstrucoesAdicionais>> listarInstrucoesAdicionais()
        //{
        //    return await _instrucoesAdicionaisDbContext.InstrucoesAdicionais.OrderBy(i => i.name).ToListAsync();
        //}

        //public async Task<List<Tipo>> listarTipoByCategoriaId(int id)
        //{

        //    return await _tipoDbContext.Tipo.Where(s => s.CategoriaId == id).OrderBy(t => t.Name).ToListAsync();

        //}

        //public async Task<List<RemedioViewModel>> converterMedicamentos(List<MedicamentosExportacaoModel> listaModel)
        //{
        //    var remedios = new List<RemedioViewModel>();

        //    foreach (var model in listaModel)
        //    {
        //        var categoria = await _categoriaDbContext.Categoria.FindAsync(model.categoria);
        //        var subcategoria = await _tipoDbContext.Tipo.FindAsync(model.subcategoria);
        //        var frequencia = await _frequenciaDbContext.Frequencia.FindAsync(model.frequencia);
        //        List<InstrucoesAdicionais> instrucoes = await _instrucoesAdicionaisDbContext.InstrucoesAdicionais.Where(i => model.instrucoesAdicionais.Contains(i.id)).ToListAsync();

        //        //cadastrar entidade de remedio pra inserir no banco pra buscar e salvar

        //        var remedio = new RemedioViewModel(categoria, subcategoria, frequencia, instrucoes);

        //        remedios.Add(remedio);


        //    }
        //    return remedios;
        //}
    }
}
