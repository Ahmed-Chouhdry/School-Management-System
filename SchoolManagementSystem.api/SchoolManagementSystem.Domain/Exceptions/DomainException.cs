namespace SchoolManagementSystem.Domain.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}

public class InsufficientWalletBalanceException : DomainException
{
    public InsufficientWalletBalanceException(decimal currentBalance, decimal attemptedAmount)
        : base($"Cannot apply adjustment of {attemptedAmount}. Current balance is {currentBalance}, resulting balance would be negative.") { }
}

public class InvalidWalletAdjustmentException : DomainException
{
    public InvalidWalletAdjustmentException(string message) : base(message) { }
}

public class UserNotFoundException : DomainException
{
    public UserNotFoundException(Guid id) : base($"User with id '{id}' was not found.") { }
}