namespace AsanaClient;

public static class Functions
{
    public static ProjectCompact GetProjectToDownloadFrom(Response66 projects)
    {
        ProjectCompact? projectToDownload = null;
        var projectList = projects.Data.ToList();
        while (projectToDownload is null)
        {
            var userInput = Console.ReadLine();
            if (!int.TryParse(userInput, out var index))
            {
                Console.WriteLine("Please enter a valid number.");
                continue;
            }
            if (index > projectList.Count || index < 1)
            {
                Console.WriteLine($"Please enter a number in the range of {1} to {(projectList.Count - 1 < 1 ? 1 : projectList.Count)}");
                continue;
            }

            projectToDownload = projectList[index - 1];
        }

        return projectToDownload;
    }
    
    public static TaskCompact GetTaskToDownloadFrom(Response115 tasks)
    {
        TaskCompact? projectToDownload = null;
        var taskList = tasks.Data.ToList();
        while (projectToDownload is null)
        {
            var userInput = Console.ReadLine();
            if (!int.TryParse(userInput, out var index))
            {
                Console.WriteLine("Please enter a valid number.");
                continue;
            }
            if (index > taskList.Count || index < 1)
            {
                Console.WriteLine($"Please enter a number in the range of {1} to {(taskList.Count - 1 < 1 ? 1 : taskList.Count)}");
                continue;
            }

            var project = taskList[index - 1];
            projectToDownload = project;
        }

        return projectToDownload;
    }

    public static bool GetRepeatDecisionFromUser()
    {
        var userInput = Console.ReadLine();
        return userInput?.ToLower() is "yes";
    }
    
    public static async Task DownloadAsync(this HttpClient client, string requestUri, Stream destination, IProgress<float> progress = null, CancellationToken cancellationToken = default)
    {
        // Get the http headers first to examine the content length
        using var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var contentLength = response.Content.Headers.ContentLength;

        await using var download = await response.Content.ReadAsStreamAsync(cancellationToken);
        
        // Ignore progress reporting when no progress reporter was 
        // passed or when the content length is unknown
        if (progress == null || !contentLength.HasValue) {
            await download.CopyToAsync(destination, cancellationToken);
            return;
        }

        // Convert absolute progress (bytes downloaded) into relative progress (0% - 100%)
        var relativeProgress = new Progress<long>(totalBytes => progress.Report((float)totalBytes / contentLength.Value));
        // Use extension method to report progress while downloading
        await download.CopyToAsync(destination, 81920, relativeProgress, cancellationToken);
        progress.Report(1);
    }
}

public static class StreamExtensions
{
    public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, IProgress<long> progress = null, CancellationToken cancellationToken = default) {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        if (!source.CanRead)
            throw new ArgumentException("Has to be readable", nameof(source));
        if (destination == null)
            throw new ArgumentNullException(nameof(destination));
        if (!destination.CanWrite)
            throw new ArgumentException("Has to be writable", nameof(destination));
        if (bufferSize < 0)
            throw new ArgumentOutOfRangeException(nameof(bufferSize));

        var buffer = new byte[bufferSize];
        long totalBytesRead = 0;
        int bytesRead;
        while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0) {
            await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
            totalBytesRead += bytesRead;
            progress?.Report(totalBytesRead);
        }
    }
}