using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using WebApi.Models;

namespace WebApi.Services;

public interface IVerificationService
{
    Task<VerificationServiceResult> SendVerificationCodeAsync(SendVerificationCodeRequest request);
    void SaveVerificationCode(SaveVeificationCodeRequest request);
    VerificationServiceResult VerifyVerificationCOde(VerifyVerificationCodeRequest request);
}

public class VerificationService(IConfiguration configuration, EmailClient emailClient, IMemoryCache cache) : IVerificationService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly EmailClient _emailClient = emailClient;
    private readonly IMemoryCache _cache = cache;
    private static readonly Random _random = new();

    public async Task<VerificationServiceResult> SendVerificationCodeAsync(SendVerificationCodeRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email))
                return new VerificationServiceResult
                {
                    Suceeded = false,
                    Error = "Recipient email address is required"
                };
            var verificationCode = _random.Next(100000, 999999).ToString();
            var subject = $"Your code is {verificationCode}";
            var plainTextContent = @$"
            Please Verify Your Email:
            Enter This Code:
            {verificationCode}
        ";

            var htmlContent = $@"

                <html>
                  <head>
                    <meta charset='UTF-8'>
                    <link href='https://fonts.googleapis.com/css2?family=Inter:wght@400;600&display=swap' rel='stylesheet'>
                  </head>
                  <body style='background-color:#F7F7F7; font-family:Inter, sans-serif; padding:32px;'>
                    <div style='max-width:500px; margin:auto; background:#FFFFFF; padding:24px; border-radius:12px; text-align:center;'>
                      <h2 style='color:#37437D;'>Verify your email</h2>
                      <p style='color:#1E1E20; font-size:15px;'>Use the code below to complete verification:</p>
                      <div style='font-size:24px; font-weight:600; background:#FCD3FE; color:#1C2346; padding:12px; margin-top:16px; border-radius:8px; display:inline-block; letter-spacing:3px;'>
                        {verificationCode}
                      </div>
                    </div>
                  </body>
                </html>
                ";
            var forcedEmail = "freedrik92@hotmail.com";// during test so all mail go to me:(
            var emailMessage = new EmailMessage(
                senderAddress: _configuration["ACS:SenderAddress"],// during test
                recipients: new EmailRecipients([new(forcedEmail)]),// during test

            //senderAddress: _configuration["ACS:SenderAddress"],
            //recipients: new EmailRecipients([new  (request.Email)]),
            content: new EmailContent(subject)
            {
                PlainText = plainTextContent,
                Html = htmlContent
            });

            var emailSendOperation = await _emailClient.SendAsync(WaitUntil.Started, emailMessage);
            SaveVerificationCode(new SaveVeificationCodeRequest
            {
                Email = request.Email,
                Code = verificationCode,
                ValidFor = TimeSpan.FromMinutes(5)
            });

            return new VerificationServiceResult { Suceeded = true, Message = "Verification Email was sent successfully." };
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return new VerificationServiceResult { Suceeded = false, Error = ex.Message };
        }


    }

    public void SaveVerificationCode(SaveVeificationCodeRequest request)
    {
        _cache.Set(request.Email.ToLowerInvariant(), request.Code, request.ValidFor);
    }

    public VerificationServiceResult VerifyVerificationCOde(VerifyVerificationCodeRequest request)
    {
        var key = request.Email.ToLowerInvariant();

        if (_cache.TryGetValue(key, out string? storedCode))
        {
           if (storedCode == request.Code)
            {
                _cache.Remove(key);
                return new VerificationServiceResult
                {
                    Suceeded = true,
                    Message = "Verification successfull"
                };
            }
        }
        return new VerificationServiceResult
        {
            Suceeded = false,
            Error = "Invalid or expored verification code."
        };
    }
}
