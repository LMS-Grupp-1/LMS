using System.Diagnostics;
using System.Net.Sockets;

namespace LMS.API.Extensions;

public static class AzuriteExtensions
{
	private const int BlobPort = 10000;

	// Where Visual Studio installs its own copy of Azurite, below each <version>\<edition> folder.
	private static readonly string[] VisualStudioAzuritePath =
		["Common7", "IDE", "Extensions", "Microsoft", "Azure Storage Emulator", "azurite.exe"];

	// Local development only: starts Azurite if it is not already running.
	// Uses the copy that ships with Visual Studio when it exists, otherwise a global npm install.
	public static async Task EnsureAzuriteRunningAsync(this WebApplication app)
	{
		if (!app.Environment.IsDevelopment() || await IsListeningAsync())
		{
			return;
		}

		var dataFolder = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"azurite-lms");

		// Create the folder first so Azurite does not fail on a missing path.
		Directory.CreateDirectory(dataFolder);

		var azuriteArguments = $"--blobPort {BlobPort} --location \"{dataFolder}\"";

		var startInfo = CreateStartInfo(azuriteArguments)
			?? throw new InvalidOperationException(
				"Azurite was not found. Install the Azure development tools in Visual Studio, " +
				"or run 'npm install -g azurite' (Rider, Linux and macOS). See README.md.");

		app.Logger.LogInformation("Starting Azurite: {FileName}", startInfo.FileName);
		Process.Start(startInfo);

		// Wait up to 60 seconds for Azurite to start listening (first start on Windows can be slow).
		for (var i = 0; i < 120 && !await IsListeningAsync(); i++)
		{
			await Task.Delay(500);
		}

		if (!await IsListeningAsync())
		{
			throw new InvalidOperationException(
				$"Azurite was started but did not listen on port {BlobPort} within 60 seconds. " +
				$"Check the Azurite window for errors, or whether another program uses port {BlobPort}.");
		}
	}

	// Returns how to start Azurite on this machine, or null when it is not installed.
	private static ProcessStartInfo? CreateStartInfo(string azuriteArguments)
	{
		if (OperatingSystem.IsWindows())
		{
			// 1. The copy that ships with Visual Studio: no Node needed.
			var visualStudioAzurite = FindVisualStudioAzurite();
			if (visualStudioAzurite is not null)
			{
				return new ProcessStartInfo(visualStudioAzurite, azuriteArguments)
				{
					// Opens a separate window that keeps running after the API stops.
					UseShellExecute = true
				};
			}

			// 2. A global npm install. cmd.exe always starts, so check the PATH first
			// to fail fast instead of waiting 60 seconds for something that is not there.
			if (!ExistsOnPath("azurite.cmd"))
			{
				return null;
			}

			return new ProcessStartInfo("cmd.exe", $"/k azurite {azuriteArguments}")
			{
				UseShellExecute = true
			};
		}

		// Linux and macOS: a global npm install, run in the background.
		// How to stop it: pkill -f azurite
		if (!ExistsOnPath("azurite"))
		{
			return null;
		}

		return new ProcessStartInfo
		{
			FileName = "/bin/sh",
			Arguments = $"-c \"nohup azurite {azuriteArguments.Replace("\"", "\\\"")} > /dev/null 2>&1 &\"",
			UseShellExecute = false
		};
	}

	// Looks in every installed version and edition, e.g. 18\Professional or 2022\Community.
	private static string? FindVisualStudioAzurite()
	{
		var visualStudioRoot = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
			"Microsoft Visual Studio");

		if (!Directory.Exists(visualStudioRoot))
		{
			return null;
		}

		return Directory.EnumerateDirectories(visualStudioRoot)
			.SelectMany(Directory.EnumerateDirectories)
			.Select(editionFolder => Path.Combine([editionFolder, .. VisualStudioAzuritePath]))
			.FirstOrDefault(File.Exists);
	}

	private static bool ExistsOnPath(string fileName)
	{
		var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;

		return path
			.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
			.Any(folder => File.Exists(Path.Combine(folder, fileName)));
	}

	private static async Task<bool> IsListeningAsync()
	{
		try
		{
			using var client = new TcpClient();
			await client.ConnectAsync("127.0.0.1", BlobPort);
			return true;
		}
		catch (SocketException)
		{
			return false;
		}
	}
}