using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Shared.DTOs.FeedbackDtos;
using Service.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace LMS.Services;

public class FeedbackService : IFeedbackService
{
    private readonly IUnitOfWork _unitOfWork;

    public FeedbackService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<FeedbackDto>> GetFeedbacksBySubmissionIdAsync(int submissionId, CancellationToken cancellationToken = default)
    {
        var feedbacks = await _unitOfWork.Feedbacks.GetFeedbacksBySubmissionIdAsync(submissionId, cancellationToken);

        return feedbacks.Select(f => new FeedbackDto
        {
            Id = f.Id,
            SubmissionId = f.SubmissionId,
            TeacherId = f.TeacherId,
            TeacherName = f.Teacher?.UserName ?? string.Empty,
            Comment = f.Comment,
            CreatedAt = f.CreatedAt
        });
    }

    public async Task CreateFeedbackAsync(FeedbackFormDto dto, string teacherId, CancellationToken cancellationToken = default)
    {
        var feedback = new Feedback
        {
            SubmissionId = dto.SubmissionId,
            Comment = dto.Comment,
            TeacherId = teacherId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _unitOfWork.Feedbacks.AddAsync(feedback);
        await _unitOfWork.CompleteAsync();
    }
}
