using Unity.Entities;
using VaultDebug.Runtime.Logger;

namespace VaultDebug.Runtime.Logger.Entities
{
    /// <summary>
    /// Completes producer jobs and forwards their queued VaultLogger records at the end of simulation.
    /// </summary>
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial struct VaultLoggerEcsDrainSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency.Complete();
            DIBootstrapper.Container.Resolve<ILoggerProvider>().Drain();
        }
    }
}
