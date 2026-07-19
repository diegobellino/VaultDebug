# VaultLogger

`VaultLogger` sends structured logs to Unity Console by default. Each message starts with its logger context:

```text
[Gameplay] Player spawned
```

When logger has color, only `[Gameplay]` tag is colored. Message remains uncolored.

## Quick start

```csharp
using Unity.Collections;
using UnityEngine;
using VaultDebug.Runtime.Logger;

public class PlayerSpawner : MonoBehaviour
{
    void Start()
    {
        var loggerProvider = DIBootstrapper.Container.Resolve<ILoggerProvider>();
        var logger = loggerProvider.GetLogger("Gameplay", Color.cyan, maxProperties: 6);

        logger.Info("Player spawned.");
        logger.Debug("Spawn point selected.");
        logger.Warn("Spawn point is occupied.");
        VaultLogProperties properties = default;
        properties.TryAdd("playerId", 42);
        properties.TryAdd("spawnPoint", new FixedString128Bytes("NorthGate"));
        logger.Error(new FixedString512Bytes("Player spawn failed."), properties);
    }
}
```

`DIBootstrapper` initializes automatically in normal Unity usage. `ILoggerProvider` creates value-type loggers that can be copied into jobs; pass the color when creating a logger.

## Burst and structured log methods

| Method | Unity Console output |
| --- | --- |
| `Info(FixedString512Bytes, VaultLogProperties)` | Log |
| `Debug(FixedString512Bytes, VaultLogProperties)` | Log |
| `Warn(FixedString512Bytes, VaultLogProperties)` | Warning |
| `Error(FixedString512Bytes, VaultLogProperties)` | Error |

`VaultLogger` is Burst-safe when called with `FixedString` values. It queues records in native memory and forwards them to handlers on the Unity main thread. `VaultLogProperties` can hold up to eight typed properties (`string`, `long`, `double`, or `bool`); use `TryAdd` and handle `false` when the fixed payload is full. Configure each logger's retained property count with `maxProperties` (zero through eight). The record always reserves storage for eight fields so it remains an unmanaged, fixed-size Burst value. The main-thread adapter exposes retained fields as `IVaultLog.Properties`; Unity Console output ignores them. Caller stack traces are not captured.

## Vault Console

Vault Console stays closed by default. Open it from `Vault Debug > Console > (Testing) Open Window`, or enable automatic opening in `Vault Debug > Settings`.

Vault Console can display stored properties, filter by context with `@context:"Gameplay"`, and export captured logs.

## Custom handlers

Register handlers through dispatcher:

```csharp
var dispatcher = DIBootstrapper.Container.Resolve<IVaultLogDispatcher>();
dispatcher.RegisterHandler(myHandler);
```

Vault logs are pooled. A handler retaining a log must call `log.Clone()` before storing it.
