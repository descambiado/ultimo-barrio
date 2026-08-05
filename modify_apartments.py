# coding=utf-8
import json
import uuid

scene_file = 'Assets/scenes/ultimo_barrio_alpha.scene'
with open(scene_file, 'r', encoding='utf-8') as f:
    data = json.load(f)

def create_anchor(name):
    return {
        "__guid": str(uuid.uuid4()),
        "Flags": 0,
        "Name": name,
        "Position": "0,0,0",
        "Enabled": True,
        "Components": [],
        "Children": []
    }

def find_stash_for_apt(apt_id, node):
    if 'Components' in node:
        for c in node['Components']:
            if c.get('__type') == 'UltimoBarrio.Inventory.StashComponent' or c.get('__type') == 'UltimoBarrio.StashComponent':
                if c.get('ApartmentId') == apt_id:
                    return node
    for child in node.get('Children', []):
        res = find_stash_for_apt(apt_id, child)
        if res: return res
    return None

def remove_node(guid_to_remove, parent_list):
    for i, n in enumerate(parent_list):
        if n.get('__guid') == guid_to_remove:
            return parent_list.pop(i)
        if 'Children' in n:
            res = remove_node(guid_to_remove, n['Children'])
            if res: return res
    return None

def process_apt(apt_node):
    apt_id = None
    for c in apt_node.get('Components', []):
        if c.get('__type') == 'UltimoBarrio.Apartments.ApartmentComponent':
            apt_id = c.get('ApartmentId')
    if not apt_id: return

    anchors = [
        "ApartmentEntranceAnchor",
        "ApartmentDoorAnchor",
        "ApartmentInteriorAnchor",
        "ApartmentStashAnchor",
        "ApartmentSpawnAnchor",
        "ApartmentWindowAnchor"
    ]
    
    stash_anchor = create_anchor("ApartmentStashAnchor")
    created = [create_anchor(a) for a in anchors if a != "ApartmentStashAnchor"]
    created.append(stash_anchor)
    
    if 'Children' not in apt_node:
        apt_node['Children'] = []
    
    existing_names = [c.get('Name') for c in apt_node['Children']]
    if "ApartmentEntranceAnchor" not in existing_names:
        apt_node['Children'].extend(created)
        
    stash_node = find_stash_for_apt(apt_id, {"Children": data.get('GameObjects', [])})
    if stash_node:
        remove_node(stash_node['__guid'], data.get('GameObjects', []))
        
        stash_node['Position'] = "0,0,0"
        stash_node['Rotation'] = "0,0,0,1"
        
        for c in apt_node['Children']:
            if c.get('Name') == 'ApartmentStashAnchor':
                if 'Children' not in c: c['Children'] = []
                c['Children'].append(stash_node)
                print(f"Moved stash to {apt_id} stash anchor")
                break

def traverse(node):
    for c in node.get('Components', []):
        if c.get('__type') == 'UltimoBarrio.Apartments.ApartmentComponent':
            apt_id = c.get('ApartmentId')
            if apt_id in ['apartment-a01', 'apartment-a02']:
                process_apt(node)
    for child in node.get('Children', []):
        traverse(child)

for go in data.get('GameObjects', []):
    traverse(go)

with open(scene_file, 'w', encoding='utf-8') as f:
    json.dump(data, f, indent=2)

print("A01 and A02 anchors created and stashes reparented.")
