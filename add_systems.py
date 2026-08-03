import json
import uuid

with open("Assets/scenes/main.scene", "r", encoding="utf-8") as f:
    data = json.load(f)

# Find Systems GameObject
def find_systems(nodes):
    for node in nodes:
        if node.get("Name") == "Systems":
            return node
        if "Children" in node:
            res = find_systems(node["Children"])
            if res: return res
    return None

systems = find_systems(data.get("GameObjects", []))

if systems is not None:
    if "Components" not in systems:
        systems["Components"] = []
    
    def add_comp(type_name):
        systems["Components"].append({
            "__type": type_name,
            "__guid": str(uuid.uuid4()),
            "__enabled": True
        })

    add_comp("UltimoBarrio.WorldTime.WorldClock")
    add_comp("UltimoBarrio.Raids.RaidManager")
    add_comp("UltimoBarrio.Trading.Trader") # EconomyTrader component? Wait, Trader was standard Component

    with open("Assets/scenes/main.scene", "w", encoding="utf-8") as f:
        json.dump(data, f, indent=2)
    print("Added components to Systems.")
else:
    print("Could not find Systems.")
