import json
import os

prefab = 'Assets/prefabs/player.prefab'
with open(prefab, 'r', encoding='utf-8') as f:
    d = json.load(f)

d['RootObject']['Components'].extend([
    {
        "__type": "UltimoBarrio.Combat.HeldItemController",
        "__enabled": True,
        "MeleePrefab": {
            "_type": "gameobject",
            "prefab": "prefabs/weapons/ub_melee.prefab"
        },
        "PistolPrefab": {
            "_type": "gameobject",
            "prefab": "prefabs/weapons/ub_usp.prefab"
        },
        "HandBone": {
            "_type": "gameobject",
            "go": "6b5cbc3e-e37c-4eb9-afa6-577469114047"
        }
    },
    {
        "__type": "UltimoBarrio.Players.PlayerMovementModifier",
        "__enabled": True
    }
])

with open(prefab, 'w', encoding='utf-8') as f:
    json.dump(d, f, indent=2)

print("Injected into player.prefab")
