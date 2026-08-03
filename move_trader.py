import json
import uuid

with open("Assets/scenes/main.scene", "r", encoding="utf-8") as f:
    data = json.load(f)

world = None
systems = None
for node in data.get("GameObjects", []):
    if node.get("Name") == "World":
        world = node
    if node.get("Name") == "Systems":
        systems = node

trader_comp = None
if systems:
    new_comps = []
    for c in systems.get("Components", []):
        if c.get("__type") == "UltimoBarrio.Trading.Trader":
            trader_comp = c
        else:
            new_comps.append(c)
    systems["Components"] = new_comps

if trader_comp and world:
    trader_go = {
        "__guid": str(uuid.uuid4()),
        "Flags": 0,
        "Name": "Neighborhood Trader",
        "Position": "300,300,0",
        "Enabled": True,
        "Components": [
            trader_comp,
            {
                "__type": "Sandbox.BoxCollider",
                "__guid": str(uuid.uuid4()),
                "Center": "0,0,32",
                "Extents": "32,32,32",
                "IsTrigger": False,
                "Static": False
            }
        ]
    }
    if "Children" not in world:
        world["Children"] = []
    world["Children"].append(trader_go)
    print("Moved Trader from Systems to a new GameObject in World with a BoxCollider.")

with open("Assets/scenes/main.scene", "w", encoding="utf-8") as f:
    json.dump(data, f, indent=2)

