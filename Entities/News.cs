using System;
using System.Collections.Generic;

namespace News.Entities;

public partial class News
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Image { get; set; }

    public string? Content { get; set; }
}
