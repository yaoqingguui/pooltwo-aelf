using System;
using System.Threading.Tasks;
using AElf.Contracts.MultiToken;
using AElf.CSharp.Core;
using AElf.Types;
using Gandalf.Contracts.PoolTwoContract;
using Google.Protobuf.WellKnownTypes;
using Shouldly;
using Xunit;

namespace Gandalf.Contracts.PoolTwo
{
    public partial class PoolTwoContractTests : PoolTwoContractTestBase
    {
        [Fact]
        public async Task Init()
        {
            await Initialize();
        }
        
        [Fact]
        public async Task Deposit_After_Startblock_And_Withdraw_In_Stage_Two()
        {
            var ownerStub = await Initialize();
            var allocPoint = 10;
            await AddPoolFunc(ownerStub,allocPoint, LPTOKEN_01, false);
            await AddPoolFunc(ownerStub, allocPoint, LPTOKEN_01, false);
            var amount = 10000000000;
            var tomPoolStub = GetPoolTwoContractStub(TomPair);
            var tomTokenStub = GetTokenContractStub(TomPair);
            await tomTokenStub.Approve.SendAsync(new ApproveInput
            {
                Amount = amount,
                Spender = DAppContractAddress,
                Symbol = LPTOKEN_01

            });
            var startBlock = (await tomPoolStub.StartBlock.CallAsync(new Empty())).Value;
            var currentBlockHeight = await GetCurrentBlockHeight();
            var skipBlock = startBlock.Add(20).Sub(currentBlockHeight);
            currentBlockHeight = await SkipBlocks(skipBlock);
            await tomPoolStub.Deposit.SendAsync(new DepositInput
            {
                Amount = amount,
                Pid = 1
            });
            var depositBlock = currentBlockHeight.Add(1);
            
            //skip to withdraw blocks
            currentBlockHeight = await GetCurrentBlockHeight();
            skipBlock = startBlock.Add(600).Sub(currentBlockHeight);
            currentBlockHeight = await SkipBlocks(skipBlock);
            var distributeTokenPerBlock = await tomPoolStub.DistributeTokenPerBlock.CallAsync(new Empty());
            var pending = await tomPoolStub.Pending.CallAsync(new PendingInput
            {
                Pid = 1,
                User = Tom
            });
            var stageOneEndBlock = startBlock.Add(HalvingPeriod);
            var pendingStageOneExpect = stageOneEndBlock.Sub(depositBlock)
                .Mul(Convert.ToInt64(distributeTokenPerBlock.Value)).Div(2);
            var pendingStageTwoExpect = currentBlockHeight.Add(1).Sub(stageOneEndBlock).Mul(Convert.ToInt64(distributeTokenPerBlock.Value))
                .Div(2).Div(2);
            var pendingExpect = pendingStageOneExpect.Add(pendingStageTwoExpect);
            pending.ShouldBe(pendingExpect);

            var withdrawSendAsync = await tomPoolStub.Withdraw.SendAsync(new WithdrawInput
            {
                Amount = amount,
                Pid = 1
            });
            var blockNumber = withdrawSendAsync.TransactionResult.BlockNumber;
            var withdrawDistributeTokenStageOneExpect =
                stageOneEndBlock.Sub(depositBlock).Mul(Convert.ToInt64(distributeTokenPerBlock.Value)).Div(2);
            var withdrawDistributeTokenStageTwoExpect =
                blockNumber.Sub(stageOneEndBlock).Mul(Convert.ToInt64(distributeTokenPerBlock.Value)).Div(2).Div(2);
            var withdrawDistributeTokenExpect =
                withdrawDistributeTokenStageOneExpect.Add(withdrawDistributeTokenStageTwoExpect);
            var balance = await tomTokenStub.GetBalance.CallAsync(new GetBalanceInput
            {
                Owner = Tom,
                Symbol = DISTRIBUTETOKEN
            });
            balance.Balance.ShouldBe(withdrawDistributeTokenExpect);
        }
        
