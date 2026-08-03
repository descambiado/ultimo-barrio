
import json
import uuid

# Load prefab
prefab = json.load(open('Assets/prefabs/world/pf_apartment_unit.prefab', 'r', encoding='utf-8'))
root = prefab['GameObjects'][0]

def deep_clone(node, guid_map):
    new_node = node.copy()
    
    # Generate new GUID
    old_guid = new_node.get('__guid')
    if old_guid:
        new_guid = str(uuid.uuid4())
        guid_map[old_guid] = new_guid
        new_node['__guid'] = new_guid
        if 'Id' in new_node:
            new_node['Id'] = new_guid
    
    if 'Components' in new_node:
        new_comps = []
        for c in new_node['Components']:
            nc = c.copy()
            old_c_guid = nc.get('__guid')
            if old_c_guid:
                new_c_guid = str(uuid.uuid4())
                guid_map[old_c_guid] = new_c_guid
                nc['__guid'] = new_c_guid
            new_comps.append(nc)
        new_node['Components'] = new_comps
        
    if 'Children' in new_node:
        new_children = []
        for c in new_node['Children']:
            new_children.append(deep_clone(c, guid_map))
        new_node['Children'] = new_children
        
    return new_node

def remap_refs(node, guid_map):
    if 'Components' in node:
        for c in node['Components']:
            for k, v in c.items():
                if isinstance(v, dict) and v.get('_type') == 'gameobject' and 'go' in v:
                    if v['go'] in guid_map:
                        v['go'] = guid_map[v['go']]
    if 'Children' in node:
        for c in node['Children']:
            remap_refs(c, guid_map)

# Clone A01
map_a01 = {}
a01 = deep_clone(root, map_a01)
remap_refs(a01, map_a01)
a01['Name'] = 'apartment-a01'
for c in a01.get('Components', []):
    if 'ApartmentComponent' in c.get('__type', ''):
        c['ApartmentId'] = 'apartment-a01'
        c['SaveVersion'] = 1

# Clone A02
map_a02 = {}
a02 = deep_clone(root, map_a02)
remap_refs(a02, map_a02)
a02['Name'] = 'apartment-a02'
a02['Position'] = '0,0,500'
for c in a02.get('Components', []):
    if 'ApartmentComponent' in c.get('__type', ''):
        c['ApartmentId'] = 'apartment-a02'
        c['SaveVersion'] = 1

# Clean scene of ANY previous prefabs or a01/a02
scene_path = 'Assets/scenes/main.scene'
d = json.load(open(scene_path, 'r', encoding='utf-8'))
new_root = []
for go in d['GameObjects']:
    if go.get('__Prefab') == 'prefabs/world/pf_apartment_unit.prefab':
        continue
    if go.get('Name') in ['apartment-a01', 'apartment-a02', 'Prototype Apartment A01']:
        continue
    if go.get('Name') == 'World':
        if 'Children' in go:
            go['Children'] = [c for c in go['Children'] if c.get('Name') not in ['apartment-a01', 'apartment-a02', 'Prototype Apartment A01']]
    new_root.append(go)
d['GameObjects'] = new_root

# Append clones to root
d['GameObjects'].append(a01)
d['GameObjects'].append(a02)

with open(scene_path, 'w', encoding='utf-8') as f:
    json.dump(d, f, indent=2)

print('Injected PERFECT raw clones for A01 and A02.')

