using System.ComponentModel.DataAnnotations;

namespace Proj_ICFT.Models.DTO
{
    public class AdministradorDTO
    {
        public AdministradorDTO() { }
        //add token para validacao webapi
        public AdministradorDTO(string email, string senha, string? token)
        {
            Email = email;
            Senha = senha;
        }


        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Senha { get; set; }
        //exemplo de login e senha
        public string? token { get; set; }
    }
}
