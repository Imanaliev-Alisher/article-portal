using System.ComponentModel.DataAnnotations;

namespace ContentPortal.Models;

public class Category
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Название обязательно")]
    [StringLength(100)]
    public string Name { get; set; } = "";
    
    public string? Description { get; set; }
    
    // Навигационное свойство
    public ICollection<Article> Articles { get; set; } = new List<Article>();
}
