using CommandLine;

namespace Dangl.GiteaOrgManager
{
    public class OrgCreationOptions
    {
        [Option('u', "url", Required = true, HelpText = "The base URL of the Gitea API instance, e.g. http://gitea.dangl.dev/api/v1")]
        public string GiteaBaseUrl { get; set; } = string.Empty;

        [Option('t', "token", Required = true, HelpText = "The Gitea access token to use for the API calls")]
        public string GiteaAccessToken { get; set; } = string.Empty;

        [Option('s', "source", Required = true, HelpText = "The name of the source organization to mirror from")]
        public string SourceOrgName { get; set; } = string.Empty;

        [Option('o', "org", Required = true, HelpText = "The name of the target organization to mirror to")]
        public string TargetOrgName { get; set; } = string.Empty;

        [Option('e', "exclude", Required = false, HelpText = "A list of repositories to exclude from the mirroring, comma separated")]
        public string? ExcludedRepos { get; set; }

        [Option('m', "skip-mirror", Required = false, HelpText = "If enabled, then no automatic Gitea mirroring will be setup")]
        public bool SkipMirrorSetup { get; set; }
    }
}
