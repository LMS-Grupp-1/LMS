using Azure.Storage.Blobs;
using Domain.Contracts;
using LMS.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LMS.API.Extensions;

public static class DocumentStorageExtensions
{
    public static IServiceCollection AddDocumentStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<BlobContainerClient>(_ =>
        {
            var connectionString =
                configuration["BlobStorage:ConnectionString"];

            var containerName =
                configuration["BlobStorage:ContainerName"];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "BlobStorage:ConnectionString is missing.");
            }

            if (string.IsNullOrWhiteSpace(containerName))
            {
                throw new InvalidOperationException(
                    "BlobStorage:ContainerName is missing.");
            }

            return new BlobContainerClient(
                connectionString,
                containerName);
        });

        services.AddSingleton<IDocumentStorage, BlobDocumentStorage>();

        return services;
    }
}