using CommandLine;

namespace Dangl.GiteaOrgManager
{
    public class OrgCreationOptions
    {
        [Option('u', "url", Required = true, HelpText = "The base URL of the Gitea API instance, e.g. http://gitea.dangl.dev/api/v1")]
        public string GiteaBaseUrl { get; set; }

        [Option('t', "token", Required = true, HelpText = "The Gitea access token to use for the API calls")]
        public string GiteaAccessToken { get; set; }

        [Option('s', "source", Required = true, HelpText = "The name of the source organization to mirror from")]
        public string SourceOrgName { get; set; }

        [Option('o', "org", Required = true, HelpText = "The name of the target organization to mirror to")]
        public string TargetOrgName { get; set; }

        [Option('e', "exclude", Required = false, HelpText = "A list of repositories to exclude from the mirroring")]
        public string[] ExcludedRepos { get; set; }
    }
}
