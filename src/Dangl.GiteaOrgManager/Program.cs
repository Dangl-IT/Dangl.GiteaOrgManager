using CommandLine;
using CommandLine.Text;

namespace Dangl.GiteaOrgManager
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            HeadingInfo.Default.WriteMessage("Visit https://www.dangl-it.com to find out more about this exporter");
            HeadingInfo.Default.WriteMessage("This generator is available on GitHub: https://github.com/GeorgDangl/Dangl.SevDeskExport");
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
