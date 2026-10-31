using Azure.Storage.Blobs.Models;
using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using LMS.Shared.DTOs.DocumentDtos;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly ApplicationDbContext _context;

    public DocumentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int?> ResolveCourseAsync(
        DocumentTarget target,
        CancellationToken cancellationToken)
    {
        if (target.CourseId is int courseId)
        {
            return await _context.Courses
                .Where(course => course.Id == courseId)
                .Select(course => (int?)course.Id)
                .SingleOrDefaultAsync(cancellationToken);
        }

        if (target.ModuleId is int moduleId)
        {
            return await _context.Modules
                .Where(module => module.Id == moduleId)
                .Select(module => (int?)module.CourseId)
                .SingleOrDefaultAsync(cancellationToken);
        }

        if (target.ActivityId is int activityId)
        {
            return await _context.Activities
                .Where(activity => activity.Id == activityId)
                .Select(activity => (int?)activity.Module.CourseId)
                .SingleOrDefaultAsync(cancellationToken);
        }

        return null;
    }

    public async Task<int?> GetStudentCourseAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        return await _context.Users
            .Where(user => user.Id == userId)
            .Select(user => user.CourseId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<string?> GetUploaderNameAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        return await _context.Users
            .Where(user => user.Id == userId)
            .Select(user => user.FirstName + " " + user.LastName)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DocumentDto>> ListAsync(
        int courseId,
        CancellationToken cancellationToken)
    {
        return await _context.Documents
            .AsNoTracking()
            .Include(document => document.Activity)
            .Where(document =>
                document.CourseId == courseId ||
                (document.Module != null &&
                    document.Module.CourseId == courseId) ||
                (document.Activity != null &&
                    document.Activity.Module.CourseId == courseId))
            .OrderByDescending(document => document.UploadedAt)
            .ThenByDescending(document => document.Id)
            .Select(document => new DocumentDto(
                document.Id,
                document.Name,
                document.Description,
                document.FileName,
                document.ContentType,
                document.SizeBytes,
                document.UploadedAt,
                document.UploadedBy.FirstName + " " +
                    document.UploadedBy.LastName,
                courseId,
                document.ModuleId != null ? document.ModuleId 
                    : (document.ActivityId != null ? document.Activity.ModuleId : null),
                document.ActivityId,
                document.CourseId != null
                    ? "Kursmaterial"
                    : document.Module != null
                        ? "Modul: " + document.Module.Name
                        : document.Activity != null
                            ? "Aktivitet: " +
                                document.Activity.Module.Name + " / " +
                                document.Activity.Name
                            : ""))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DocumentTargetDto>> TargetsAsync(
        int courseId,
        CancellationToken cancellationToken)
    {
        var courseExists = await _context.Courses
            .AnyAsync(
                course => course.Id == courseId,
                cancellationToken);

        if (!courseExists)
        {
            return Array.Empty<DocumentTargetDto>();
        }

        var targets = new List<DocumentTargetDto>();

        var modules = await _context.Modules
            .AsNoTracking()
            .Where(module => module.CourseId == courseId)
            .OrderBy(module => module.Name)
            .Select(module => new
            {
                module.Id,
                module.Name
            })
            .ToListAsync(cancellationToken);

        targets.AddRange(modules.Select(module =>
            new DocumentTargetDto(
                new DocumentTarget(ModuleId: module.Id),
                "Modul: " + module.Name)));

        var activities = await _context.Activities
            .AsNoTracking()
            .Where(activity => activity.Module.CourseId == courseId)
            .OrderBy(activity => activity.Module.Name)
            .ThenBy(activity => activity.Name)
            .Select(activity => new
            {
                activity.Id,
                activity.Name,
                ModuleName = activity.Module.Name
            })
            .ToListAsync(cancellationToken);

        targets.AddRange(activities.Select(activity =>
            new DocumentTargetDto(
                new DocumentTarget(ActivityId: activity.Id),
                "Aktivitet: " + activity.ModuleName + " / " +
                    activity.Name)));

        return targets;
    }

    public async Task<Document?> FindAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await _context.Documents
            .AsNoTracking()
            .SingleOrDefaultAsync(
                document => document.Id == id,
                cancellationToken);
    }

    public async Task AddAsync(
        Document document,
        CancellationToken cancellationToken)
    {
        await _context.Documents.AddAsync(
            document,
            cancellationToken);
    }

    public async Task<Document?> FindForManagementAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await _context.Documents
            .Where(document =>
            !_context.Submissions.Any(submission =>
                submission.DocumentId == document.Id))
            .SingleOrDefaultAsync(
            document => document.Id == id,
            cancellationToken);
    }
    public void Delete(Document document)
    {
        _context.Documents.Remove(document);
    }
}