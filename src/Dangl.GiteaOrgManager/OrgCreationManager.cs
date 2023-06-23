using Dangl.GiteaOrgManager.Client;
using System.Security.Cryptography;

namespace Dangl.GiteaOrgManager
{
    public class OrgCreationManager
    {
        private readonly OrgCreationOptions _options;
        private readonly HttpClient _httpClient;

        public OrgCreationManager(OrgCreationOptions options)
        {
            _options = options;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("token", _options.GiteaAccessToken);
        }

        public async Task CreateOrgAndSetUpMirrorRepositoriesAsync()
        {
            var sourceData = await GetSourceOrgDataAsync();

            var targetOrganization = await CreateNewOrganizationAsync();

            var targetRepositories = await CreateRepositoriesInTargetOrganizationAsync(targetOrganization,
                sourceData.repos);

            var pushUserCredentials = await CreateUserForPushAccessInMirrorRepositoriesAsync(targetOrganization);

            await ConfigurePushTargetsFromSourceToTargetRepositoriesAsync(sourceData.org,
                sourceData.repos,
                targetRepositories,
                pushUserCredentials.username,
                pushUserCredentials.password);
        }

        private async Task<(Organization org, List<Repository> repos)> GetSourceOrgDataAsync()
        {
            var orgClient = GetOrganizationClient();
            var sourceOrganization = await orgClient.OrgGetAsync(_options.SourceOrgName);

            var repos = new List<Repository>();
            var hasMore = true;
            var page = 1;
            while (hasMore)
            {
                var response = await orgClient.OrgListReposAsync(_options.SourceOrgName, page, null);
                var newRepos = response.Where(r => repos.All(rr => rr.Name != r.Name)).ToList();
                hasMore = newRepos.Any();
                if (newRepos.Any())
                {
                    repos.AddRange(newRepos);
                }
            }

            if (!string.IsNullOrWhiteSpace(_options.ExcludedRepos))
            {
                var splitExludes = _options.ExcludedRepos.Split(',').ToList();
                repos = repos.Where(r => !splitExludes.Any(rr => rr == r.Name))
                            .ToList();
            }

            return (sourceOrganization, repos);
        }

        private async Task<Organization> CreateNewOrganizationAsync()
        {
            var orgClient = GetOrganizationClient();

            var creationResult = await orgClient.OrgCreateAsync(new CreateOrgOption
            {
                Description = "This is only a code mirror intended for read-only operations. No upstream changes are propagated.",
                Full_name = _options.TargetOrgName,
                Repo_admin_change_team_access = false,
                Visibility = CreateOrgOptionVisibility.Private,
                Username = _options.TargetOrgName
            });

            return creationResult;
        }

        private async Task<List<Repository>> CreateRepositoriesInTargetOrganizationAsync(Organization targetOrganization,
            List<Repository> sourceRepositories)
        {
            var orgClient = GetOrganizationClient();

            var targetRepositories = new List<Repository>();
            foreach (var sourceRepository in sourceRepositories)
            {
                var targetRepository = await orgClient.CreateOrgRepoAsync(targetOrganization.Name, new CreateRepoOption
                {
                    Auto_init = false,
                    Description = "This is only a code mirror intended for read-only operations. No upstream changes are propagated.",
                    Name = sourceRepository.Name,
                    Private = true,
                    Trust_model = CreateRepoOptionTrust_model.Default,
                });

                targetRepositories.Add(targetRepository);
            }

            return targetRepositories;
        }

        private async Task<(string username, string password)> CreateUserForPushAccessInMirrorRepositoriesAsync(Organization targetOrganization)
        {
            var adminClient = GetAdminClient();

            var password = GetSecureRandomStringWithoutDashes(24);
            var username = $"{targetOrganization.Name}_{GetSecureRandomStringWithoutDashes(4)}_PushMirrorUser";

            var userCreationResult = await adminClient.AdminCreateUserAsync(new CreateUserOption
            {
                Email = $"{targetOrganization.Name}_PushMirrorUser@blackhole.dangl-it.com",
                Login_name = username,
                Password = password,
                Send_notify = false,
                Username = username,
                Must_change_password = false
            });

            var organizationClient = GetOrganizationClient();
            var teams = await organizationClient.OrgListTeamsAsync(targetOrganization.Name, 1, null);
            var ownerTeam = teams.Single(t => t.Name == "Owners");
            await organizationClient.OrgAddTeamMemberAsync(ownerTeam.Id!.Value, username);
            return (username, password);
        }

        private async Task ConfigurePushTargetsFromSourceToTargetRepositoriesAsync(Organization sourceOrg,
            List<Repository> sourceRepos,
            List<Repository> targetRepos,
            string pushUserName,
            string pushUserPassword)
        {
            var repositoryClient = GetRepositoryClient();
            foreach (var sourceRepo in sourceRepos)
            {
                var targetRepo = targetRepos.Single(tr => tr.Name == sourceRepo.Name);

                var pushMirror = await repositoryClient.RepoAddPushMirrorAsync(sourceOrg.Name,
                    sourceRepo.Name, new CreatePushMirrorOption
                    {
                        Interval = "24h0m0s",
                        Sync_on_commit = true,
                        Remote_address = targetRepo.Clone_url,
                        Remote_username = pushUserName,
                        Remote_password = pushUserPassword,
                    });

                // We want to initiate a push right away to ensure the new org
                // has up to date code
                await repositoryClient.RepoPushMirrorSyncAsync(sourceOrg.Name, sourceRepo.Name);
            }
        }

        private string GetSecureRandomStringWithoutDashes(int length)
        {
            var bytes = RandomNumberGenerator.GetBytes(length);
            var stringValue = BitConverter.ToString(bytes).Replace("-", string.Empty);
            return stringValue;
        }

        private OrganizationClient GetOrganizationClient()
        {
            return new OrganizationClient(_options.GiteaBaseUrl, _httpClient);
        }

        private RepositoryClient GetRepositoryClient()
        {
            return new RepositoryClient(_options.GiteaBaseUrl, _httpClient);
        }

        private AdminClient GetAdminClient()
        {
            return new AdminClient(_options.GiteaBaseUrl, _httpClient);
        }
    }
}
