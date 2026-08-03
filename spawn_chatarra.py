import json
import uuid

with open("Assets/scenes/main.scene", "r", encoding="utf-8") as f:
    data = json.load(f)

world = None
for node in data.get("GameObjects", []):
    if node.get("Name") == "World":
        world = node
        break

if world:
    chatarra_go = {
        "__guid": str(uuid.uuid4()),
        "Flags": 0,
        "Name": "Chatarra Pickup",
        "Position": "0,50,50",
        "Enabled": True,
        "Components": [
            {
                "__type": "UltimoBarrio.WorldItemPickup",
                "__guid": str(uuid.uuid4()),
                "ItemId": "chatarra",
                "Amount": 1
            },
            {
                "__type": "Sandbox.BoxCollider",
                "__guid": str(uuid.uuid4()),
                "Center": "0,0,0",
                "Extents": "10,10,10",
                "IsTrigger": False,
                "Static": False
            }
        ]
    }
    if "Children" not in world:
        world["Children"] = []
    world["Children"].append(chatarra_go)
    print("Spawned Chatarra pickup in World")

with open("Assets/scenes/main.scene", "w", encoding="utf-8") as f:
    json.dump(data, f, indent=2)
