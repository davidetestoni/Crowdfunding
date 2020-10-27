# Crowdfunding
A decentralized crowdfunding platform built using smart contracts on the ethereum blockchain.

## Testing
1. Start your local test chain on the default port (like geth or ganache). Make sure it has at least 2 premade accounts.
2. Use `solc` to compile `Campaign.sol` and output `bin/Campaign.abi`, `bin/Campaign.bin`, `bin/CampaignFactory.abi` and `bin/CampaignFactory.bin`
3. Install .NET 5
4. Run `dotnet test`