using Switch.Domain.Exceptions;

namespace Switch.Domain;

/// <summary>
/// Aggregate root for a single card authorization. Moves through a strict state machine —
/// Received → Approved | Declined → Persisted → (Reversed) — and rejects any other transition
/// with <see cref="InvalidTransactionStateException"/> rather than allowing an impossible state.
/// </summary>
public sealed class AuthorizationTransaction
{
    private static readonly IReadOnlyDictionary<TransactionState, TransactionState[]> AllowedTransitions =
        new Dictionary<TransactionState, TransactionState[]>
        {
            [TransactionState.Received] = [TransactionState.Approved, TransactionState.Declined],
            [TransactionState.Approved] = [TransactionState.Persisted],
            [TransactionState.Declined] = [TransactionState.Persisted],
            [TransactionState.Persisted] = [TransactionState.Reversed],
            [TransactionState.Reversed] = []
        };

    public Stan Stan { get; }

    public Pan Pan { get; }

    public Amount Amount { get; }

    public TransactionState State { get; private set; } = TransactionState.Received;

    public ResponseCode? ResponseCode { get; private set; }

    public AuthorizationTransaction(Stan stan, Pan pan, Amount amount)
    {
        Stan = stan;
        Pan = pan;
        Amount = amount;
    }

    public void Approve(ResponseCode responseCode)
    {
        TransitionTo(TransactionState.Approved);
        ResponseCode = responseCode;
    }

    public void Decline(ResponseCode responseCode)
    {
        TransitionTo(TransactionState.Declined);
        ResponseCode = responseCode;
    }

    public void Persist() => TransitionTo(TransactionState.Persisted);

    public void Reverse() => TransitionTo(TransactionState.Reversed);

    private void TransitionTo(TransactionState next)
    {
        if (!Array.Exists(AllowedTransitions[State], allowed => allowed == next))
        {
            throw new InvalidTransactionStateException(State, next);
        }

        State = next;
    }
}
