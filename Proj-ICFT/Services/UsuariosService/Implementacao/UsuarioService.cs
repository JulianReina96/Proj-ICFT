using Microsoft.EntityFrameworkCore;
using Proj_ICFT.DataNew;
using Proj_ICFT.Models;
using Proj_ICFT.Models.ViewModels;
using Proj_ICFT.ModelsNew;
using Proj_ICFT.Services.UsuariosService.Interface;

namespace Proj_ICFT.Services.UsuariosService.Implementacao
{
    public class UsuarioService : IUsuarioService
    {

        private readonly AppDbContextNew _appDbContextNew;
        public UsuarioService(AppDbContextNew appDbContextNew)
        {
            _appDbContextNew = appDbContextNew;
        }


        public Usuario GetUsuarioByEmail(string email) 
        {
            return _appDbContextNew.Usuarios.Include(r => r.Usuario1 == email).FirstOrDefault();
        }

        public void CadastrarUsuario(UsuarioViewModel usuario) 
        {
            try
            {

                Usuario usuarioEntity = new Usuario();
                usuarioEntity.Usuario1 = usuario.Usuario;
                usuarioEntity.DataCriacao = usuario.DataCriacao;

                _appDbContextNew.Usuarios.Add(usuarioEntity);
                _appDbContextNew.SaveChanges();
            }
            catch (Exception ex) 
            {
                throw ex;
            }

            
        }
        

    }
}
