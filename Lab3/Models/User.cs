using System;
using System.Collections.Generic;

namespace Lab2.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public decimal? Rating { get; set; }

    public virtual ICollection<Advertisement> Advertisements { get; set; } = new List<Advertisement>();

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public virtual ICollection<Message> MessageReceiverUsers { get; set; } = new List<Message>();

    public virtual ICollection<Message> MessageSenderUsers { get; set; } = new List<Message>();
}
