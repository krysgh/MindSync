using FluentValidation;
using MediatR;
using MindSync.Application.Common.Behaviours;
using MindSync.Application.Common.Interfaces;
using MindSync.Application.Features.FavoritedSessionLists.Interfaces;
using MindSync.Domain.Interfaces;
using MindSync.Infrastructure.Data.Context;
using MindSync.Infrastructure.Data.Queries;
using MindSync.Infrastructure.Data.Repositories;
using MindSync.Infrastructure.Services;

namespace MindSync.Api.Configurations;

public static class DependencyInjectionConfig
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("A String de conexão 'DefaultConnection' não foi encontrada.");

        services.AddSingleton(new DbConnectionFactory(connectionString));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IStressSessionRepository, StressSessionRepository>();
        services.AddScoped<IFavoritedListRepository, FavoritedListRepository>();
        services.AddScoped<IFavoritedListQueries, FavoritedListQueries>();
        services.AddScoped<IAudioFileUrlResolver, AudioFileUrlResolver>();
        services.AddScoped<IAudioTrackRepository, AudioTrackRepository>();
        services.AddScoped<ISessionAudioMixRepository, SessionAudioMixRepository>();
        services.AddScoped<IAudioMixComposer, AudioMixComposer>();

        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IPasswordHasher).Assembly));
        services.AddValidatorsFromAssembly(typeof(IPasswordHasher).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

        return services;
    }
}