        [Fact]
        public async Task Deposit_Before_Start_Block_And_Withdraw_In_Stage_Two()
        {
            var ownerStub = await Initialize();
            var allocPoint = 10;
            await AddPoolFunc(ownerStub,allocPoint, LPTOKEN_01, false);
            await AddPoolFunc(ownerStub, allocPoint, LPTOKEN_01, false);
            var tomPoolStub = GetPoolTwoContractStub(TomPair);
            var tomTokenStub = GetTokenContractStub(TomPair);
            var amount = 10000000000;
            var startBlock = (await ownerStub.StartBlock.CallAsync(new Empty())).Value;
            await tomTokenStub.Approve.SendAsync(new ApproveInput
            {
                Amount = amount,
                Spender = DAppContractAddress,
                Symbol = LPTOKEN_01
            });
           
            await tomPoolStub.Deposit.SendAsync(new DepositInput
            {
                Amount = amount,
                Pid = 1
            });
            //skip to withdraw blocks (600)
            var currentBlockHeight = await GetCurrentBlockHeight();
            var skipBlocks = startBlock.Add(600).Sub(currentBlockHeight);
            currentBlockHeight = await SkipBlocks(skipBlocks);
            var distributeTokenPerBlock = await tomPoolStub.DistributeTokenPerBlock.CallAsync(new Empty());
            var pending = await tomPoolStub.Pending.CallAsync(new PendingInput
            {
                Pid = 1,
                User = Tom
            });
            var stageOneEndBlock = startBlock.Add(HalvingPeriod);
            var pendingStageOneExpect = stageOneEndBlock.Sub(startBlock).Mul(Convert.ToInt64(distributeTokenPerBlock.Value)).Div(2);
            var pendingStageTwoExpect = currentBlockHeight.Add(1).Sub(stageOneEndBlock)
                .Mul(Convert.ToInt64(distributeTokenPerBlock.Value)).Div(2).Div(2);
            var pendingExpect = pendingStageOneExpect.Add(pendingStageTwoExpect);
            pending.ShouldBe(pendingExpect);

            await tomPoolStub.Withdraw.SendAsync(new WithdrawInput
            {
                Amount = amount,
                Pid = 1
            });

            var withdrawDistributeTokenStageOneExpect = stageOneEndBlock.Sub(startBlock)
                .Mul(Convert.ToInt64(distributeTokenPerBlock.Value)).Div(2);

            var withdrawDistributeTokenStageTwoExpect = currentBlockHeight.Add(1).Sub(stageOneEndBlock)
                .Mul(Convert.ToInt64(distributeTokenPerBlock.Value)).Div(2).Div(2);
            
            var withdrawDistributeTokenExpect =
                withdrawDistributeTokenStageOneExpect.Add(withdrawDistributeTokenStageTwoExpect);

            var balance = await tomTokenStub.GetBalance.CallAsync(new GetBalanceInput
            {
                Owner = Tom,
                Symbol = DISTRIBUTETOKEN
            });
            balance.Balance.ShouldBe(withdrawDistributeTokenExpect);
        }
        
        
        
