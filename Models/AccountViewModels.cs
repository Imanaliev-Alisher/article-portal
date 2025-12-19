using System.ComponentModel.DataAnnotations;

namespace ContentPortal.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Неверный формат email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Пароль обязателен")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Display(Name = "Запомнить меня")]
    public bool RememberMe { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Неверный формат email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Пароль обязателен")]
    [StringLength(100, MinimumLength = 4, ErrorMessage = "Пароль должен содержать минимум 4 символа")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Подтверждение пароля обязательно")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Пароли не совпадают")]
    [Display(Name = "Подтвердите пароль")]
    public string ConfirmPassword { get; set; } = "";

    [Display(Name = "Полное имя")]
    [StringLength(100)]
    public string? FullName { get; set; }
}

public class EditUserViewModel
{
    public string UserId { get; set; } = "";
    
    [Display(Name = "Email")]
    public string Email { get; set; } = "";
    
    [Display(Name = "Полное имя")]
    [StringLength(100)]
    public string? FullName { get; set; }
    
    public List<string> CurrentRoles { get; set; } = new();
    public List<string> AllRoles { get; set; } = new();
    
    [Display(Name = "Роли")]
    public List<string>? SelectedRoles { get; set; }
}
