using Microsoft.EntityFrameworkCore;
using Proj_ICFT.Data;
using Proj_ICFT.Models;
using Proj_ICFT.Services.FormularioServices.Interface;

namespace Proj_ICFT.Services.FormularioServices.Implementacao
{
    public class FormularioServices : IFormularioServices
    {

        private readonly FrequenciaDbContext _frequenciaDbContext;
        private readonly CategoriaDbContext _categoriaDbContext;
        private readonly InstrucoesAdicionaisDbContext _instrucoesAdicionaisDbContext;
        private readonly TipoDbContext _tipoDbContext;

        public FormularioServices(FrequenciaDbContext frequenciaDbContext, CategoriaDbContext categoriaDbContext, InstrucoesAdicionaisDbContext instrucoesAdicionaisDbContext, TipoDbContext tipoDbContext)
        {
            _frequenciaDbContext = frequenciaDbContext;
            _categoriaDbContext = categoriaDbContext;
            _instrucoesAdicionaisDbContext = instrucoesAdicionaisDbContext;
            _tipoDbContext = tipoDbContext;
        }


        public async Task<List<Categoria>> listarCategorias()
        {

            return await _categoriaDbContext.Categoria.Include(x=> x.tipos).OrderBy(c=> c.Name).ToListAsync();
                
         }

        public async Task<List<Frequencia>> listarFrequencias ()
        {
            return await _frequenciaDbContext.Frequencia.OrderBy(f=>f.Name).ToListAsync();
        }

        public async Task<List<InstrucoesAdicionais>> listarInstrucoesAdicionais()
        {
            return await _instrucoesAdicionaisDbContext.InstrucoesAdicionais.OrderBy(i=> i.name).ToListAsync();
        }

        public async Task<List<Tipo>> listarTipoByCategoriaId(int id)
        {

            return await _tipoDbContext.Tipo.Where(s => s.CategoriaId == id).OrderBy(t=> t.Name).ToListAsync();

        }



    }
}
