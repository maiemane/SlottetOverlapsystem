using System.ComponentModel.DataAnnotations;

namespace Slottet.Auth;

public sealed class LoginFormModel
{
    [Required(ErrorMessage = "Email er påkrævet.")]
    [EmailAddress(ErrorMessage = "Indtast en gyldig email.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password er påkrævet.")]
    public string Password { get; set; } = string.Empty;
}
