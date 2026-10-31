using LMS.Shared.DTOs.ActivityDtos;
using Domain.Models.Entities;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.Json;

namespace LMS.Services;
    internal static class ActivityDeletionPreviewFactory
    {
    public static ActivityDeletionPreviewDto Create(Activity activity)
    {
        var documents = activity.Documents
                .OrderBy(document => document.Id)
                .ToList();

        var assignments = activity.Assignments
            .OrderBy(assignment => assignment.Id)
            .ToList();

        var snapshot = new
        {
            activity.Id,
            activity.ModuleId,
            activity.Name,
            Documents = documents.Select(document => new
            {
                document.Id,
                document.Name,
                document.FileName,
                document.BlobPath
            }).ToArray(),
            Assignments = assignments.Select(assignment => new
            {
                assignment.Id,
                assignment.Name,
                Submissions = assignment.Submissions
                    .OrderBy(submission => submission.Id)
                    .Select(submission => new
                    {
                        submission.Id,
                        submission.DocumentId,
                        submission.Document.BlobPath,
                        FeedbackIds = submission.Feedbacks
                            .OrderBy(feedback => feedback.Id)
                            .Select(feedback => feedback.Id)
                            .ToArray()
                    })
                    .ToArray()
            }).ToArray()
        };

        string version = Convert.ToHexString(
            SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(snapshot)));

        return new ActivityDeletionPreviewDto(
            activity.Id,
            activity.Name,
            documents.Select(document => new ActivityDeletionDocumentDto(
                document.Id,
                document.Name,
                document.FileName)).ToList(),
            assignments.Select(assignment => new ActivityDeletionAssignmentDto(
                assignment.Id,
                assignment.Name,
                assignment.Submissions.Count,
                assignment.Submissions.Sum(submission =>
                    submission.Feedbacks.Count))).ToList())
        {
            Version = version
        };
    }
    }
