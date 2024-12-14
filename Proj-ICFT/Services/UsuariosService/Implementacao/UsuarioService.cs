using Microsoft.EntityFrameworkCore;
using Proj_ICFT.Data;
using Proj_ICFT.Models;
using Proj_ICFT.Models.ViewModels;
using Proj_ICFT.Services.UsuariosService.Interface;

namespace Proj_ICFT.Services.UsuariosService.Implementacao
{
    public class UsuarioService : IUsuarioService
    {

        private readonly UsuariosDbContext _usuariosDbContext;
        public UsuarioService(UsuariosDbContext usuariosDbContext)
        {
            _usuariosDbContext = usuariosDbContext;
        }


        public Usuarios GetUsuarioByEmail(string email) 
        {
            return _usuariosDbContext.Usuarios.Include(r => r.Usuario == email).FirstOrDefault();
        }

        public void CadastrarUsuario(UsuarioViewModel usuario) 
        {
            try
            {

                Usuarios usuarioEntity = new Usuarios();
                usuarioEntity.Usuario = usuario.Usuario;
                usuarioEntity.DataCriacao = usuario.DataCriacao;

                _usuariosDbContext.Add(usuarioEntity);
                _usuariosDbContext.SaveChanges();
            }
            catch (Exception ex) 
            {
                throw ex;
            }

            
        }
        

    }
}
