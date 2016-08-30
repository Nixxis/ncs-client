using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nixxis.Client.Agent
{
    public interface IMediaPanel
    {
        bool IsReadOnly { get; set; }
    }

    public interface IMailPanel: IMediaPanel
    {
        MailMessage MailMessage { get; }
        string GetHtmlText();
        List<string> GetAttachmentIds();
    }
}
