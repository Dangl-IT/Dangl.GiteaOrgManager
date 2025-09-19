# Dangl.GiteaOrgManager

This is a small app that allows mirroring an organization on a Gitea instance. You need admin rights to do it, and it will basically create a new organization, with mirrored repositories. You can download it at Dangl**Docu**: https://docs.dangl-it.com/Projects/Dangl.GiteaOrgManager

The following CLI options are supported:

```
  -u, --url        Required. The base URL of the Gitea API instance, e.g. http://gitea.dangl.dev/api/v1

  -t, --token      Required. The Gitea access token to use for the API calls

  -s, --source     Required. The name of the source organization to mirror from

  -o, --org        Required. The name of the target organization to mirror to

  -e, --exclude    A list of repositories to exclude from the mirroring, comma separated
  
  -m, --skip-mirror    If enabled, then no automatic Gitea mirroring will be setup

  --help           Display this help screen.

  --version        Display version information.
  ```