using System.ComponentModel.DataAnnotations;

namespace Bookify.Business.Models
{
    public class EmailTemplateModel
    {
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
