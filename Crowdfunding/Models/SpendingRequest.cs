using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Numerics;

namespace Crowdfunding.Models
{
    [FunctionOutput]
    public class SpendingRequest : IFunctionOutputDTO
    {
        public int Index { get; set; } = 0;

        [Parameter("string", 1)]
        public string Description { get; set; }

        [Parameter("uint256", 2)]
        public BigInteger Value { get; set; }

        [Parameter("address", 3)]
        public string Recipient { get; set; }

        [Parameter("bool", 4)]
        public bool Complete { get; set; }

        [Parameter("uint256", 5)]
        public BigInteger ApprovalCount { get; set; }
    }
}
