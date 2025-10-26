using System.ComponentModel.DataAnnotations.Schema;

namespace Proj_ICFT.Models
{
    [Serializable]
    [Table("Medicamentos")]
    public class Medicamentos
    {

        public int Id { get; set; }
        public string NOME_PRODUTO { get; set; }
        public string PRINCIPIO_ATIVO { get; set; }
        public string CATEGORIA_REGULATORIA { get; set; }
        public string CLASSE_TERAPEUTICA { get; set; }
        public string NUMERO_REGISTRO_PRODUTO { get; set; }
        public string EMPRESA_DETENTORA_REGISTRO { get; set; }
        public string SITUACAO_REGISTRO { get; set; }
       


    }
}
