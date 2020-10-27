# Crowdfunding
A decentralized crowdfunding platform built using smart contracts on the ethereum blockchain.

## Testing
1. Start your local test chain on the default port (like geth or ganache). Make sure it has at least 2 premade unlockable accounts
2. Use `solc` to compile `Campaign.sol` and output `bin/Campaign.abi`, `bin/Campaign.bin`, `bin/CampaignFactory.abi` and `bin/CampaignFactory.bin`
3. Install the .NET 5 sdk from Microsoft's official repository
4. Run `dotnet test`