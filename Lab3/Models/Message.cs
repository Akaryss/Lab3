using System;
using System.Collections.Generic;

namespace Lab2.Models;

public partial class Message
{
    public int MessageId { get; set; }

    public int AdvertisementId { get; set; }

    public int SenderUserId { get; set; }

    public int ReceiverUserId { get; set; }

    public string MessageText { get; set; } = null!;

    public DateTime? SendDate { get; set; }

    public virtual Advertisement Advertisement { get; set; } = null!;

    public virtual User ReceiverUser { get; set; } = null!;

    public virtual User SenderUser { get; set; } = null!;
}
