using AElf.Boilerplate.TestBase;
using AElf.Cryptography.ECDSA;
using Gandalf.Contracts.PoolTwoContract;

namespace Gandalf.Contracts.PoolTwo
{
    public class PoolTwoContractTestBase : DAppContractTestBase<PoolTwoContractTestModule>
    {
        // You can get address of any contract via GetAddress method, for example:
        // internal Address DAppContractAddress => GetAddress(DAppSmartContractAddressNameProvider.StringName);

        internal PoolTwoContractContainer.PoolTwoContractStub GetPoolTwoContractStub(ECKeyPair senderKeyPair)
        {
            return GetTester<PoolTwoContractContainer.PoolTwoContractStub>(DAppContractAddress, senderKeyPair);
        }
    }
}