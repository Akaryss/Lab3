using System;
using System.Collections.Generic;

namespace Lab2.Models;

public partial class ActiveAdvertisement
{
    public int AdvertisementId { get; set; }

    public string Title { get; set; } = null!;

    public decimal? Price { get; set; }

    public DateTime? PublicationDate { get; set; }

    public string Username { get; set; } = null!;

    public string CategoryName { get; set; } = null!;

    public string RegionName { get; set; } = null!;

    public string CityName { get; set; } = null!;
}
