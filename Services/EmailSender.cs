using Microsoft.AspNetCore.Identity.UI.Services;
namespace property_lease_saas.Services;
public class EmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // Do nothing (for now)
            //👉 Say the next step you want:

            // 🔐 Email confirmation with SMTP

            // 🔁 Login redirect by claim

            // 🧱 Authorization policies in depth

            // 🛡️ Admin vs UserType architecture
            // do this for email confirmation feature in future

        return Task.CompletedTask;
    }
}
