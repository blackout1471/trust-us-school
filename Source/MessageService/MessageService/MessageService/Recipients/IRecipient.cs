using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageService.Recipients
{
    public interface IRecipient
    {
        string To { get; }
    }
}
