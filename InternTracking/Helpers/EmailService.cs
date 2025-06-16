using QRCoder;
using System.Net;
using System.Net.Mail;

namespace InternTracking.Helpers
{
    public class EmailService
    {
       
            public static void SendEmailWithAttachmentFromStream(string toEmail, string subject, string body, byte[] fileBytes, string fileName)
            {
                var fromEmail = "aryanprajapati5523@gmail.com";
                var password = "qjqpozuuabxjbqvk"; 

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromEmail, password),
                    EnableSsl = true,
                };

                var mail = new MailMessage(fromEmail, toEmail, subject, body);

                // Create attachment from memory
                var stream = new MemoryStream(fileBytes);
                stream.Position = 0;
                var attachment = new Attachment(stream, fileName, "application/pdf");
                mail.Attachments.Add(attachment);

                smtpClient.Send(mail);
            }

        public static string GenerateQrCodeBase64(string url)
        {
            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrCodeBytes = qrCode.GetGraphic(20);
                return Convert.ToBase64String(qrCodeBytes);
            }
        }


    }


    
    }
