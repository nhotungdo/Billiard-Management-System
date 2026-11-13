using System.ComponentModel.DataAnnotations;

namespace BilliardManagement.Business.DTOs;

public class UpdateStoreAdminDto
{
    [Required, StringLength(100)]
    public string FullName { get; set; } = "";
    [Phone] public string? PhoneNumber { get; set; }
    [EmailAddress] public string? Email { get; set; }
}
