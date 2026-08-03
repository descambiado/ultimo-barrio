import json
import uuid
import os

path = r'Assets/scenes/test_economy.scene'
with open(path, 'r', encoding='utf-8') as f:
    data = json.load(f)

# Add NetworkHelper
network_helper = {
    "__guid": str(uuid.uuid4()),
    "Flags": 0,
    "Name": "Network",
    "Enabled": True,
    "Components": [
        {
            "__type": "Sandbox.NetworkHelper",
            "__guid": str(uuid.uuid4()),
            "PlayerPrefab": {
                "_type": "gameobject",
                "prefab": "prefabs/player.prefab"
            },
            "SpawnPoints": [
                {
                    "_type": "gameobject",
                    "go": "00000000-0000-0000-0000-000000000000"
                }
            ],
            "StartServer": True
        }
    ]
}

# Add Trader prefab instance
trader_instance = {
    "__guid": str(uuid.uuid4()),
    "Flags": 0,
    "Name": "Trader",
    "Position": "0,50,0",
    "Enabled": True,
    "__prefab": "prefabs/economy/trader.prefab",
    "__prefab_variables": {}
}

if "GameObjects" not in data:
    data["GameObjects"] = []

data["GameObjects"].append(network_helper)
data["GameObjects"].append(trader_instance)

with open(path, 'w', encoding='utf-8') as f:
    json.dump(data, f, indent=2)

print('Updated test_economy.scene')
