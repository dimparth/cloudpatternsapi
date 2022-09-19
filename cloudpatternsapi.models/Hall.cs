using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cloudpatternsapi.models
{
    public class Hall
    {
        [Key]
        public int Id { get; init; }
        [Required]
        [MaxLength(60)]
        public string? Title { get; set; }
        [Required]
        [MaxLength(500)]
        public string? Description { get; set; }
        [Required]
        public string? Address { get; set; }
        [Required]
        [Range(0, 60)]
        public int Capacity { get; set; }
        [Required]
        [Phone]
        public string? Phone { get; set; }
        [Required]
        [EmailAddress]
        public string? EmailAddress { get; set; }
        public ICollection<Show>? Shows { get; set; }
    }
}
