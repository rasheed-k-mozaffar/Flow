using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow.Shared.DataTransferObjects
{
    public class MessagesRequestDto
    {
        public int MessageId { get; set; }
        public Guid ThreadId { get; set; }
        public DateTime LastMessageDate{ get; set; }
    }
}