        [Fact]
        public async Task Deposit_After_Startblock_And_Withdraw_In_Stage_One()
        {
            var ownerStub = await Initialize();
            var allocPoint = 10;
            await AddPoolFunc(ownerStub,allocPoint, LPTOKEN_01, false);
            await AddPoolFunc(ownerStub, allocPoint, LPTOKEN_01, false);
            var startBlock = await ownerStub.StartBlock.CallAsync(new Empty());
            var tomPoolStub = GetPoolTwoContractStub(TomPair);
            var tomTokenStub = GetTokenContractStub(TomPair);
            await tomTokenStub.Approve.SendAsync(new ApproveInput
            {
                Amount = 10000000000,
                Spender = DAppContractAddress,
                Symbol = LPTOKEN_01
            });
            var currentBlockHeight = await GetCurrentBlockHeight();
            var skipBlock = (startBlock.Value - currentBlockHeight + 20);
            currentBlockHeight = await SkipBlocks(skipBlock);
            await tomPoolStub.Deposit.SendAsync(new DepositInput
            {
                Amount = 10000000000,
                Pid = 1
            });
            var depositBlock = currentBlockHeight.Add(1);
            // skip to withdraw height.
            currentBlockHeight = await SkipBlocks(50);
            var distributeTokenPerBlock = await tomPoolStub.DistributeTokenPerBlock.CallAsync(new Empty());
            var pending = await tomPoolStub.Pending.CallAsync(new PendingInput
            {
                Pid = 1,
                User = Tom
            });
            var pendingExpect = (currentBlockHeight.Add(1).Sub(depositBlock)).Mul(Convert.ToInt64(distributeTokenPerBlock.Value)).Div(2);
            pending.ShouldBe(pendingExpect);
            await tomPoolStub.Withdraw.SendAsync(new WithdrawInput
            {
                Amount = 10000000000,
                Pid = 1
            });

            var withdrawDistributeTokenExpect = currentBlockHeight.Add(1).Sub(depositBlock)
                .Mul(Convert.ToInt64(distributeTokenPerBlock.Value).Div(2));
            var balance = await tomTokenStub.GetBalance.CallAsync(new GetBalanceInput
            {
                Owner = Tom,
                Symbol = DISTRIBUTETOKEN
            });
            balance.Balance.ShouldBe(withdrawDistributeTokenExpect);
        }
        
        [Fact]
        public async Task Deposit_Before_StartBlock_And_Withdraw_In_Stage_One()
        {   
            var ownerStub = await Initialize();
            var allocPoint = 10;
            await AddPoolFunc(ownerStub,allocPoint, LPTOKEN_01, false);
            await AddPoolFunc(ownerStub, allocPoint, LPTOKEN_01, false);
            var startBlock = await ownerStub.StartBlock.CallAsync(new Empty());
            var tomPoolStub = GetPoolTwoContractStub(TomPair);
            var tomTokenStub = GetTokenContractStub(TomPair);
            await tomTokenStub.Approve.SendAsync(new ApproveInput
            {
                Amount = 10000000000,
                Spender = DAppContractAddress,
                Symbol = LPTOKEN_01
            });
            await tomPoolStub.Deposit.SendAsync(new DepositInput
            {
                Amount = 10000000000,
                Pid = 1
            });
            var currentBlock = await SkipBlocks(50);
            var distributeTokenPerBlock = await tomPoolStub.DistributeTokenPerBlock.CallAsync(new Empty());
            var pending = await tomPoolStub.Pending.CallAsync(new PendingInput
            {
                Pid = 1,
                User = Tom
            });

            var pendingExpect = distributeTokenPerBlock.Div(2).Mul(currentBlock.Sub(startBlock.Value).Add(1));
            pendingExpect.ShouldBe(pending);

            await tomPoolStub.Withdraw.SendAsync(new WithdrawInput
            {
                Pid = 1,
                Amount = 10000000000
            });
            var currentBlockHeight = await GetCurrentBlockHeight();
            var withdrawDistributeTokenExpect =
                distributeTokenPerBlock.Div(2).Mul(currentBlockHeight.Sub(startBlock.Value));
            var balance = await tomTokenStub.GetBalance.CallAsync(new GetBalanceInput
            {
                Symbol = DISTRIBUTETOKEN,
                Owner = Tom
            });
            withdrawDistributeTokenExpect.ShouldBe(balance.Balance);
        }
        
