namespace MindSync.Application.Common.Interfaces;

public interface IAudioFileUrlResolver
{
    string BuildUrl(string fileName);
}