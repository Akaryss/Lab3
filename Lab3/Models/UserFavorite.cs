using System;
using System.Collections.Generic;

namespace Lab2.Models;

public partial class UserFavorite
{
    public int UserId { get; set; }

    public int AdvertisementId { get; set; }

    public DateTime? AddedDate { get; set; }

    public string Title { get; set; } = null!;

    public decimal? Price { get; set; }

    public string Username { get; set; } = null!;
}
