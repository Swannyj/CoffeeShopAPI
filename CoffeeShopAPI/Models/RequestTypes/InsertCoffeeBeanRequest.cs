using System.ComponentModel.DataAnnotations;

namespace CoffeeShopAPI.Models.RequestTypes
{
    public class InsertCoffeeBeanRequest
    {
        [Required]
        public string Name { get; set; }

        public string? Image { get; set; }

        [Required]
        public string Cost { get; set; }

        [Required]
        public string Colour { get; set; }

        [StringLength(500, ErrorMessage = "Description can't exceed 500 characters.")]
        public string Description { get; set; }

    }
}
