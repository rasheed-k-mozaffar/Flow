using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Flow.Server.Models;

public class ColorScheme
{
    public int Id { get; set; }

    [MaxLength(100)]
    [Column(TypeName = "varchar")]
    public string? Name { get; set; }

    [MaxLength(150)]
    public string? SentMsgBubbleColor { get; set; }

    [MaxLength(150)]
    public string? ReceivedMsgBubbleColor { get; set; }

    [MaxLength(150)]
    public string? AccentsColor { get; set; }

    [MaxLength(150)]
    public string? SelectedMessageColor { get; set; }
}
