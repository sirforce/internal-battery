﻿using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using UpDiddyLib.Dto;
using System.Threading.Tasks;
using System.Linq;

namespace UpDiddyLib.Helpers
{
    public class SysEmail : ISysEmail
    {
        private IConfiguration _configuration;
 
        public SysEmail(IConfiguration Configuration)
        {
            _configuration = Configuration;

        }

        public async Task<bool> SendEmailAsync(string email, string subject, string htmlContent, Constants.SendGridAccount SendGridAccount)
        {

            // Include the environment as part of the subject  
            if (IsProdEnvironment() == false)
                subject = "Environment=" + GetEnvironment().ToUpper() + ":" + subject;            

            bool isDebugMode = _configuration[$"SysEmail:DebugMode"] == "true";
            string SendGridAccountType = Enum.GetName(typeof(Constants.SendGridAccount), SendGridAccount);
            var client = new SendGridClient(_configuration[$"SysEmail:{SendGridAccountType}:ApiKey"]);

            SendGrid.Helpers.Mail.EmailAddress from = new EmailAddress(_configuration[$"SysEmail:{SendGridAccountType}:FromEmailAddress"], "CareerCircle Support");
            SendGrid.Helpers.Mail.EmailAddress to = null;
            // check debug mode to only send emails to actual users in the system is not in debug mode 
            if (isDebugMode == false)
                to = new EmailAddress(email);
            else
                to = new EmailAddress(_configuration[$"SysEmail:SystemDebugEmailAddress"]);

            var plainTextContent = Regex.Replace(htmlContent, "<[^>]*>", "");
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            // Add custom subject property that will be sent to the webhook            
            msg.CustomArgs = new Dictionary<string, string>()
            {
                { "Subject", subject }
            };
            var response = await client.SendEmailAsync(msg);
            return true;
        }

        public Task<bool> SendTemplatedEmailAsync(
            string email,
            string templateId,
            dynamic templateData,
            Constants.SendGridAccount SendGridAccount,
            string subject = null,
            List<Attachment> attachments = null,
            DateTime? sendAt = null,
            int? unsubscribeGroupId = null,
            string cc = null,
            string bcc = null)
        {
            return SendTemplatedEmailAsync(
                email == null? new string[0] : new[] { email },
                templateId,
                templateData,
                SendGridAccount,
                subject,
                attachments,
                sendAt,
                unsubscribeGroupId,
                cc == null ? new string[0] : new[] { cc },
                bcc == null ? new string[0] : new[] { bcc });
        }

        public async Task<bool> SendTemplatedEmailAsync(
            string[] to,
            string templateId,
            dynamic templateData,
            Constants.SendGridAccount SendGridAccount,
            string subject = null,
            List<Attachment> attachments = null,
            DateTime? sendAt = null,
            int? unsubscribeGroupId = null,
            string[] cc = null,
            string[] bcc = null)
        {
            bool isDebugMode = _configuration[$"SysEmail:DebugMode"] == "true";
            string SendGridAccountType = Enum.GetName(typeof(Constants.SendGridAccount), SendGridAccount);

            var client = new SendGridClient(_configuration[$"SysEmail:{SendGridAccountType}:ApiKey"]);
            var message = new SendGridMessage();
            if (sendAt.HasValue)
                message.SendAt = Utils.ToUnixTimeInSeconds(sendAt.Value);
            message.SetFrom(new EmailAddress(_configuration[$"SysEmail:{SendGridAccountType}:FromEmailAddress"], "CareerCircle"));
            message.SetReplyTo(new EmailAddress(_configuration[$"SysEmail:{SendGridAccountType}:ReplyToEmailAddress"]));

            // check debug mode to only send emails to actual users in the system is not in debug mode 
            if (isDebugMode == false)
            {
                void addrecipient(Action<string, string> action, string[] emailList)
                {
                    if (emailList?.Any() != true) { return; }
                    foreach (var email in emailList)
                    {
                        action(email, null);
                    }
                }

                addrecipient(message.AddTo, to);
                addrecipient(message.AddCc, cc);
                addrecipient(message.AddBcc, bcc);
            }
            else
                message.AddTo(new EmailAddress(_configuration[$"SysEmail:SystemDebugEmailAddress"]));

            message.SetTemplateId(templateId);
            message.SetTemplateData(templateData);

            // include the unsubscribe group for the sub-account if one is specified. 
            // note that it is not possible to associate unsubscribe groups from the parent account or other sub-accounts. 
            // (attempting to do this causes the email to be dropped by SendGrid with the following error message: This email was not sent because the SMTPAPI header was invalid.)
            if (unsubscribeGroupId.HasValue)
                message.SetAsm(unsubscribeGroupId.Value, new List<int>() { unsubscribeGroupId.Value });

            if (attachments != null)
                message.AddAttachments(attachments);
            if (subject != null)
                message.SetSubject(subject);


            // Add custom property that will be sent to the webhook. For templated emails, we will use the templated ID as the subject since the actual
            // subject of the template is not readily available 
            string webhookSubject = $"CC Template: {templateId}";
            if (subject != null)
                webhookSubject += $" with a subject of {subject}";
            message.CustomArgs = new Dictionary<string, string>()
            {
                { "Subject", webhookSubject }
            };

            var response = await client.SendEmailAsync(message);
            int statusCode = (int)response.StatusCode;

            return statusCode >= 200 && statusCode <= 299;
        }


