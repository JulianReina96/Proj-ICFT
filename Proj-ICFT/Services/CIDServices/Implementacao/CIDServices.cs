using Microsoft.EntityFrameworkCore;
using Proj_ICFT.DataNew;
using Proj_ICFT.Models;
using Proj_ICFT.Models.ViewModels;
using Proj_ICFT.ModelsNew;
using Proj_ICFT.Services.CIDServices.Interface;
using Proj_ICFT.Services.FormularioServices.Interface;
using static System.Reflection.Metadata.BlobBuilder;

namespace Proj_ICFT.Services.FormularioServices.Implementacao
{
    public class CIDServices : ICIDServices
    {

       private readonly AppDbContextNew _appDbContextNew;

        public CIDServices(AppDbContextNew appDbContextNew)
        {
            _appDbContextNew = appDbContextNew;
        }


        private static string LimparTitulo(string? titulo) =>
            titulo?.TrimStart('-', ' ') ?? string.Empty;

        public async Task<List<Blocos_CID>> listarBlocosCID()
        {
            var blocos = await _appDbContextNew.Blocos_CIDs.ToListAsync();
            return blocos
                .Where(b => !string.IsNullOrWhiteSpace(LimparTitulo(b.Title)))
                .OrderBy(b => LimparTitulo(b.Title))
                .ToList();
        }

        public async Task<List<Capitulos_CID>> listarCapitulosCID()
        {
            return await _appDbContextNew.Capitulos_CIDs.OrderBy(c => c.ChapterNo).ToListAsync();
        }

        public async Task<List<Categorias_CID>> listarCategoriasCID()
        {
            return await _appDbContextNew.Categorias_CIDs.OrderBy(c => c.Code).ToListAsync();
        }

        public async Task<List<Categorias_CID>> listarCategoriasPorBloco(string blocoId)
        {
            return await _appDbContextNew.Categorias_CIDs
                .Where(c => c.BlockId == blocoId)
                .OrderBy(c => c.Code)
                .ToListAsync();
        }

        public async Task<List<GrupoEnfermidadesViewModel>> agruparEnfermidadesPorBloco()
        {
            var resultado = await _appDbContextNew.Blocos_CIDs
                .GroupJoin(
                    _appDbContextNew.Categorias_CIDs,
                    bloco => bloco.BlockId,
                    categoria => categoria.BlockId,
                    (bloco, categorias) => new GrupoEnfermidadesViewModel
                    {
                        Bloco = bloco,
                        Enfermidades = categorias.ToList()
                    })
                .ToListAsync();

            return resultado;
        }

        //    public async Task<List<Categoria>> listarCategorias()
        //    {

        //        return await _categoriaDbContext.Categoria.Include(x => x.tipos).OrderBy(c => c.Name).ToListAsync();

        //    }

        //    public async Task<List<Frequencia>> listarFrequencias()
        //    {
        //        return await _frequenciaDbContext.Frequencia.OrderBy(f => f.Name).ToListAsync();
        //    }

        //    public async Task<List<InstrucoesAdicionais>> listarInstrucoesAdicionais()
        //    {
        //        return await _instrucoesAdicionaisDbContext.InstrucoesAdicionais.OrderBy(i => i.name).ToListAsync();
        //    }

        //    public async Task<List<Tipo>> listarTipoByCategoriaId(int id)
        //    {

        //        return await _tipoDbContext.Tipo.Where(s => s.CategoriaId == id).OrderBy(t => t.Name).ToListAsync();

        //    }

        //    public async Task<List<RemedioViewModel>> converterMedicamentos(List<MedicamentosExportacaoModel> listaModel)
        //    {
        //        var remedios = new List<RemedioViewModel>();

        //        foreach (var model in listaModel)
        //        {
        //            var categoria = await _categoriaDbContext.Categoria.FindAsync(model.categoria);
        //            var subcategoria = await _tipoDbContext.Tipo.FindAsync(model.subcategoria);
        //            var frequencia = await _frequenciaDbContext.Frequencia.FindAsync(model.frequencia);
        //            List<InstrucoesAdicionais> instrucoes = await _instrucoesAdicionaisDbContext.InstrucoesAdicionais.Where(i => model.instrucoesAdicionais.Contains(i.id)).ToListAsync();

        //            //cadastrar entidade de remedio pra inserir no banco pra buscar e salvar

        //            var remedio = new RemedioViewModel(categoria, subcategoria, frequencia, instrucoes);

        //            remedios.Add(remedio);


        //        }
        //        return remedios;
        //    }
        //}
    }
}
