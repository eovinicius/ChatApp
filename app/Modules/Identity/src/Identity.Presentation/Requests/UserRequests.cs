using System.ComponentModel.DataAnnotations;

namespace Identity.Presentation.Requests;

public sealed class UserRegisterRequest
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    public string Name { get; set; } = default!;

    [Required(ErrorMessage = "O username é obrigatório")]
    [MinLength(3, ErrorMessage = "O username deve ter no mínimo 3 caracteres")]
    public string Username { get; set; } = default!;

    [Required(ErrorMessage = "A senha é obrigatória")]
    [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres")]
    public string Password { get; set; } = default!;
}

public sealed class UserLoginRequest
{
    [Required(ErrorMessage = "O username é obrigatório")]
    [MinLength(3, ErrorMessage = "O username deve ter no mínimo 3 caracteres")]
    public string Username { get; set; } = default!;

    [Required(ErrorMessage = "A senha é obrigatória")]
    [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres")]
    public string Password { get; set; } = default!;
}
