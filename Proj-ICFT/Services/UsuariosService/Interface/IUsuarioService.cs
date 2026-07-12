using Proj_ICFT.Models;
using Proj_ICFT.Models.ViewModels;
using Proj_ICFT.ModelsNew;

namespace Proj_ICFT.Services.UsuariosService.Interface
{
    public interface IUsuarioService
    {
        public Usuario GetUsuarioByEmail(string email);

        public void CadastrarUsuario(UsuarioViewModel usuario);
    }
}
