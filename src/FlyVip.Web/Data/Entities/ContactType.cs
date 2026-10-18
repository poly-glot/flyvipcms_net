using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlyVip.Web.Data.Entities;

public sealed class ContactType
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public byte Id { get; set; }

    [MaxLength(40)]
    public string Name { get; set; } = "";

    [MaxLength(1)]
    public string Suffix { get; set; } = "";
}
