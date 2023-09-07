# AragonProposalViewer

This is a demonstration Aragon proposal viewer using Asp.net core.
It may be useful to include a list of proposals in your website.

appsettings.json contains the config for the app
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "DAOId": "<Your DAO ID here>",
  "AragonGraphQLEndPoint": "https://subgraph.satsuma-prod.com/qHR2wGfc5RLi6/aragon/osx-polygon/api",
  "AragonIPFSEndPoint": "https://prod.ipfs.aragon.network/api/v0/",
  "AragonIPFSKey": "b477RhECf8s8sdM7XrkLBs2wHc4kCMwpbcFC55Kt"
}
```
Add your own DAO id to make it work.
There are two sets of remote calls used. The first is two the Aragon GraphQL Server.
Note that the example is set for the polygon chain. By changing __AragonGraphQLEndPoint__ you can get proposals for DAOs on other chains.
See the Aragon documentation for urls.

The GraphQL call to get a list of proposals for a given DAO is:
```graphql
query ($daoId: String!)
{
  tokenVotingProposals(where: {dao: $daoId} orderBy: createdAt orderDirection: desc )
  {
    metadata
    createdAt
    open
    executed
    yes
    no
    abstain
    supportThreshold
    minVotingPower
    castedVotingPower
    totalVotingPower
  }
}
```
You can no-doubt adapt this for other purposes. Using a web browser to access the GraphQL endpoint brings up a playground editor you can use to create new expressions.
The second set of calls is to the Aragon IPFS server.
There are two, test and production. I've used the production server here, because the test server is rate-limited, and I want to load several proposals in rapid succession.
I'm only reading, so this does not burden the server.

The IPFS call fetches the proposal metadata, containing the title, summary and a list of related document urls.

```C#
        public async Task<Metadata> GetIPFSContent(string ipfsId)
        {
            HttpClient _ipfsClient;
            _ipfsClient = new HttpClient();
            _ipfsClient.BaseAddress = new Uri(_config["AragonIPFSEndPoint"]);
            _ipfsClient.DefaultRequestHeaders.Add("X-API-KEY", _config["AragonIPFSKey"]);
            var cid = ipfsId.Replace("ipfs://", "");
            using var res = await _ipfsClient.PostAsync($"cat?arg={cid}", null);
            var str = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Metadata>(str); ;
        }
```
