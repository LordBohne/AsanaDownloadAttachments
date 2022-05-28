using System.Net.Http.Headers;
using AsanaClient;
using Konsole;

const string accessToken = "Your access token here!";
var client = new HttpClient();
client.DefaultRequestHeaders.Authorization =  new AuthenticationHeaderValue("Bearer", accessToken);
var asanaClientWithCredentials = new Client(client);
var exit = false;
while (!exit)
{
    var projects = await asanaClientWithCredentials.GetProjectsAsync();

    Console.WriteLine("Choose the project you want to download attachments from by typing in the number next to the name");
    Console.WriteLine(string.Join("\n", projects.Data.Select((x, index) => $"{index + 1} {x.Name}" )));

    var projectNameToDownload = Functions.GetProjectToDownloadFrom(projects);
    var projectTasks = await asanaClientWithCredentials.GetTasksForProjectAsync(projectNameToDownload.Gid);

    Console.WriteLine("Choose the task you want to download attachments from by typing in the number next to the name");
    Console.WriteLine(string.Join("\n", projectTasks.Data.Select((x, index) => $"{index + 1} {x.Name}" )));

    var taskToDownloadFrom = Functions.GetTaskToDownloadFrom(projectTasks);
    var attachmentsResponse = await asanaClientWithCredentials.GetAttachmentsForTaskAsync(taskToDownloadFrom.Gid);
    Console.WriteLine($"Downloading {attachmentsResponse.Data.Count} attachments.");

    var httpClientWithoutCredentials = new HttpClient();
    foreach (var attachment in attachmentsResponse.Data)
    {
        var attachmentInformation = await asanaClientWithCredentials.GetAttachmentAsync(attachment.Gid);
        var fileName = attachmentInformation.Data.Name;
        var tries = 1;
        while (File.Exists(fileName))
        {
            fileName = Path.GetFileNameWithoutExtension(attachmentInformation.Data.Name) + $"({tries})" + Path.GetExtension(attachmentInformation.Data.Name);
            tries++;
        }
        
        await using var fileStream = File.Create(fileName);
        var pb = new ProgressBar(PbStyle.SingleLine, 50);
        var progress = new Progress<float>(f =>
        {
            pb.Refresh((int)(f * 100), fileName);
        });
        await httpClientWithoutCredentials.DownloadAsync(attachmentInformation.Data.Download_url!.ToString(), fileStream, progress);
    }
    await Task.Delay(100);
    Console.Clear();
    Console.WriteLine("Finished downloading the attachments. Do you want to download more? Enter:");
    Console.WriteLine("Yes to start from the beginning.");
    Console.WriteLine("No to quit the application.");
    exit = !Functions.GetRepeatDecisionFromUser();
    Console.Clear();
}