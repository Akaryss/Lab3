using System;
using System.Collections.Generic;

namespace Lab2.Models;

public partial class Advertisement
{
    public int AdvertisementId { get; set; }

    public int UserId { get; set; }

    public int CategoryId { get; set; }

    public int RegionId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public string? Status { get; set; }

    public DateTime? PublicationDate { get; set; }

    public virtual ICollection<AdvertisementPhoto> AdvertisementPhotos { get; set; } = new List<AdvertisementPhoto>();

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual Region Region { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
