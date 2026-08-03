import json
import uuid

with open("Assets/scenes/main.scene", "r", encoding="utf-8") as f:
    data = json.load(f)

def add_stash_inventory(node):
    if node.get("Name") == "Stash Anchor":
        comps = node.get("Components", [])
        has_inv = any(c.get("__type") == "UltimoBarrio.Inventory.InventoryComponent" for c in comps)
        if not has_inv:
            comps.append({
                "__type": "UltimoBarrio.Inventory.InventoryComponent",
                "__guid": str(uuid.uuid4()),
                "__enabled": True
            })
            node["Components"] = comps
            
    for child in node.get("Children", []):
        add_stash_inventory(child)

for root in data.get("GameObjects", []):
    add_stash_inventory(root)

with open("Assets/scenes/main.scene", "w", encoding="utf-8") as f:
    json.dump(data, f, indent=2)

print("Added InventoryComponent to all Stash Anchors.")
