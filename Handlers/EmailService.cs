using Microsoft.Extensions.Configuration;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Threading.Tasks;

public class EmailService
{
    private readonly RestClient _client;

    public EmailService()
    {
        var mailgunApiKey = Environment.GetEnvironmentVariable("MAILGUN_APIKEY");
        var mailgunDomain = Environment.GetEnvironmentVariable("MAILGUN_DOMAIN");

        _client = new RestClient(new RestClientOptions("https://api.mailgun.net/v3")
        {
            Authenticator = new HttpBasicAuthenticator("api", mailgunApiKey)
        });

        MailgunDomain = mailgunDomain;
    }

    private string MailgunDomain { get; }

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        var request = new RestRequest("{domain}/messages", Method.Post);
        request.AddParameter("domain", MailgunDomain, ParameterType.UrlSegment);
        request.AddParameter("from", $"Excited User <mailgun@{MailgunDomain}>");
        request.AddParameter("to", toEmail);
        request.AddParameter("subject", subject);
        request.AddParameter("text", message);

        await _client.ExecuteAsync(request);
    }
}
