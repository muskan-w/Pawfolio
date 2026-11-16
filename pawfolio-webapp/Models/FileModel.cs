using System.ComponentModel.DataAnnotations;

namespace pawfolio_webapp.Models
{
    public class FileModel

    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public string ContentType { get; set; } = string.Empty;

        [Required]
        public byte[] Data { get; set; } = Array.Empty<byte>();
    }
}
