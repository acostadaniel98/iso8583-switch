using Switch.Domain.Exceptions;

namespace Switch.Domain.Tests;

public class AuthorizationTransactionTests
{
    public enum Action
    {
        Approve,
        Decline,
        Persist,
        Reverse
    }

    [Fact]
    public void NewTransaction_StartsInReceivedState()
    {
        var tx = NewTransaction();

        Assert.Equal(TransactionState.Received, tx.State);
        Assert.Null(tx.ResponseCode);
    }

    [Fact]
    public void Approve_FromReceived_TransitionsToApprovedAndSetsResponseCode()
    {
        var tx = NewTransaction();

        tx.Approve(ResponseCode.Approved);

        Assert.Equal(TransactionState.Approved, tx.State);
        Assert.Equal(ResponseCode.Approved, tx.ResponseCode);
    }

    [Fact]
    public void Decline_FromReceived_TransitionsToDeclinedAndSetsResponseCode()
    {
        var tx = NewTransaction();
        var code = new ResponseCode("05");

        tx.Decline(code);

        Assert.Equal(TransactionState.Declined, tx.State);
        Assert.Equal(code, tx.ResponseCode);
    }

    [Fact]
    public void Persist_FromApproved_TransitionsToPersisted()
    {
        var tx = NewTransaction();
        tx.Approve(ResponseCode.Approved);

        tx.Persist();

        Assert.Equal(TransactionState.Persisted, tx.State);
    }

    [Fact]
    public void Persist_FromDeclined_TransitionsToPersisted()
    {
        var tx = NewTransaction();
        tx.Decline(new ResponseCode("05"));

        tx.Persist();

        Assert.Equal(TransactionState.Persisted, tx.State);
    }

    [Fact]
    public void Reverse_FromPersisted_TransitionsToReversed()
    {
        var tx = NewTransaction();
        tx.Approve(ResponseCode.Approved);
        tx.Persist();

        tx.Reverse();

        Assert.Equal(TransactionState.Reversed, tx.State);
    }

    // Every (state, action) pair that is NOT one of the five valid transitions above —
    // 5 states x 4 actions = 20 combinations, minus the 5 valid ones, leaves these 15.
    public static TheoryData<TransactionState, Action> InvalidTransitions => new()
    {
        { TransactionState.Approved, Action.Approve },
        { TransactionState.Declined, Action.Approve },
        { TransactionState.Persisted, Action.Approve },
        { TransactionState.Reversed, Action.Approve },
        { TransactionState.Approved, Action.Decline },
        { TransactionState.Declined, Action.Decline },
        { TransactionState.Persisted, Action.Decline },
        { TransactionState.Reversed, Action.Decline },
        { TransactionState.Received, Action.Persist },
        { TransactionState.Persisted, Action.Persist },
        { TransactionState.Reversed, Action.Persist },
        { TransactionState.Received, Action.Reverse },
        { TransactionState.Approved, Action.Reverse },
        { TransactionState.Declined, Action.Reverse },
        { TransactionState.Reversed, Action.Reverse }
    };

    [Theory]
    [MemberData(nameof(InvalidTransitions))]
    public void InvalidTransition_ThrowsInvalidTransactionStateException(TransactionState fromState, Action action)
    {
        var tx = InState(fromState);

        var exception = Assert.Throws<InvalidTransactionStateException>(() => Apply(tx, action));

        Assert.Equal(fromState, exception.CurrentState);
    }

    private static AuthorizationTransaction NewTransaction() =>
        new(new Stan(123456), new Pan("4111111111111111"), new Amount(1000));

    private static AuthorizationTransaction InState(TransactionState state)
    {
        var tx = NewTransaction();

        switch (state)
        {
            case TransactionState.Received:
                break;
            case TransactionState.Approved:
                tx.Approve(ResponseCode.Approved);
                break;
            case TransactionState.Declined:
                tx.Decline(new ResponseCode("05"));
                break;
            case TransactionState.Persisted:
                tx.Approve(ResponseCode.Approved);
                tx.Persist();
                break;
            case TransactionState.Reversed:
                tx.Approve(ResponseCode.Approved);
                tx.Persist();
                tx.Reverse();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }

        return tx;
    }

    private static void Apply(AuthorizationTransaction tx, Action action)
    {
        switch (action)
        {
            case Action.Approve:
                tx.Approve(ResponseCode.Approved);
                break;
            case Action.Decline:
                tx.Decline(new ResponseCode("05"));
                break;
            case Action.Persist:
                tx.Persist();
                break;
            case Action.Reverse:
                tx.Reverse();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }
}
