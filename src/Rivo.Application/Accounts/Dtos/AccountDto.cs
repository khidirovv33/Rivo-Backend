using Rivo.Domain.Enums;

namespace Rivo.Application.Accounts.Dtos;

public class AccountDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public AccountType Type { get; set; }

    public decimal Balance { get; set; }

    public bool IsActive { get; set; }
}

public class CreateAccountDto
{
    public string Name { get; set; } = null!;

    public AccountType Type { get; set; }
}

public class UpdateAccountDto
{
    public string Name { get; set; } = null!;

    public bool IsActive { get; set; }
}
