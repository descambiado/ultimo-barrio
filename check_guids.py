import json
from collections import defaultdict

with open("Assets/scenes/main.scene", "r", encoding="utf-8") as f:
    data = json.load(f)

guids = defaultdict(list)

def traverse(node, path=""):
    name = node.get("Name", "Unnamed")
    current_path = f"{path}/{name}" if path else name
    
    guid = node.get("__guid")
    if guid:
        guids[guid].append(f"GameObject: {current_path}")
        
    for i, comp in enumerate(node.get("Components", [])):
        comp_guid = comp.get("__guid")
        comp_type = comp.get("__type", "UnknownComponent")
        if comp_guid:
            guids[comp_guid].append(f"Component: {comp_type} on {current_path}")
            
    for child in node.get("Children", []):
        traverse(child, current_path)

for root in data.get("GameObjects", []):
    traverse(root)

duplicates = {k: v for k, v in guids.items() if len(v) > 1}

if duplicates:
    print(f"FOUND {len(duplicates)} DUPLICATE GUIDS:")
    for guid, paths in duplicates.items():
        print(f"GUID {guid} used by:")
        for p in paths:
            print(f"  - {p}")
    exit(1)
else:
    print("No duplicate GUIDs found in main.scene.")
