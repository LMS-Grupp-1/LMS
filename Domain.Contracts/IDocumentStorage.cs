using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Contracts
{
    public interface IDocumentStorage
    {
        Task SaveAsync(
            string blobPath,
            Stream content,
            string contentType,
            CancellationToken cancellationToken);

        Task<Stream> OpenReadAsync(
            string blobPath,
            CancellationToken cancellationToken);

        Task DeleteAsync(
            string blobPath,
            CancellationToken cancellationToken);
    }
}
