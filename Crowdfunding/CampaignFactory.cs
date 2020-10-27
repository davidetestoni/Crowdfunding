using Crowdfunding.Models;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Crowdfunding
{
    public class CampaignFactory
    {
        private readonly Web3 web3;
        private readonly Contract contract;

        // Internal constructor to create an instance of this class given its address
        internal CampaignFactory(Contract contract, Web3 web3)
        {
            this.contract = contract;
            this.web3 = web3;
        }

        /// <summary>
        /// Deploys a new <see cref="CampaignFactory"/> contract on the blockchain.
        /// </summary>
        /// <param name="manager">The address of the account that created the campaign.</param>
        public static async Task<CampaignFactory> Deploy(Web3 web3, string manager)
        {
            // Read the ABI and the bytecode from the solidity compiler output files
            var abi = File.ReadAllText(Path.Combine("Contracts", "bin", "CampaignFactory.abi"));
            var bytecode = File.ReadAllText(Path.Combine("Contracts", "bin", "CampaignFactory.bin"));

            // Deploy the contract and get the address
            var transactionReceipt = await web3.Eth.DeployContract.SendRequestAndWaitForReceiptAsync(bytecode, manager, new HexBigInteger(1000000));
            var contractAddress = transactionReceipt.ContractAddress;

            // Get the contract
            var contract = web3.Eth.GetContract(abi, contractAddress);
            return new CampaignFactory(contract, web3);
        }

        /// <summary>
        /// Retrieves a <see cref="CampaignFactory"/> from the blockchain at a given <paramref name="address"/>.
        /// </summary>
        public static CampaignFactory FromChain(Web3 web3, string address)
        {
            // Read the ABI and the bytecode from the solidity compiler output files
            var abi = File.ReadAllText(@"Contracts\bin\CampaignFactory.abi");
            
            // Get the contract
            var contract = web3.Eth.GetContract(abi, address);
            return new CampaignFactory(contract, web3);
        }

        /// <summary>
        /// Deploys a new <paramref name="campaign"/> on the blockchain and immediately retrieves it.
        /// </summary>
        /// <param name="manager">The address of the account that created the campaign.</param>
        /// <param name="minimum">The minimum amount to pledge in order to qualify as approver (in wei).</param>
        public async Task<Campaign> CreateCampaign(string manager, uint minimum)
        {
            var createFunction = contract.GetFunction("createCampaign");
            await createFunction.SendTransactionAndWaitForReceiptAsync(manager, new HexBigInteger(1000000),
                new HexBigInteger(0), null, minimum);

            var countFunction = contract.GetFunction("campaignsCount");
            var campaignsCount = await countFunction.CallAsync<uint>();

            var getFunction = contract.GetFunction("deployedCampaigns");
            var campaignAddress = await getFunction.CallAsync<string>(campaignsCount - 1);

            return await Campaign.FromChain(web3, campaignAddress);
        }

        /// <summary>
        /// Gets a list of all campaigns that the factory deployed on the blockchain.
        /// </summary>
        public async Task<Campaign[]> GetCampaigns()
        {
            var countFunction = contract.GetFunction("campaignsCount");
            var campaignsCount = await countFunction.CallAsync<uint>();

            var function = contract.GetFunction("deployedCampaigns");

            var campaigns = new List<Campaign>();
            for (int i = 0; i < campaignsCount; i++)
            {
                var address = await function.CallAsync<string>(i);
                campaigns.Add(await Campaign.FromChain(web3, address));
            }

            return campaigns.ToArray();
        }
    }
}
