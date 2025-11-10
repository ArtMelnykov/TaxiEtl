using Microsoft.Extensions.DependencyInjection;
using TaxiDataETL.Core.Interfaces;
using TaxiDataETL.Core.Services;

namespace TaxiDataETL.Core
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            services.AddScoped<ICsvTripReader, CsvTripReader>();

            return services;
        }
    }
}