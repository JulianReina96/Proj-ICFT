using Proj_ICFT.Models;
using Proj_ICFT.ModelsNew;

namespace Proj_ICFT.Services.MedicamentosServices.Interface
{
    public interface IMedicamentoService
    {
        Task<List<Medicamento>> listarMedicamentos();

        Task<List<Medicamento>> ListarMedicamentosUnique();

        Task<(List<Medicamento> Items, bool HasMore)> BuscarMedicamentos(string? q, int page, int pageSize);
    }
}