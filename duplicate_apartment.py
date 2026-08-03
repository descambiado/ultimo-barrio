import json
import uuid
import copy

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

a01 = None
for child in world.get("Children", []):
    if child.get("Name") == "Prototype Apartment A01":
        a01 = child
        break

if a01:
    a02 = copy.deepcopy(a01)
    
    guid_map = {}
    
    def regen_guids(node):
        old_guid = node.get("__guid")
        new_guid = str(uuid.uuid4())
        guid_map[old_guid] = new_guid
        node["__guid"] = new_guid
        
        for comp in node.get("Components", []):
            comp_old_guid = comp.get("__guid")
            comp_new_guid = str(uuid.uuid4())
            guid_map[comp_old_guid] = comp_new_guid
            comp["__guid"] = comp_new_guid
                
        for child in node.get("Children", []):
            regen_guids(child)

    # First pass: replace all GUIDs and build map
    regen_guids(a02)
    
    # Second pass: fix references in A02 and update A01 just in case
    def fix_refs(node):
        for comp in node.get("Components", []):
            if comp.get("__type") == "UltimoBarrio.Apartments.ApartmentComponent":
                comp["ApartmentId"] = "apartment-a02"
                
                # Check for mapped references
                door = comp.get("DoorReference")
                if door and isinstance(door, dict) and door.get("go") in guid_map:
                    door["go"] = guid_map[door["go"]]
                    
                spawn = comp.get("SpawnReference")
                if spawn and isinstance(spawn, dict) and spawn.get("go") in guid_map:
                    spawn["go"] = guid_map[spawn["go"]]
                    
                stash = comp.get("StashReference")
                if stash and isinstance(stash, dict) and stash.get("go") in guid_map:
                    stash["go"] = guid_map[stash["go"]]
                    
            if comp.get("__type") == "UltimoBarrio.Apartments.ApartmentClaimInteractable":
                comp["ApartmentId"] = "apartment-a02"
                
        for child in node.get("Children", []):
            fix_refs(child)

    fix_refs(a02)
    a02["Name"] = "Prototype Apartment A02"
    a02["Position"] = "0,800,0" # move it aside
    
    world["Children"].append(a02)

    with open("Assets/scenes/main.scene", "w", encoding="utf-8") as f:
        json.dump(data, f, indent=2)
    print("Duplicated A01 to A02 with correct references.")
else:
    print("Could not find A01.")
