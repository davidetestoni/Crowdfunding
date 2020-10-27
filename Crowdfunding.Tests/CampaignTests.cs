using Crowdfunding.Models;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Crowdfunding.Tests
{
    // Execute this once before all tests
    public class FactoryFixture
    {
        public CampaignFactory Factory { get; private set; }
        public string[] Accounts { get; private set; }
        public Web3 Web3 { get; private set; }

        public FactoryFixture()
        {
            Web3 = new Web3();
            Accounts = Web3.Eth.Accounts.SendRequestAsync().Result;
            Web3.Personal.UnlockAccount.SendRequestAsync(Accounts[0], "password", 120).Wait();
            Web3.Personal.UnlockAccount.SendRequestAsync(Accounts[1], "password", 120).Wait();
            Factory = CampaignFactory.Deploy(Web3, Accounts[0]).Result;
        }
    }

    public class CampaignTests : IClassFixture<FactoryFixture>
    {
        FactoryFixture factoryFixture;

        public CampaignTests(FactoryFixture fixture)
        {
            factoryFixture = fixture;
        }

        [Fact]
        public async Task Contribute_MoreThanMinimum_SetAsApprover()
        {
            var campaign = await factoryFixture.Factory.CreateCampaign(factoryFixture.Accounts[0], 100);
            await campaign.Contribute(factoryFixture.Accounts[0], new HexBigInteger(200));

            Assert.True(await campaign.IsContributor(factoryFixture.Accounts[0]));
        }

        [Fact]
        public async Task Contribute_LessThanMinimum_DontSetAsApprover()
        {
            var campaign = await factoryFixture.Factory.CreateCampaign(factoryFixture.Accounts[0], 100);
            await campaign.Contribute(factoryFixture.Accounts[0], new HexBigInteger(50));

            Assert.False(await campaign.IsContributor(factoryFixture.Accounts[0]));
        }

        [Fact]
        public async Task CreateSpendingRequest_FromManager_Allow()
        {
            var campaign = await factoryFixture.Factory.CreateCampaign(factoryFixture.Accounts[0], 100);
            await campaign.CreateSpendingRequest(factoryFixture.Accounts[0], "Buy batteries", 300, factoryFixture.Accounts[1]);

            var requests = await campaign.GetSpendingRequests();
            var request = requests.Last();

            Assert.Equal("Buy batteries", request.Description);
        }

        [Fact]
        public async Task CreateSpendingRequest_FromOther_Deny()
        {
            var campaign = await factoryFixture.Factory.CreateCampaign(factoryFixture.Accounts[0], 100);
            await Assert.ThrowsAnyAsync<Exception>(() => campaign
                .CreateSpendingRequest(factoryFixture.Accounts[1], "Buy batteries", 300, factoryFixture.Accounts[1]));
        }

        [Fact]
        public async Task ApproveSpendingRequest_Twice_ApproveOnlyOnce()
        {
            var campaign = await factoryFixture.Factory.CreateCampaign(factoryFixture.Accounts[0], 10000);
            await campaign.Contribute(factoryFixture.Accounts[0], new HexBigInteger(20000));
            await campaign.CreateSpendingRequest(factoryFixture.Accounts[0], "Buy batteries", 5000, factoryFixture.Accounts[1]);

            await campaign.ApproveSpendingRequest(factoryFixture.Accounts[0], 0);
            await campaign.ApproveSpendingRequest(factoryFixture.Accounts[0], 0);

            var request = await campaign.GetSpendingRequest(0);
            Assert.Equal(1, request.ApprovalCount);
        }

        [Fact]
        public async Task FinalizeRequest_ApprovedByEnoughPeople_SendMoneyToRecipient()
        {
            var initialBalance = await factoryFixture.Web3.Eth.GetBalance.SendRequestAsync(factoryFixture.Accounts[1]);

            var campaign = await factoryFixture.Factory.CreateCampaign(factoryFixture.Accounts[0], 10000);
            await campaign.Contribute(factoryFixture.Accounts[0], new HexBigInteger(20000));
            await campaign.CreateSpendingRequest(factoryFixture.Accounts[0], "Buy batteries", 5000, factoryFixture.Accounts[1]);

            await campaign.ApproveSpendingRequest(factoryFixture.Accounts[0], 0);
            await campaign.FinalizeSpendingRequest(factoryFixture.Accounts[0], 0);

            var finalBalance = await factoryFixture.Web3.Eth.GetBalance.SendRequestAsync(factoryFixture.Accounts[1]);
            Assert.True(finalBalance.Value - initialBalance.Value > 0);

            var request = await campaign.GetSpendingRequest(0);
            Assert.True(request.Complete);
        }
    }
}
