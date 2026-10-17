using System.ComponentModel.DataAnnotations;

namespace Toeic.Users.Dto;

public class ChangeUserLanguageDto
{
    [Required]
    public string LanguageName { get; set; }
}