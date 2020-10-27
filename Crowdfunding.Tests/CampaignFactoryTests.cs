using Crowdfunding.Models;
using Nethereum.Web3;
using System.Threading.Tasks;
using Xunit;

namespace Crowdfunding.Tests
{
    public class CampaignFactoryTests
    {
        [Fact]
        public async Task Deploy_Standard_DeploysContract()
        {
            var web3 = new Web3();
            var accounts = await web3.Eth.Accounts.SendRequestAsync();
            await web3.Personal.UnlockAccount.SendRequestAsync(accounts[0], "password", 120);
            await CampaignFactory.Deploy(web3, accounts[0]);
        }

        [Fact]
        public async Task CreateCampaign_Standard_CreatesCampaign()
        {
            var web3 = new Web3();
            var accounts = await web3.Eth.Accounts.SendRequestAsync();
            await web3.Personal.UnlockAccount.SendRequestAsync(accounts[0], "password", 120);
            var factory = await CampaignFactory.Deploy(web3, accounts[0]);

            var campaign = await factory.CreateCampaign(accounts[0], 100);

            Assert.Equal<uint>(100, campaign.MinimumContribution);
            Assert.Equal(accounts[0], campaign.Manager);
        }
    }
}
