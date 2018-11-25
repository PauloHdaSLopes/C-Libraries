using MailKit.Search;
using Paulo.Console.model.emailUtil;
using System.Collections.Generic;

namespace Paulo.Console.interfaces
{
    interface IEmailService
    {
        void Send(EmailMessage emailMessage);
        List<EmailMessage> GetAll(SearchQuery search);
    }
}
