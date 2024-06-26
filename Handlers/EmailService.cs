using Microsoft.Extensions.Configuration;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Threading.Tasks;

public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly RestClient _client;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
        _client = new RestClient(new RestClientOptions("https://api.mailgun.net/v3")
        {
            Authenticator = new HttpBasicAuthenticator("api", _configuration["Mailgun:ApiKey"])
        });
    }

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        var request = new RestRequest("{domain}/messages", Method.Post);
        request.AddParameter("domain", _configuration["Mailgun:Domain"], ParameterType.UrlSegment);
        request.AddParameter("from", $"Excited User <mailgun@{_configuration["Mailgun:Domain"]}>");
        request.AddParameter("to", toEmail);
        request.AddParameter("subject", subject);
        request.AddParameter("text", message);

        await _client.ExecuteAsync(request);
    }
}
