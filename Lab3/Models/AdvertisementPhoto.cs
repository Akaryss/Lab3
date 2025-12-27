using System;
using System.Collections.Generic;

namespace Lab2.Models;

public partial class AdvertisementPhoto
{
    public int PhotoId { get; set; }

    public int AdvertisementId { get; set; }

    public string PhotoUrl { get; set; } = null!;

    public bool? IsMain { get; set; }

    public virtual Advertisement Advertisement { get; set; } = null!;
}
