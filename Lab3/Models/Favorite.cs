using System;
using System.Collections.Generic;

namespace Lab2.Models;

public partial class Favorite
{
    public int FavoriteId { get; set; }

    public int UserId { get; set; }

    public int AdvertisementId { get; set; }

    public DateTime? AddedDate { get; set; }

    public virtual Advertisement Advertisement { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
