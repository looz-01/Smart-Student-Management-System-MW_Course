using Microsoft.Extensions.DependencyInjection;

namespace StudentMangmentSystem_API.Mapping
{
    public static class AutoMapperServiceCollectionExtensions
    {
        public static IServiceCollection AddAutoMapper(this IServiceCollection services, params Type[] profileTypes)
        {
            services.AddAutoMapper(cfg =>
            {
                foreach (var type in profileTypes)
                {
                    cfg.AddProfile(type);
                }
            });

            return services;
        }
    }
}
