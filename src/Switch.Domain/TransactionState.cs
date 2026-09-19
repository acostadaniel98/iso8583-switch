namespace Switch.Domain;

/// <summary>Lifecycle states of an <see cref="AuthorizationTransaction"/>.</summary>
public enum TransactionState
{
    Received,
    Approved,
    Declined,
    Persisted,
    Reversed
}
