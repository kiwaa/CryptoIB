namespace CIB.Exchange.Model
{
    public enum OrderState
    {
        New,
        RejectedByExchange,
        Accepted,
        CancelPending,
        CancelReject,
        Cancelled
    }
}
