using Proj_ICFT.Models.ViewModels;

namespace Proj_ICFT.Services.RelatorioService.Interface;

public interface IRelatorioService
{
    RelatorioDataViewModel GetDados(RelatorioFiltroViewModel filtro, int userId);
}
