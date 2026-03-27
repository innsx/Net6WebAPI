using Domain.Commons;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Specifies the column as decimal with 18 total digits and 4 decimal places
        [Column(TypeName = "decimal(18,4)")]
        public decimal Rate { get; set; }
    }
}
