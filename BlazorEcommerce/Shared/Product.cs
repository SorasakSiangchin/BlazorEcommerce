
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorEcommerce.Shared
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public List<Image> Images { get; set; } = new List<Image>();
        public Category? Category { get; set; }
        public int CategoryId { get; set; }
        public bool Featured { get; set; } = false; // สินค้าแนะนำ
        // ตัวเลือกสินค้า
        public List<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public bool Visible { get; set; } = true; // ให้มองเห็นได้
        public bool Deleted { get; set; } = false; // ห้ามใช้งาน
        [NotMapped]
        public bool Editing { get; set; } = false; // แก้ไข
        [NotMapped]
        public bool IsNew { get; set; } = false; // ประเภทใหม่หรือป่าว
    }
}
