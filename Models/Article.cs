using System;
using System.ComponentModel.DataAnnotations;

namespace ContentPortal.Models;

public class Article
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Заголовок обязателен")]
    [StringLength(200)]
    public string Title { get; set; } = "";
    
    [Required(ErrorMessage = "Содержание обязательно")]
    public string Content { get; set; } = "";
    
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
    
    public string? ImagePath { get; set; }
    
    // Связь с категорией
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
    
    // Связь с пользователем
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    
    [Obsolete("Используйте User вместо AuthorName")]
    public string AuthorName { get; set; } = "";
}
