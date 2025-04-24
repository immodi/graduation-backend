using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;
using backend.DTOs.Responses;

namespace backend.Services;

public static class EmailService
{
    public static async Task<BaseResponse> Send(string email, string code)
    {
        var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
        var client = new SendGridClient(apiKey);
        var from = new EmailAddress("ay727467@gmail.com", "Admin");
        const string subject = "Verification Code for Password Reset";
        var to = new EmailAddress(email, "");
        var htmlContent = $"Here is your code <strong>{code}</strong>";
        var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlContent);
        var response = await client.SendEmailAsync(msg);

        if (response.IsSuccessStatusCode)
        {
            return new ResetResponse(email);
        }

        Console.WriteLine(response.Body.ToString());
        return new ErrorResponse("An error occured, please try again later"){ StatusCode = 500 };
    }
}
