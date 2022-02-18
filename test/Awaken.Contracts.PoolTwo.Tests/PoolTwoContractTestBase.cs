using AElf.Boilerplate.TestBase;
using AElf.Contracts.MultiToken;
using AElf.Cryptography.ECDSA;
using Awaken.Contracts.PoolTwoContract;

namespace Awaken.Contracts.PoolTwo
{
    public class PoolTwoContractTestBase : DAppContractTestBase<PoolTwoContractTestModule>
    {
        // You can get address of any contract via GetAddress method, for example:
        // internal Address DAppContractAddress => GetAddress(DAppSmartContractAddressNameProvider.StringName);

        internal PoolTwoContractContainer.PoolTwoContractStub GetPoolTwoContractStub(ECKeyPair senderKeyPair)
        {
            return GetTester<PoolTwoContractContainer.PoolTwoContractStub>(DAppContractAddress, senderKeyPair);
        }

        internal TokenContractContainer.TokenContractStub GetTokenContractStub(ECKeyPair pair)
        {
            return GetTester<TokenContractContainer.TokenContractStub>(TokenContractAddress, pair);
        }
    }
}