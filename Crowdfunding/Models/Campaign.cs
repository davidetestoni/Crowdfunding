using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Crowdfunding.Models
{
    public class Campaign
    {
        private readonly Contract contract;
        private readonly Web3 web3;
        public uint MinimumContribution { get; private set; }
        public string Manager { get; private set; }
        public string Address => contract.Address;

        internal Campaign(Contract contract, Web3 web3)
        {
            this.contract = contract;
            this.web3 = web3;
        }

        /// <summary>
        /// Retrieves a <see cref="Campaign"/> from the blockchain at a given <paramref name="address"/>.
        /// </summary>
        public static async Task<Campaign> FromChain(Web3 web3, string address)
        {
            // Read the ABI and the bytecode from the solidity compiler output files
            var abi = File.ReadAllText(Path.Combine("Contracts", "bin", "Campaign.abi"));

            // Get the contract
            var contract = web3.Eth.GetContract(abi, address);

            return new Campaign(contract, web3)
            {
                MinimumContribution = await contract.GetFunction("minimumContribution").CallAsync<uint>(),
                Manager = await contract.GetFunction("manager").CallAsync<string>()
            };
        }

        /// <summary>
        /// Contributes to a campaign.
        /// </summary>
        /// <param name="from">The account that contributes.</param>
        /// <param name="amount">The amount to contribute.</param>
        public async Task Contribute(string from, HexBigInteger amount)
        {
            var function = contract.GetFunction("contribute");
            await function.SendTransactionAndWaitForReceiptAsync(from, new HexBigInteger(1000000), amount);
        }

        /// <summary>
        /// Checks if someone is a contributor.
        /// </summary>
        /// <param name="address">The account to check.</param>
        public async Task<bool> IsContributor(string address)
        {
            var function = contract.GetFunction("contributors");
            return await function.CallAsync<bool>(address);
        }

        /// <summary>
        /// Creates a spending request with the given parameters.
        /// </summary>
        /// <param name="from">The account that is creating the spending request (must be the manager).</param>
        /// <param name="description">The description of the spending request.</param>
        /// <param name="value">The value that is being required in wei.</param>
        /// <param name="recipient">The account where the required money will be sent upon approval.</param>
        /// <returns></returns>
        public async Task CreateSpendingRequest(string from, string description, uint value, string recipient)
        {
            var function = contract.GetFunction("createRequest");
            await function.SendTransactionAndWaitForReceiptAsync(from, new HexBigInteger(1000000), new HexBigInteger(0),
                null, description, value, recipient);
        }

        /// <summary>
        /// Gets a spending request by <paramref name="index"/>.
        /// </summary>
        public async Task<SpendingRequest> GetSpendingRequest(int index)
        {
            var function = contract.GetFunction("requests");
            var request = await function.CallDeserializingToObjectAsync<SpendingRequest>(index);
            request.Index = index;
            return request;
        }

        /// <summary>
        /// Gets a list of all campaigns that the factory deployed on the blockchain.
        /// </summary>
        public async Task<SpendingRequest[]> GetSpendingRequests()
        {
            var countFunction = contract.GetFunction("requestsCount");
            var requestsCount = await countFunction.CallAsync<uint>();

            var function = contract.GetFunction("requests");

            var requests = new List<SpendingRequest>();
            for (int i = 0; i < requestsCount; i++)
            {
                var request = await function.CallDeserializingToObjectAsync<SpendingRequest>(i);
                request.Index = i;
                requests.Add(request);
            }

            return requests.ToArray();
        }

        /// <summary>
        /// Votes for the approval of a spending request.
        /// </summary>
        /// <param name="from">The voting account.</param>
        /// <param name="index">The index of the spending request to approve.</param>
        public async Task ApproveSpendingRequest(string from, uint index)
        {
            var function = contract.GetFunction("approveRequest");
            await function.SendTransactionAndWaitForReceiptAsync(from, new HexBigInteger(1000000), new HexBigInteger(0), null, index);
        }

        /// <summary>
        /// Finalizes a spending requests (marks it as complete and transfers the money to the recipient).
        /// </summary>
        /// <param name="from">Who is asking for finalization (should be the manager).</param>
        /// <param name="index">The index of the request to finalize.</param>
        /// <returns></returns>
        public async Task FinalizeSpendingRequest(string from, uint index)
        {
            var function = contract.GetFunction("finalizeRequest");
            await function.SendTransactionAndWaitForReceiptAsync(from, new HexBigInteger(1000000), new HexBigInteger(0), null, index);
        }
    }
}
