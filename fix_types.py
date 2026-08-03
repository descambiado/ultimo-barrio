import json

def fix_file(path):
    with open(path, "r", encoding="utf-8") as f:
        data = json.load(f)
    
    def walk(n):
        for c in n.get("Components", []):
            if c.get("__type") == "UltimoBarrio.Inventory.InventoryComponent":
                c["__type"] = "UltimoBarrio.InventoryComponent"
        for child in n.get("Children", []):
            walk(child)
            
    if "RootObject" in data:
        walk(data["RootObject"])
    if "GameObjects" in data:
        for go in data["GameObjects"]:
            walk(go)
            
    with open(path, "w", encoding="utf-8") as f:
        json.dump(data, f, indent=2)
        
fix_file("Assets/scenes/main.scene")
fix_file("Assets/prefabs/player.prefab")
print("Fixed component types")
