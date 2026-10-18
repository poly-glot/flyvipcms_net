namespace FlyVip.Web.Data.Entities;

public enum UserRole
{
    Admin,
    Member,
    Submember,
}

public enum MemberStatus
{
    Inactive,
    Active,
    Banned,
}

public enum PaymentType
{
    Joining,
    Yearly,
    Quarterly,
    Bonus,
    Points,
}

public enum ReservationKind
{
    Complete,
    Partial,
}

public enum ReservationStatus
{
    Pending,
    Confirmed,
    Cancelled,
}

public enum LedgerKind
{
    Payment,
    Adjustment,
    TransferIn,
    TransferOut,
    Booking,
    Refund,
}
