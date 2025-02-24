// Code from Brevo: https://developers.brevo.com/reference/sendtransacemail
using Newtonsoft.Json.Linq;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Model;
using System.Diagnostics;

namespace CyberLoot.Services
{
    public class EmailSender
    {
        public static void SendEmail(string senderName, string senderEmail, string toName, string toEmail, 
            string subject, string textContent)
        {
            var apiInstance = new TransactionalEmailsApi();
            SendSmtpEmailSender Email = new SendSmtpEmailSender(senderName, senderEmail);
            SendSmtpEmailTo smtpEmailTo = new SendSmtpEmailTo(toEmail, toName);
            List<SendSmtpEmailTo> To = new List<SendSmtpEmailTo>();
            To.Add(smtpEmailTo);

            try
            {
                var sendSmtpEmail = new SendSmtpEmail(Email, To, null, null, null, textContent, subject);
                CreateSmtpEmail result = apiInstance.SendTransacEmail(sendSmtpEmail);
                Console.WriteLine("Email Sender Success: \n" + result.ToJson());
            }
            catch (Exception e)
            {
                Console.WriteLine("Email Sender Failed: \n" + e.Message);
            }
        }
    }
}
