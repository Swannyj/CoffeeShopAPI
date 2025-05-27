using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CoffeeShopAPI.Models;

public class CoffeeBean
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; }

    public string? Image { get; set; }

    public string Cost { get; set; }

    public bool IsBOTD { get; set; }

    public int Index { get; set; }

    public string Colour { get; set; }

    public string Description { get; set; } 
}

