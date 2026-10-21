using System.Globalization;
using System.Net.Http.Headers;
using LMS.Blazor.Client.Services.ApiProxy;
using LMS.Shared.Constants;
using LMS.Shared.DTOs.DocumentDtos;
using Microsoft.AspNetCore.Components.Forms;

namespace LMS.Blazor.Client.Services;

public sealed class DocumentService(IApiProxyClient apiProxy)
{
    public async Task<List<DocumentDto>> ListAsync(
        int courseId,
        CancellationToken cancellationToken)
    {
        return await apiProxy.SendAsync<List<DocumentDto>>(
            HttpMethod.Get,
            $"documents?courseId={courseId}",
            cancellationToken: cancellationToken) ?? [];
    }

    public async Task<List<DocumentTargetDto>> TargetsAsync(
        int courseId,
        CancellationToken cancellationToken)
    {
        return await apiProxy.SendAsync<List<DocumentTargetDto>>(
            HttpMethod.Get,
            $"documents/targets?courseId={courseId}",
            cancellationToken: cancellationToken) ?? [];
    }

    public async Task<(DocumentDto? Document, string? ErrorMessage)> UploadAsync(
        DocumentTarget target,
        string name,
        string description,
        IBrowserFile file,
        CancellationToken cancellationToken)
    {
        using var form = new MultipartFormDataContent();

        form.Add(new StringContent(name.Trim()), "Name");
        form.Add(new StringContent(description.Trim()), "Description");

        AddId(form, "CourseId", target.CourseId);
        AddId(form, "ModuleId", target.ModuleId);
        AddId(form, "ActivityId", target.ActivityId);

        using var stream = file.OpenReadStream(
            DocumentConstraints.MaxFileSize,
            cancellationToken);

        using var fileContent = new StreamContent(stream);

        fileContent.Headers.ContentType =
            new MediaTypeHeaderValue("application/octet-stream");

        form.Add(fileContent, "File", file.Name);

        var document = await apiProxy.SendAndHandleErrorAsync<DocumentDto>(
            HttpMethod.Post,
            "documents",
            form,
            cancellationToken);
    }

    private static void AddId(
        MultipartFormDataContent form,
        string fieldName,
        int? value)
    {
        if (value.HasValue)
        {
            form.Add(
                new StringContent(
                    value.Value.ToString(CultureInfo.InvariantCulture)),
                fieldName);
        }
    }
}