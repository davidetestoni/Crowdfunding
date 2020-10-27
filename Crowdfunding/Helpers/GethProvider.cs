using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.IO;
using System.Linq;

namespace Crowdfunding.Helpers
{
    public static class Geth
    {
        public static Account[] GetAccounts(string keystoreDirectory, string password)
        {
            // Spawn an instance of web3 and decrypt all test accounts from the geth local chain
            var web3 = new Web3();
            var addresses = web3.Eth.Accounts.SendRequestAsync().Result;
            return addresses.Select(a => DecryptAccount(a, keystoreDirectory, password)).ToArray();
        }

        private static Account DecryptAccount(string address, string keystoreDirectory, string password)
        {
            if (address.StartsWith("0x"))
                address = address[2..];

            var file = Directory.EnumerateFiles(keystoreDirectory);
            var json = File.ReadAllText(file.First(f => f.Contains(address)));
            return Account.LoadFromKeyStore(json, password);
        }
    }
}
