using AragonProposalViewer.Models;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AragonProposalViewer.Pages
{
    public class IndexModel : PageModel
    {
        IConfiguration _config;
        public List<ProposalView> Proposals { get; set; } = new();
        private readonly ILogger<IndexModel> _logger;


        public IndexModel(ILogger<IndexModel> logger, IConfiguration config)
        {
            _config = config;
            _logger = logger;

            ITraceWriter traceWriter = new MemoryTraceWriter();
            var client = new GraphQLHttpClient(config["AragonGraphQLEndPoint"], new NewtonsoftJsonSerializer(new JsonSerializerSettings
            {
                TraceWriter = traceWriter,
                //                ContractResolver = new CamelCasePropertyNamesContractResolver { IgnoreIsSpecifiedMembers = true },
                MissingMemberHandling = MissingMemberHandling.Ignore,
                //               Converters = { new ConstantCaseEnumConverter() }
            }));
            string daoId = config["DAOId"];
            GraphQLResponse<ProposalResponse>? proposal = null;
            try
            {
                var proposalReq = new GraphQLHttpRequest
                {
                    Variables = new { daoId },
                    Query = "query ($daoId: String!){tokenVotingProposals(where: {dao: $daoId} orderBy: createdAt orderDirection: desc ){metadata createdAt open executed yes no abstain supportThreshold minVotingPower castedVotingPower totalVotingPower}}"
                };
                var res = client.SendQueryAsync<ProposalResponse>(proposalReq).Result;
                foreach (var prop in res.Data.tokenVotingProposals)
                {
                    var meta = GetIPFSContent(prop.metadata).Result;
                    Proposals.Add(new ProposalView { createdAt = Convert(prop.createdAt), summary = meta.summary, title = meta.title, resources = meta.resources, status = Convert(prop) });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Fetching proposals failed. Message: {ex.Message}");
            }
        }

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

        private ProposalView.Status Convert(Proposal prop)
        {
            if (prop.open) return ProposalView.Status.open;
            if (prop.executed) return ProposalView.Status.executed;
            if (Convert(prop.castedVotingPower) < Convert(prop.minVotingPower) || Convert(prop.yes) < Convert(prop.supportThreshold) || Convert(prop.yes) < Convert(prop.no)) return ProposalView.Status.failed;
            return ProposalView.Status.succeeded;
        }
        private double Convert(string cryptoInt)
        {
            double d = 0;
            if(!string.IsNullOrEmpty(cryptoInt))
            {
                foreach (var c in cryptoInt)
                {
                    if (char.IsDigit(c))
                    {
                        d += c - '0';
                        d *= 10;
                    }
                    else
                    {
                        throw new Exception($"Invalid character: {c}");
                    }
                }
                d /= (10 ^ 19);
            }
            return d;
        }

        private DateTime Convert(int secondsOffset)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0) + TimeSpan.FromSeconds(secondsOffset);
        }
    }

}
