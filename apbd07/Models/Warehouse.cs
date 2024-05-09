using System.ComponentModel.DataAnnotations;

namespace apbd07.Models;

public class Warehouse
{
    [Required]
    public int IdProduct { get; set; }
    [Required]
    public int IdWarehouse { get; set; }
    [Required]
    public int Amount { get; set; }
    [Required]
    public DateTime CreatedAt { get; set; }
}