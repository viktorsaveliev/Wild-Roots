using System.Net;
using System.Net.Mail;
using UnityEngine;

public class EmailSupport : MonoBehaviour
{
    private void Start()
    {
        SendEmail("ximotiv@gmail.com", "Test", "test");
        print("Otpravil");
    }

    public void SendEmail(string recipientEmail, string subject, string body)
    {
        // Create an SMTP client
        SmtpClient client = new("mail.adm.tools")
        {
            // Set your credentials
            Credentials = new NetworkCredential("support@wildroots.fun", "Rg%L;5h)95xG")
        };

        // Create a MailMessage object
        MailMessage mail = new()
        {
            From = new MailAddress("support@wildroots.fun")
        };

        mail.To.Add(new MailAddress(recipientEmail));
        mail.Subject = subject;
        mail.Body = body;

        // Send the email
        client.Send(mail);
    }
}
