using LMS.Blazor.Client.Services.ApiProxy;
using LMS.Shared.Constants;
using LMS.Shared.DTOs.AssignmentDtos;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Headers;

namespace LMS.Blazor.Client.Services
{
	public class AssignmentService(IApiProxyClient apiProxy) : IAssignmentService
	{
		private readonly IApiProxyClient _apiProxy = apiProxy;

		public async Task<IReadOnlyList<StudentAssignmentDto>?> GetMyAssignmentsAsync(CancellationToken cancellationToken = default)
		{
			return await _apiProxy.GetAsync<List<StudentAssignmentDto>>("assignments/my", cancellationToken);
		}

		public async Task<(StudentAssignmentDto? Assignment, string? ErrorMessage)> SubmitAsync(
		int assignmentId,
		IBrowserFile file,
		CancellationToken cancellationToken = default)
		{
			using var form = new MultipartFormDataContent();

			// The size limit is checked again by the API; this stops large files before they are sent.
			using var stream = file.OpenReadStream(DocumentConstraints.MaxFileSize, cancellationToken);
			using var fileContent = new StreamContent(stream);
			fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

			// "File" must match the property name in SubmissionUploadRequest.
			form.Add(fileContent, "File", file.Name);

			return await _apiProxy.SendAndHandleErrorAsync<StudentAssignmentDto>(
				HttpMethod.Post,
				$"assignments/{assignmentId}/submissions",
				form,
				cancellationToken);
		}
	}
}
