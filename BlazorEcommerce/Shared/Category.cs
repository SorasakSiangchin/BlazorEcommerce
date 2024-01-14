using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorEcommerce.Shared
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public bool Visible { get; set; } = true; // ให้มองเห็นได้
        public bool Deleted { get; set; } = false; // ห้ามใช้งาน
        [NotMapped]
        public bool Editing { get; set; } = false; // แก้ไข
        [NotMapped]
        public bool IsNew { get; set; } = false; // ประเภทใหม่หรือป่าว
    }
}
