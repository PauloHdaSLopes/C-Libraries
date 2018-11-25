using MailKit;
using MailKit.Search;
using MailKit.Net.Pop3;
using MailKit.Net.Smtp;
using MailKit.Net.Imap;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Paulo.Console.interfaces;
using System.IO;
using static Paulo.Console.model.emailUtil.EmailMessage;

namespace Paulo.Console.model.emailUtil
{
    public class EmailService
    {
        private readonly IEmailConfiguration _emailConfiguration;

        public EmailService(IEmailConfiguration emailConfiguration)
        {
            _emailConfiguration = emailConfiguration;
        }

        public void Send(EmailMessage emailMessage)
        {
            var message = new MimeMessage();
            message.To.AddRange(emailMessage.ToAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
            message.From.AddRange(emailMessage.FromAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));

            message.Subject = emailMessage.Subject;
            //We will say we are sending HTML. But there are options for plaintext etc. 
            message.Body = new TextPart(TextFormat.Html)
            {
                Text = emailMessage.Content
            };

            //Be careful that the SmtpClient class is the one from Mailkit not the framework!
            using (var emailClient = new SmtpClient())
            {
                //The last parameter here is to use SSL (Which you should!)
                emailClient.Connect(_emailConfiguration.Server, _emailConfiguration.Port, false);

                //Remove any OAuth functionality as we won't be using it. 
                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

                emailClient.Authenticate(_emailConfiguration.Username, _emailConfiguration.Password);

                emailClient.Send(message);

                emailClient.Disconnect(true);
            }
        }

