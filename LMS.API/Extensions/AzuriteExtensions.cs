using System.Diagnostics;
using System.Net.Sockets;

namespace LMS.API.Extensions;

public static class AzuriteExtensions
{
	private const int BlobPort = 10000;

	// Local development only: starts Azurite in its own window if it is not already running.
	// Requires Azurite to be installed globally: npm install -g azurite
	public static async Task EnsureAzuriteRunningAsync(this WebApplication app)
	{
		if (!app.Environment.IsDevelopment() || await IsListeningAsync())
		{
			return;
		}

		var dataFolder = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"azurite-lms");
				
		var azuriteArguments = $"--blobPort {BlobPort} --location \"{dataFolder}\"";

		try
		{
			Process.Start(OperatingSystem.IsWindows()
				? new ProcessStartInfo
				{
					// Windows: opens a separate window that keeps running after the API stops.
					FileName = "cmd.exe",
					Arguments = $"/k azurite {azuriteArguments}",
					UseShellExecute = true // Opens a separate window that keeps running after the API stops.
				}
				: new ProcessStartInfo
				{
					// Linux and macOS: runs in the background and keeps running after the API stops.
					// How to stop program in Linux: pkill -f azurite.
					FileName = "/bin/sh",
					Arguments = $"-c \"nohup azurite {azuriteArguments.Replace("\"", "\\\"")} > /dev/null 2>&1 &\"",
					UseShellExecute = false
				});
		}
		catch (Exception ex)
		{
			throw new InvalidOperationException(
				"Could not start Azurite. Install it with 'npm install -g azurite' or start it manually.", ex);
		}

		// Wait up to 60 seconds for Azurite to start listening (first start on Windows can be slow).
		for (var i = 0; i < 120 && !await IsListeningAsync(); i++)
		{
			await Task.Delay(500);
		}

		if (!await IsListeningAsync())
		{
			throw new InvalidOperationException(
				$"Azurite did not start on port {BlobPort} within 60 seconds. Check that 'azurite' works in a new terminal " +
				"(install with 'npm install -g azurite'), then restart Visual Studio so it picks up the updated PATH.");
		}
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