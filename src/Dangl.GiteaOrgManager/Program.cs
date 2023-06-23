using CommandLine.Text;
using CommandLine;

namespace Dangl.GiteaOrgManager
{
    internal static class Program
    {
        static async Task Main(string[] args)
        {

            HeadingInfo.Default.WriteMessage("Visit https://www.dangl-it.com to find out more about this tool");
            HeadingInfo.Default.WriteMessage("This tool is available on GitHub: https://github.com/Dangl-IT/Dangl.GiteaOrgManager");
            HeadingInfo.Default.WriteMessage($"Version {VersionInfo.Version}");
            await Parser.Default.ParseArguments<OrgCreationOptions>(args)
                .MapResult(async options =>
                {
                    try
                    {
                        var creationManager = new OrgCreationManager(options);
                        await creationManager.CreateOrgAndSetUpMirrorRepositoriesAsync();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                },
                errors =>
                {
                    Console.WriteLine("Could not parse CLI arguments");
                    return Task.CompletedTask;
                }).ConfigureAwait(false);
        }
    }
}