using System;
using System.Numerics;
using System.Text.RegularExpressions;
using AElf.CSharp.Core;
using AElf.Types;
using Google.Protobuf.WellKnownTypes;

namespace Gandalf.Contracts.PoolTwoContract
{
    public partial class PoolTwoContract
    {
        /**
         * 
         */
        public override BigIntValue Pending(PendingInput input)
        {
            var pool = State.PoolInfo.Value.PoolList[input.Pid];
            var user = State.UserInfo[input.Pid][input.User];
            var accDistributeTokenPerShare = pool.AccDistributeTokenPerShare;
            var lpSupply = pool.TotalAmount;
            if (user.Amount > 0)
            {
                if (Context.CurrentHeight > pool.LastRewardBlock)
                {
                    var blockReward = GetDistributeTokenBlockReward(pool.LastRewardBlock);
                    var distributeTokenReward = blockReward.Mul(pool.AllocPoint).Div(State.totalAllocPoint.Value);
                    accDistributeTokenPerShare = accDistributeTokenPerShare.Add(
                        distributeTokenReward.Mul(new BigIntValue
                        {
                            Value = Extension
                        }).Div(lpSupply)
                    );
                    return accDistributeTokenPerShare.Mul(user.Amount).Div(new BigIntValue
                    {
                        Value = Extension
                    }).Sub(user.RewardDebt);
                }

                return accDistributeTokenPerShare.Mul(user.Amount).Div(new BigIntValue
                {
                    Value = Extension
                }).Sub(user.RewardDebt);
            }
            return new BigIntValue(0);
        }

        /**
         * 
         */
        public override BigIntValue GetDistributeTokenBlockReward(Int64Value input)
        {
            return GetDistributeTokenBlockReward(input.Value);
        }


        private BigIntValue GetDistributeTokenBlockReward(long lastRewardBlock)
        {
            var blockReward = new BigIntValue(0);
            var rewardBlock = Context.CurrentHeight > State.EndBlock.Value
                ? State.EndBlock.Value
                : Context.CurrentHeight;
            if (rewardBlock <= lastRewardBlock)
            {
                return new BigIntValue(0);
            }

            var n = Phase(lastRewardBlock).Value;
            var m = Phase(rewardBlock).Value;
            while (n < m)
            {
                n++;
                var r = n.Mul(State.HalvingPeriod.Value).Add(State.StartBlock.Value);
                blockReward = blockReward.Add(
                    Reward(new Int64Value
                    {
                        Value = r
                    }).Mul((r.Sub(lastRewardBlock).ToString()))
                );
                lastRewardBlock = r;
            }

            blockReward = blockReward.Add(
                Reward(new Int64Value
                {
                    Value = rewardBlock
                }).Mul(rewardBlock.Sub(lastRewardBlock))
            );
            return blockReward;
        }

        /**
         * 
         */
        public override BigIntValue Reward(Int64Value input)
        {
            var phase = Phase(new Int64Value
            {
                Value = input.Value
            });
            return new BigIntValue
            {
                Value = State.DistributeTokenPerBlock.Value.Div(new BigIntValue
                {
                    Value = (2 ^ phase.Value).ToString()
                }).ToString()
            };
        }

        /**
         * 
         */
        public override Int64Value Phase(Int64Value input)
        {
            return Phase(input.Value);
        }

        private Int64Value Phase(long blockNumber)
        {
            if (State.HalvingPeriod.Value == 0)
            {
                return new Int64Value
                {
                    Value = 0
                };
            }

            if (blockNumber > State.StartBlock.Value)
            {
                return new Int64Value
                {
                    Value = (Context.CurrentHeight - State.StartBlock.Value - 1) / State.HalvingPeriod.Value
                };
            }

            return new Int64Value();
        }

        /**
         * 
         */
        public override Int64Value PoolLength(Empty input)
        {
            return new Int64Value
            {
                Value = State.PoolInfo.Value.PoolList.Count
            };
        }
    }
}