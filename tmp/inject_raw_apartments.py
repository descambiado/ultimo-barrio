
import json
import uuid

# Load prefab
prefab = json.load(open('Assets/prefabs/world/pf_apartment_unit.prefab', 'r', encoding='utf-8'))
root = prefab['GameObjects'][0]

def clone_node(node):
    new_node = node.copy()
    new_node['__guid'] = str(uuid.uuid4())
    if 'Id' in new_node:
        new_node['Id'] = new_node['__guid']
    
    if 'Components' in new_node:
        new_comps = []
        for c in new_node['Components']:
            nc = c.copy()
            nc['__guid'] = str(uuid.uuid4())
            new_comps.append(nc)
        new_node['Components'] = new_comps
        
    if 'Children' in new_node:
        new_children = []
        for c in new_node['Children']:
            new_children.append(clone_node(c))
        new_node['Children'] = new_children
        
    return new_node

# Clone A01
a01 = clone_node(root)
a01['Name'] = 'apartment-a01'
for c in a01.get('Components', []):
    if c.get('__type') == 'ApartmentComponent':
        c['ApartmentId'] = 'apartment-a01'

# Clone A02
a02 = clone_node(root)
a02['Name'] = 'apartment-a02'
a02['Position'] = '0,0,500'
for c in a02.get('Components', []):
    if c.get('__type') == 'ApartmentComponent':
        c['ApartmentId'] = 'apartment-a02'

# Clean scene
scene_path = 'Assets/scenes/main.scene'
d = json.load(open(scene_path, 'r', encoding='utf-8'))
new_root = []
for go in d['GameObjects']:
    if go.get('__Prefab') == 'prefabs/world/pf_apartment_unit.prefab':
        continue
    if go.get('Name') == 'apartment-a01' or go.get('Name') == 'apartment-a02':
        continue
    new_root.append(go)
d['GameObjects'] = new_root

# Append clones
d['GameObjects'].append(a01)
d['GameObjects'].append(a02)

with open(scene_path, 'w', encoding='utf-8') as f:
    json.dump(d, f, indent=2)

print('Injected raw clones for A01 and A02.')