        public async Task<bool> SendTemplatedEmailWithReplyToAsync(string email, string templateId, dynamic templateData, Constants.SendGridAccount SendGridAccount, string subject = null, List<Attachment> attachments = null, DateTime? sendAt = null, int? unsubscribeGroupId = null, string replyToEmail = null)
        {
            bool isDebugMode = _configuration[$"SysEmail:DebugMode"] == "true";
            string SendGridAccountType = Enum.GetName(typeof(Constants.SendGridAccount), SendGridAccount);

            var client = new SendGridClient(_configuration[$"SysEmail:{SendGridAccountType}:ApiKey"]);
            var message = new SendGridMessage();
            if (sendAt.HasValue)
                message.SendAt = Utils.ToUnixTimeInSeconds(sendAt.Value);
            message.SetFrom(new EmailAddress(_configuration[$"SysEmail:{SendGridAccountType}:FromEmailAddress"], "CareerCircle"));

            EmailAddress replyToEmailAddress = String.IsNullOrWhiteSpace(replyToEmail) ? 
                                               new EmailAddress(_configuration[$"SysEmail:{SendGridAccountType}:ReplyToEmailAddress"]) :
                                               new EmailAddress(replyToEmail);

            message.SetReplyTo(replyToEmailAddress);

            // check debug mode to only send emails to actual users in the system is not in debug mode 
            if (isDebugMode == false)
                message.AddTo(new EmailAddress(email));
            else
                message.AddTo(new EmailAddress(_configuration[$"SysEmail:SystemDebugEmailAddress"]));

            message.SetTemplateId(templateId);
            message.SetTemplateData(templateData);

            // include the unsubscribe group for the sub-account if one is specified. 
            // note that it is not possible to associate unsubscribe groups from the parent account or other sub-accounts. 
            // (attempting to do this causes the email to be dropped by SendGrid with the following error message: This email was not sent because the SMTPAPI header was invalid.)
            if (unsubscribeGroupId.HasValue)
                message.SetAsm(unsubscribeGroupId.Value, new List<int>() { unsubscribeGroupId.Value });

            if (attachments != null)
                message.AddAttachments(attachments);
            if (subject != null)
                message.SetSubject(subject);

  
            // Add custom property that will be sent to the webhook. For templated emails, we will use the templated ID as the subject since the actual
            // subject of the template is not readily available 
            string webhookSubject = $"CC Template: {templateId}";
            if ( subject != null )
                webhookSubject += $" with a subject of {subject}";
            message.CustomArgs = new Dictionary<string, string>()
            {
                { "Subject", webhookSubject }
            };

            var response = await client.SendEmailAsync(message);           
            int statusCode = (int)response.StatusCode;

            return statusCode >= 200 && statusCode <= 299;
        }

        public async void SendPurchaseReceiptEmail(
            string sendgridTemplateId,
            string profileUrl,
            string email,
            string subject,
            string courseName,
            decimal courseCost,
            decimal promoApplied,
            string formattedStartDate,
            Guid enrollmentGuid,
            string rebateToc)
        {

            // Add and or append environment information for non-production environments 
            if (IsProdEnvironment() == false)
            {
                if (subject == null)
                    subject = "Environment=" + GetEnvironment().ToUpper();
                else
                    subject = "Environment=" + GetEnvironment().ToUpper() + ":" + subject;
            }



            var client = new SendGridClient(_configuration["SysEmail:Transactional:ApiKey"]);
            var message = new SendGridMessage();
            message.SetFrom(new EmailAddress(_configuration["SysEmail:Transactional:FromEmailAddress"], "CareerCircle"));
            message.SetReplyTo(new EmailAddress(_configuration["SysEmail:Transactional:ReplyToEmailAddress"]));
            message.AddTo(new EmailAddress(email));
            message.SetTemplateId(sendgridTemplateId);
            PurchaseReceipt purchaseReceipt = new PurchaseReceipt
            {
                Subject = subject,
                Profile_Url = profileUrl,
                Course_Name = courseName,
                Course_Price = courseCost.ToString(),
                Discount = promoApplied.ToString(),
                Final_Price = (courseCost - promoApplied).ToString(),
                Start_Date = formattedStartDate,
                Enrollment_Guid = enrollmentGuid.ToString(),
                Rebate_Toc = rebateToc
            };
            message.SetTemplateData(purchaseReceipt);
            var response = await client.SendEmailAsync(message);

        }


        // Tried to capture the env in the constructor for a cleaner implementation but it appeared not to be called when code like:
        // _sysEmail = services.GetService<ISysEmail>();  
        // was used to DI in this class.  The time required to figure how to reliably capture the env information 
        // in the class constructor was not justified over this less elegant solution.
        private string GetEnvironment()
        {
            string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (string.IsNullOrEmpty(env))
                env = "Development";

            return env;
        }

        private bool IsProdEnvironment()
        {        
            return GetEnvironment().Trim() == "production";
        }

 
        #region Private Helper Classes
        private class TemplateData
        {
            [JsonProperty("subject")]
            public string Subject { get; set; }
        }
        private class PurchaseReceipt : TemplateData
        {
            [JsonProperty("course_name")]
            public string Course_Name { get; set; }

            [JsonProperty("profile_url")]
            public string Profile_Url { get; set; }

            [JsonProperty("course_price")]
            public string Course_Price { get; set; }

            [JsonProperty("discount")]
            public string Discount { get; set; }

            [JsonProperty("final_price")]
            public string Final_Price { get; set; }

            [JsonProperty("start_date")]
            public string Start_Date { get; set; }

            [JsonProperty("enrollment_guid")]
            public string Enrollment_Guid { get; set; }

            [JsonProperty("rebate_toc")]
            public string Rebate_Toc { get; set; }
        }
        #endregion
    }
}
