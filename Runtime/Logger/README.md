# VaultLogger

`VaultLogger` sends structured logs to Unity Console by default. Each message starts with its logger context:

```text
[Gameplay] Player spawned
```

When logger has color, only `[Gameplay]` tag is colored. Message remains uncolored.

## Quick start

```csharp
using System.Collections.Generic;
using UnityEngine;
using VaultDebug.Runtime.Logger;

public class PlayerSpawner : MonoBehaviour
{
    void Start()
    {
        var loggerProvider = DIBootstrapper.Container.Resolve<ILoggerProvider>();
        var logger = loggerProvider.GetLogger("Gameplay", Color.cyan);

        logger.Info("Player spawned.");
        logger.Debug("Spawn point selected.");
        logger.Warn("Spawn point is occupied.");
        logger.Error(
            "Player spawn failed.",
            new Dictionary<string, object>
            {
                { "playerId", 42 },
                { "spawnPoint", "NorthGate" }
            });
    }
}
```

`DIBootstrapper` initializes automatically in normal Unity usage. `ILoggerProvider` caches one logger per context; pass color when first requesting context.

## Log methods

| Method | Unity Console output |
| --- | --- |
| `Info(message, properties?)` | Log |
| `Debug(message, properties?)` | Log |
| `Warn(message, properties?)` | Warning |
| `Error(message, properties?)` | Error |

`properties` and timestamps stay on `IVaultLog` for other handlers, but Unity Console output currently ignores them.

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
