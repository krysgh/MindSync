using Microsoft.Extensions.Configuration;
using MindSync.Application.Common.Interfaces;

namespace MindSync.Infrastructure.Services;

public class AudioFileUrlResolver(IConfiguration configuration) : IAudioFileUrlResolver
{
    public string BuildUrl(string fileName)
    {
        var baseUrl = configuration["AudioSettings:BaseUrl"]?.TrimEnd('/')
            ?? throw new InvalidOperationException("A configuração 'AudioSettings:BaseUrl' não foi definida.");

        return $"{baseUrl}/{fileName}";
    }
}