        public List<EmailMessage> GetAllNew()
        {
            try
            {
                using (ImapClient client = new ImapClient())
                {
                    var credentials = new NetworkCredential(_emailConfiguration.Username, _emailConfiguration.Password);
                    var uri = new Uri("imaps://" + _emailConfiguration.Server);

                    client.Connect(uri);

                    // Remove the XOAUTH2 authentication mechanism since we don't have an OAuth2 token.
                    client.AuthenticationMechanisms.Remove("XOAUTH2");

                    client.Authenticate(credentials);

                    client.Inbox.Open(FolderAccess.ReadOnly);

                    SearchQuery search = SearchQuery.NotSeen;

                    // search for messages where the Subject header contains either "MimeKit" or "MailKit"
                    var uids = client.Inbox.Search(search);

                    List<EmailMessage> emails = new List<EmailMessage>();

                    foreach (var uid in uids)
                    {
                        List<Attachments> attachments = new List<Attachments>();

                        var message = client.Inbox.GetMessage(uid);
                        var messageAttachments = message.Attachments;

                        foreach (var attach in messageAttachments)
                        {
                            var fileName = attach.ContentDisposition?.FileName ?? attach.ContentType.Name;
                            var stream = new MemoryStream();

                            if (attach is MessagePart)
                            {
                                var rcf822 = (MessagePart)attach;
                                rcf822.WriteTo(stream);
                            }
                            else
                            {
                                var part = (MimePart)attach;
                                part.ContentObject.DecodeTo(stream);
                            }
                            attachments.Add(new Attachments() { Name = fileName, Archive = stream });
                        }
                        
                        var emailMessage = new EmailMessage
                        {
                            UID = uid,
                            Content = message.TextBody ?? message.HtmlBody,
                            Subject = message.Subject
                        };

                        emailMessage.ToAddresses.AddRange(message.To.Select(x => (MailboxAddress)x).Select(x => new EmailAddress { Address = x.Address, Name = x.Name }));
                        emailMessage.FromAddresses.AddRange(message.From.Select(x => (MailboxAddress)x).Select(x => new EmailAddress { Address = x.Address, Name = x.Name }));
                        emailMessage.attachments = attachments;
                        emails.Add(emailMessage);
                    }
                    return emails;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<EmailMessage> GetAllJunk()
        {
            try
            {
                using (ImapClient client = new ImapClient())
                {
                    var credentials = new NetworkCredential(_emailConfiguration.Username, _emailConfiguration.Password);
                    var uri = new Uri("imaps://" + _emailConfiguration.Server);

                    client.Connect(uri);

                    // Remove the XOAUTH2 authentication mechanism since we don't have an OAuth2 token.
                    client.AuthenticationMechanisms.Remove("XOAUTH2");

                    client.Authenticate(credentials);

                    var folder = client.GetFolder("").GetSubfolder("Lixo Eletrônico");

                    folder.Open(FolderAccess.ReadWrite);

                    SearchQuery search = SearchQuery.New;

                    // search for messages where the Subject header contains either "MimeKit" or "MailKit"
                    var uids = folder.Search(search);

                    List<EmailMessage> emails = new List<EmailMessage>();

                    foreach (var uid in uids)
                    {
                        List<Attachments> attachments = new List<Attachments>();

                        var message = folder.GetMessage(uid);
                        var messageAttachments = message.Attachments;
                        
                        foreach (var attach in messageAttachments)
                        {
                            var fileName = attach.ContentDisposition?.FileName ?? attach.ContentType.Name;
                            var stream = new MemoryStream();

                            if (attach is MessagePart)
                            {
                                var rcf822 = (MessagePart)attach;
                                rcf822.WriteTo(stream);
                            }
                            else
                            {
                                var part = (MimePart)attach;
                                part.ContentObject.DecodeTo(stream);
                            }
                            attachments.Add(new Attachments() { Name = fileName, Archive = stream });
                        }

                        var emailMessage = new EmailMessage
                        {
                            UID = uid,
                            Content = message.TextBody ?? message.HtmlBody,
                            Subject = message.Subject
                        };

                        emailMessage.ToAddresses.AddRange(message.To.Select(x => (MailboxAddress)x).Select(x => new EmailAddress { Address = x.Address, Name = x.Name }));
                        emailMessage.FromAddresses.AddRange(message.From.Select(x => (MailboxAddress)x).Select(x => new EmailAddress { Address = x.Address, Name = x.Name }));
                        emailMessage.attachments = attachments;
                        emails.Add(emailMessage);

                        folder.AddFlags(uid, MessageFlags.Seen, true);
                    }
                    return emails;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool MoveEmail(UniqueId uid, string destFolder)
        {
            try
            {
                using (ImapClient client = new ImapClient())
                {
                    var credentials = new NetworkCredential(_emailConfiguration.Username, _emailConfiguration.Password);
                    var uri = new Uri("imaps://" + _emailConfiguration.Server);

                    client.Connect(uri);

                    // Remove the XOAUTH2 authentication mechanism since we don't have an OAuth2 token.
                    client.AuthenticationMechanisms.Remove("XOAUTH2");

                    client.Authenticate(credentials);

                    client.Inbox.Open(FolderAccess.ReadWrite);

                    var toFolder = client.GetFolder("inbox").GetSubfolder(destFolder);

                    //toFolder.Open(FolderAccess.ReadWrite);

                    client.Inbox.MoveTo(uid, toFolder);

                    return true;

                }
            }
            catch (Exception ex)
            {
                return false;
                throw ex;
            }
        }

        #region SAMPLE FUNCTIONS

        //class Imap : IEmailService
        //{
        //    EmailService emailService;

        //    public Imap(EmailService emailService)
        //    {
        //        this.emailService = emailService;
        //    }

        //    public List<EmailMessage> GetAll(SearchQuery search)
        //    {
        //        using (ImapClient client = new ImapClient())
        //        {
        //            var credentials = new NetworkCredential(emailService._emailConfiguration.Username, emailService._emailConfiguration.Password);
        //            var uri = new Uri("imaps://" + emailService._emailConfiguration.Server);

        //            client.Connect(uri);

        //            // Remove the XOAUTH2 authentication mechanism since we don't have an OAuth2 token.
        //            client.AuthenticationMechanisms.Remove("XOAUTH2");

        //            client.Authenticate(credentials);

        //            client.Inbox.Open(FolderAccess.ReadOnly);

        //            // search for messages where the Subject header contains either "MimeKit" or "MailKit"
        //            var uids = client.Inbox.Search(search);

        //            List<EmailMessage> emails = new List<EmailMessage>();
        //            foreach (var uid in uids)
        //            {

        //                var message = client.Inbox.GetMessage(uid);
        //                var emailMessage = new EmailMessage
        //                {
        //                    Content = message.TextBody ?? message.HtmlBody,
        //                    Subject = message.Subject
        //                };
        //                emailMessage.ToAddresses.AddRange(message.To.Select(x => (MailboxAddress)x).Select(x => new EmailAddress { Address = x.Address, Name = x.Name }));
        //                emailMessage.FromAddresses.AddRange(message.From.Select(x => (MailboxAddress)x).Select(x => new EmailAddress { Address = x.Address, Name = x.Name }));
        //            }
        //            return emails;
        //        }
        //    }

        //    public void Send(EmailMessage emailMessage)
        //    {

        //    }
        //}

        //class Pop3 : IEmailService
        //{

        //    EmailService emailService;

        //    public Pop3(EmailService emailService)
        //    {
        //        this.emailService = emailService;
        //    }

        //    public List<EmailMessage> GetAll(SearchQuery search)
        //    {
        //        using (var emailClient = new Pop3Client())
        //        {
        //            emailClient.Connect(emailService._emailConfiguration.Server, emailService._emailConfiguration.Port, false);

        //            emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

        //            emailClient.Authenticate(emailService._emailConfiguration.Username, emailService._emailConfiguration.Password);

        //            List<EmailMessage> emails = new List<EmailMessage>();
        //            for (int i = 0; i < emailClient.Count && i < 10; i++)
        //            {
        //                var message = emailClient.GetMessage(i);
        //                var emailMessage = new EmailMessage
        //                {
        //                    Content = !string.IsNullOrEmpty(message.HtmlBody) ? message.HtmlBody : message.TextBody,
        //                    Subject = message.Subject
        //                };
        //                emailMessage.ToAddresses.AddRange(message.To.Select(x => (MailboxAddress)x).Select(x => new EmailAddress { Address = x.Address, Name = x.Name }));
        //                emailMessage.FromAddresses.AddRange(message.From.Select(x => (MailboxAddress)x).Select(x => new EmailAddress { Address = x.Address, Name = x.Name }));
        //            }
        //            return emails;
        //        }
        //    }

        //    public void Send(EmailMessage emailMessage)
        //    {
        //        throw new NotImplementedException();
        //    }

        //}

        //class Smtp : IEmailService
        //{
        //    EmailService emailService;

        //    public Smtp(EmailService emailService)
        //    {
        //        this.emailService = emailService;
        //    }

        //    public List<EmailMessage> GetAll(SearchQuery search)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public void Send(EmailMessage emailMessage)
        //    {
        //        var message = new MimeMessage();
        //        message.To.AddRange(emailMessage.ToAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
        //        message.From.AddRange(emailMessage.FromAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));

        //        message.Subject = emailMessage.Subject;
        //        //We will say we are sending HTML. But there are options for plaintext etc. 
        //        message.Body = new TextPart(TextFormat.Html)
        //        {
        //            Text = emailMessage.Content
        //        };

        //        //Be careful that the SmtpClient class is the one from Mailkit not the framework!
        //        using (var emailClient = new SmtpClient())
        //        {
        //            //The last parameter here is to use SSL (Which you should!)
        //            emailClient.Connect(emailService._emailConfiguration.Server, emailService._emailConfiguration.Port, false);

        //            //Remove any OAuth functionality as we won't be using it. 
        //            emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

        //            emailClient.Authenticate(emailService._emailConfiguration.Username, emailService._emailConfiguration.Password);

        //            emailClient.Send(message);

        //            emailClient.Disconnect(true);
        //        }
        //    }
        //} 

        #endregion

        #region OLD

        //public List<EmailMessage> ReceiveNewEmail()
        //{
        //    using (ImapClient client = new ImapClient())
        //    {
        //        var credentials = new NetworkCredential(_emailConfiguration.ImapUsername, _emailConfiguration.ImapPassword);
        //        var uri = new Uri("imaps://" + _emailConfiguration.ImapServer);

        //        client.Connect(uri);

        //        // Remove the XOAUTH2 authentication mechanism since we don't have an OAuth2 token.
        //        client.AuthenticationMechanisms.Remove("XOAUTH2");

        //        client.Authenticate(credentials);

        //        client.Inbox.Open(FolderAccess.ReadOnly);

        //        // search for messages where the Subject header contains either "MimeKit" or "MailKit"
        //        var query = SearchQuery.New;
        //        var uids = client.Inbox.Search(query);

        //        List<EmailMessage> emails = new List<EmailMessage>();
        //        foreach (var uid in uids)
        //        {
        //            var message = client.Inbox.GetMessage(uid);
        //            var emailMessage = new EmailMessage
        //            {
        //                Content = message.TextBody ?? message.HtmlBody,
        //                Subject = message.Subject
        //            };
        //            emailMessage.ToAddresses.AddRange(message.To.Select(x => (MailboxAddress)x).Select(x => new EmailAddress { Address = x.Address, Name = x.Name }));
        //            emailMessage.FromAddresses.AddRange(message.From.Select(x => (MailboxAddress)x).Select(x => new EmailAddress { Address = x.Address, Name = x.Name }));
        //        }
        //        return emails;
        //    }
        //}

        //public List<EmailMessage> ReceiveEmailPop(int maxCount = 10)
        //{
        //    using (var emailClient = new Pop3Client())
        //    {
        //        emailClient.Connect(_emailConfiguration.PopServer, _emailConfiguration.PopPort, false);

        //        emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

        //        emailClient.Authenticate(_emailConfiguration.PopUsername, _emailConfiguration.PopPassword);

        //        List<EmailMessage> emails = new List<EmailMessage>();
        //        for (int i = 0; i < emailClient.Count && i < maxCount; i++)
        //        {
        //            var message = emailClient.GetMessage(i);
        //            var emailMessage = new EmailMessage
        //            {
        //                Content = !string.IsNullOrEmpty(message.HtmlBody) ? message.HtmlBody : message.TextBody,
        //                Subject = message.Subject
        //            };
        //            emailMessage.ToAddresses.AddRange(message.To.Select(x => (MailboxAddress)x).Select(x => new EmailAddress { Address = x.Address, Name = x.Name }));
        //            emailMessage.FromAddresses.AddRange(message.From.Select(x => (MailboxAddress)x).Select(x => new EmailAddress { Address = x.Address, Name = x.Name }));
        //        }
        //        return emails;
        //    }
        //}

        //public void Send(EmailMessage emailMessage)
        //{
        //    var message = new MimeMessage();
        //    message.To.AddRange(emailMessage.ToAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
        //    message.From.AddRange(emailMessage.FromAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));

        //    message.Subject = emailMessage.Subject;
        //    //We will say we are sending HTML. But there are options for plaintext etc. 
        //    message.Body = new TextPart(TextFormat.Html)
        //    {
        //        Text = emailMessage.Content
        //    };

        //    //Be careful that the SmtpClient class is the one from Mailkit not the framework!
        //    using (var emailClient = new SmtpClient())
        //    {
        //        //The last parameter here is to use SSL (Which you should!)
        //        emailClient.Connect(_emailConfiguration.SmtpServer, _emailConfiguration.SmtpPort, false);

        //        //Remove any OAuth functionality as we won't be using it. 
        //        emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

        //        emailClient.Authenticate(_emailConfiguration.SmtpUsername, _emailConfiguration.SmtpPassword);

        //        emailClient.Send(message);

        //        emailClient.Disconnect(true);
        //    }
        //} 
        #endregion

    }
}
