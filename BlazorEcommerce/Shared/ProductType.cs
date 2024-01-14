using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorEcommerce.Shared
{
    public class ProductType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        [NotMapped]
        public bool Editing { get; set; } = false; // แก้ไข
        [NotMapped]
        public bool IsNew { get; set; } = false; // ประเภทใหม่หรือป่าว
    }
}
