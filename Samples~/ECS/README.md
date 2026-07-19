# VaultLogger ECS integration

Import this sample only in projects that have `com.unity.entities` installed. It adds an end-of-simulation drain system and an `ISystem.OnCreate` helper.

```csharp
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using VaultDebug.Runtime.Logger;
using VaultDebug.Runtime.Logger.Entities;

public partial struct MovementSystem : ISystem
{
    VaultLogger _logger;

    public void OnCreate(ref SystemState state)
    {
        _logger = VaultLoggerEcs.CreateLogger("Movement", maxProperties: 3);
    }

    public void OnUpdate(ref SystemState state)
    {
        state.Dependency = new MovementJob
        {
            Logger = _logger,
            Message = new FixedString512Bytes("Entity moved"),
            MovedKey = new FixedString64Bytes("moved")
        }.Schedule(state.Dependency);
    }

    [BurstCompile]
    partial struct MovementJob : IJobEntity
    {
        public VaultLogger Logger;
        public FixedString512Bytes Message;
        public FixedString64Bytes MovedKey;

        void Execute()
        {
            VaultLogProperties properties = default;
            properties.TryAdd(MovedKey, true);
            Logger.Info(Message, properties);
        }
    }
}
```

`VaultLoggerEcsDrainSystem` completes producer jobs and drains the shared logger queue at the end of the simulation update. The core package has no `com.unity.entities` dependency; this sample remains uncompiled until explicitly imported.
