namespace Switch.Domain.Exceptions;

/// <summary>
/// Raised when code attempts to move an <see cref="AuthorizationTransaction"/> to a state its
/// current state cannot legally transition to (e.g. reversing a transaction that was never persisted).
/// </summary>
public sealed class InvalidTransactionStateException : Exception
{
    public TransactionState CurrentState { get; }

    public TransactionState AttemptedState { get; }

    public InvalidTransactionStateException(TransactionState currentState, TransactionState attemptedState)
        : base($"Cannot transition an authorization transaction from '{currentState}' to '{attemptedState}'.")
    {
        CurrentState = currentState;
        AttemptedState = attemptedState;
    }
}