        [Fact]
        public async Task SetDistributeTokenPerBlock_Should_Work()
        {
            var ownerStub = await Initialize();
            var allocPoint = 10;
            await AddPoolFunc(ownerStub,allocPoint, LPTOKEN_01, false);
            await AddPoolFunc(ownerStub, allocPoint, LPTOKEN_01, false);
            await ownerStub.SetDistributeTokenPerBlock.SendAsync(new Int64Value
            {
                Value = 500
            });
            var value = await ownerStub.DistributeTokenPerBlock.CallAsync(new Empty());
            value.Value.ShouldBe("500");
        }
        
        
        [Fact]
        public async Task Set_Should_Work()
        {
            var ownerStub = await Initialize();
            var allocPoint = 10;
            await AddPoolFunc(ownerStub,allocPoint, LPTOKEN_01, false);
            await AddPoolFunc(ownerStub, allocPoint, LPTOKEN_01, false);
            await ownerStub.Set.SendAsync(new SetInput
            {
                Pid = 1,
                AllocPoint = 20,
                WithUpdate = true,
                NewPerBlock = 500
            });
            var pool = await ownerStub.PoolInfo.CallAsync(new Int32Value
            {
                Value = 1
            });

            pool.AllocPoint.ShouldBe(20);
            var int64Value = await ownerStub.TotalAllocPoint.CallAsync(new Empty());
            int64Value.Value.ShouldBe(20 - 10 + 20);
            var value = await ownerStub.DistributeTokenPerBlock.CallAsync(new Empty());
            value.Value.ShouldBe("500");
        }
        
        [Fact]
        public async Task Add_Pool_Should_Works()
        {
            var ownerStub = await Initialize();
            var allocPoint = 10;
            await AddPoolFunc(ownerStub,allocPoint, LPTOKEN_01, false);
            await AddPoolFunc(ownerStub, allocPoint, LPTOKEN_01, false);
            var pool = await ownerStub.PoolInfo.CallAsync(new Int32Value
            {
                Value = 1
            });
            pool.LpToken.ShouldBe(LPTOKEN_01);
            pool.AllocPoint.ShouldBe(allocPoint);
            var length = await ownerStub.PoolLength.CallAsync(new Empty());
            length.Value.ShouldBe(2);
            var currentBlockHeight = await GetCurrentBlockHeight();

            var reward = await ownerStub.Reward.CallAsync(new Int64Value
            {
                Value = currentBlockHeight
            });
           reward.Value.ShouldBe("10000");
        }
        
        [Fact]
        public async Task Deposit_And_Withdraw_Should_Works()
        {
            var ownerStub = await Initialize();
            var allocpoint = 10;
            await AddPoolFunc(ownerStub, allocpoint, LPTOKEN_01, false);
            await AddPoolFunc(ownerStub, allocpoint, LPTOKEN_01, false);
            var tomTokenStub = GetTokenContractStub(TomPair);
            await tomTokenStub.Approve.SendAsync(new ApproveInput
            {
                Amount = 10000000,
                Spender = DAppContractAddress,
                Symbol = LPTOKEN_01
            });

            var tomPoolStub = GetPoolTwoContractStub(TomPair);
            await tomPoolStub.Deposit.SendAsync(new DepositInput
            {
                Amount = 10000000,
                Pid = 1
            });
            var user = await tomPoolStub.UserInfo.CallAsync(new UserInfoInput
            {
                Pid = 1,
                User = Tom
            });
            user.Amount.Value.ShouldBe("10000000");

            await tomPoolStub.Withdraw.SendAsync(new WithdrawInput
            {
                Amount = 500000,
                Pid = 1
            });

            user = await tomPoolStub.UserInfo.CallAsync(new UserInfoInput
            {
                Pid = 1,
                User = Tom
            });
            user.Amount.Value.ShouldBe("9500000");
        }
        
        private async Task AddPoolFunc(PoolTwoContractContainer.PoolTwoContractStub owenrStub,
            int allocPoint,
            string token,
            bool b)
        {
            await owenrStub.Add.SendAsync(new AddInput
            {
                AllocPoint = allocPoint,
                LpToken = token,
                WithUpdate = b
            });
            
        }
    }
}