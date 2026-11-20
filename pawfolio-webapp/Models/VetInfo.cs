using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pawfolio_webapp.Models;

public class VetInfo
{
    [Key]
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = string.Empty;

    public User User { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string LicenseNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string ClinicName { get; set; } = string.Empty;

    [Required]
    [StringLength(300)]
    public string ClinicAddress { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(30)]
    public string ClinicContact { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ClinicianPosition { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string ApprovalStatus { get; set; } = "Pending";
}
