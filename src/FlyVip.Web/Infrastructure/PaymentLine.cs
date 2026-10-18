using FlyVip.Web.Data.Entities;

namespace FlyVip.Web.Infrastructure;

public sealed record PaymentLine(
    int Id,
    int UserId,
    DateOnly PaidOn,
    PaymentType Type,
    string Remark,
    string Description,
    decimal Amount,
    string? MemberCode,
    string? FirstName,
    string? LastName)
{
    public string FullName => FirstName + " " + LastName;
}
