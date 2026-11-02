using Domain.Models.Entities;
using LMS.Shared.DTOs.DocumentDtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Contracts
{
    public interface IDocumentRepository
    {
        Task<int?> ResolveCourseAsync(
            DocumentTarget target,
            CancellationToken cancellationToken);

        Task<int?> GetStudentCourseAsync(
            string userId,
            CancellationToken cancellationToken);

        Task<string?> GetUploaderNameAsync(
            string userId,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<DocumentDto>> ListAsync(
            int courseId,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<DocumentTargetDto>> TargetsAsync(
            int courseId,
            CancellationToken cancellationToken);

        Task<Document?> FindAsync(
            int id,
            CancellationToken cancellationToken);

        Task AddAsync(
            Document document,
            CancellationToken cancellationToken);

        Task<Document?> FindForManagementAsync(
            int id,
            CancellationToken cancellationToken);

        void Delete(Document document);
    }
}
