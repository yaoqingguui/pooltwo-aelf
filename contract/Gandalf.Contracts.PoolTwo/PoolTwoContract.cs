using AElf;
using AElf.Sdk.CSharp;
using AElf.Types;
using Google.Protobuf.WellKnownTypes;

namespace Gandalf.Contracts.PoolTwoContract
{
    /// <summary>
    /// The C# implementation of the contract defined in pool_two_contract.proto that is located in the "protobuf"
    /// folder.
    /// Notice that it inherits from the protobuf generated code. 
    /// </summary>
    public partial class PoolTwoContract : PoolTwoContractContainer.PoolTwoContractBase
    {

        public const string Extension = "1000000000000";
        
        public override Empty Initialize(InitializeInput input)
        {

            Assert(State.Owner.Value == null, "Already initialized.");
            State.Owner.Value = input.Owner == null || input.Owner.Value.IsNullOrEmpty() ? Context.Sender : input.Owner;
            Assert(input.StartBlock >= Context.CurrentHeight,"Invalid StartBlock");
            State.DistributeToken.Value = input.DistributeToken;
            State.DistributeTokenPerBlock.Value = input.DistributeTokenPerBlock;
            State.HalvingPeriod.Value = input.HalvingPeriod;
            State.StartBlock.Value = input.StartBlock;
            State.TotalReward.Value = input.TotalReward;
            State.IssuedReward.Value = new BigIntValue(0);
            State.TokenContract.Value =
                Context.GetContractAddressByName(SmartContractConstants.TokenContractSystemName);
            State.PoolInfo.Value = new PoolInfo();
            
            FixEndBlock(new BoolValue
            {
                Value = false
            });
            return new Empty();
        }
        
        
        private void AssertSenderIsOwner()
        {
            Assert(State.Owner.Value != null, "Contract not initialized.");
            Assert(Context.Sender == State.Owner.Value, "Not Owner.");
        }
    }
}