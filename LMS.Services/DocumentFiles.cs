using Domain.Models.Exceptions.DocumentExceptions;
using LMS.Shared.Constants;

namespace LMS.Services;

public static class DocumentFiles
{	
	public static (string FileName, string ContentType) ValidateFileName(string? fileName)
	{
		fileName = (fileName ?? string.Empty).Replace('\\', '/');
		fileName = fileName[(fileName.LastIndexOf('/') + 1)..].Trim();

		if (string.IsNullOrWhiteSpace(fileName) ||
			fileName.Length > DocumentConstraints.MaxFileNameLength ||
			fileName.Any(char.IsControl))
		{
			throw new DocumentValidationException(
				"The file name is invalid or too long.");
		}

		string extension = Path.GetExtension(fileName);

		if (!DocumentConstraints.ContentTypes.TryGetValue(extension, out var contentType))
		{
			throw new DocumentValidationException(
				"This file type is not allowed.");
		}

		return (fileName, contentType);
	}
		
	public static async Task<MemoryStream> ReadAsync(
		Stream content,
		CancellationToken cancellationToken)
	{
		var result = new MemoryStream();

		try
		{
			byte[] buffer = new byte[81920];
			int bytesRead;

			while ((bytesRead = await content.ReadAsync(buffer.AsMemory(), cancellationToken)) > 0)
			{
				if (result.Length + bytesRead > DocumentConstraints.MaxFileSize)
				{
					throw new DocumentValidationException(
						$"The file cannot exceed {DocumentConstraints.MaxFileSize} bytes.");
				}

				await result.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
			}

			if (result.Length == 0)
			{
				throw new DocumentValidationException("The file cannot be empty.");
			}

			result.Position = 0;
			return result;
		}
		catch
		{
			result.Dispose();
			throw;
		}
	}
}
