import json

with open("Assets/scenes/main.scene", "r", encoding="utf-8") as f:
    data = json.load(f)

def find_world(nodes):
    for node in nodes:
        if node.get("Name") == "World":
            return node
        if "Children" in node:
            res = find_world(node["Children"])
            if res: return res
    return None

world = find_world(data.get("GameObjects", []))

def get_child_by_name(parent, name):
    for c in parent.get("Children", []):
        if c.get("Name") == name:
            return c
    return None

a01 = get_child_by_name(world, "Prototype Apartment A01")
a02 = get_child_by_name(world, "Prototype Apartment A02")

def fix_apartment(apt_node):
    if not apt_node: return
    # Find Claim Portal, Owner Spawn Anchor, Stash Anchor
    portal = get_child_by_name(apt_node, "Claim Portal")
    anchors = get_child_by_name(apt_node, "Anchors")
    
    spawn = None
    stash = None
    if anchors:
        spawn = get_child_by_name(anchors, "Owner Spawn Anchor")
        stash = get_child_by_name(anchors, "Stash Anchor")
        
    if not portal or not spawn or not stash:
        print(f"Missing children in {apt_node.get('Name')}")
        return
        
    for comp in apt_node.get("Components", []):
        if comp.get("__type") == "UltimoBarrio.Apartments.ApartmentComponent":
            comp["DoorReference"] = { "_type": "gameobject", "go": portal["__guid"] }
            comp["SpawnReference"] = { "_type": "gameobject", "go": spawn["__guid"] }
            comp["StashReference"] = { "_type": "gameobject", "go": stash["__guid"] }
            print(f"Fixed references for {apt_node.get('Name')}")

fix_apartment(a01)
fix_apartment(a02)

with open("Assets/scenes/main.scene", "w", encoding="utf-8") as f:
    json.dump(data, f, indent=2)

print("Fixed all apartment references.")
