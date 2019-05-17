using System.ComponentModel.DataAnnotations;

namespace Bookify.DataAccess.Entities
{
    public class EmailTemplate
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Body { get; set; }
    }
}
