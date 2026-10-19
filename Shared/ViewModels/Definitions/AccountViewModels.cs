namespace Shared.ViewModels.Definitions;

public class SignInViewModel
{
    public string Email { get; set; }
    public string Password { get; set; }
    public bool RememberMe { get; set; }
}

public class RegisterViewModel
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}

public class ForgetPasswordViewModel
{
    public string Email { get; set; }
}

public class ResetPasswordViewModel
{
    public string Email { get; set; }
    public string Token { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
}

public class ChangePasswordViewModel
{
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
    public string ConfirmPassword { get; set; }
}

public class ProfileViewModel
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public int? TownId { get; set; }
    public string Street { get; set; }

    public string TownArName { get; set; }
    public string TownEnName { get; set; }

    public int? CountryId { get; set; }
    public int? GovernorateId { get; set; }
}

public class AccountIdentity
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string Image { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class AccountToken
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string Token { get; set; }
}
