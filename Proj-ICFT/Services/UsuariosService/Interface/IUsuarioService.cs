using Proj_ICFT.Models;
using Proj_ICFT.Models.ViewModels;

namespace Proj_ICFT.Services.UsuariosService.Interface
{
    public interface IUsuarioService
    {
        public Usuarios GetUsuarioByEmail(string email);

        public void CadastrarUsuario(UsuarioViewModel usuario);
    }
}
