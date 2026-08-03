
import json
import uuid

# Load prefab
prefab = json.load(open('Assets/prefabs/world/pf_apartment_unit.prefab', 'r', encoding='utf-8'))
root = prefab['GameObjects'][0]

def build_mapping(node, mapping):
    mapping[node['__guid']] = str(uuid.uuid4())
    for comp in node.get('Components', []):
        mapping[comp['__guid']] = str(uuid.uuid4())
    for child in node.get('Children', []):
        build_mapping(child, mapping)

# Map A01
map_a01 = {}
build_mapping(root, map_a01)
inst_a01 = {
    '__guid': map_a01[root['__guid']],
    '__version': 2,
    '__Prefab': 'prefabs/world/pf_apartment_unit.prefab',
    '__PrefabInstancePatch': {
        'PropertyOverrides': [
            { 'Target': { 'Type': 'GameObject', 'IdValue': root['__guid'] }, 'Property': 'Name', 'Value': 'apartment-a01' },
            { 'Target': { 'Type': 'Component', 'IdValue': '0f58841d-b584-4da9-a783-a5e34917e36c' }, 'Property': 'ApartmentId', 'Value': 'apartment-a01' }
        ]
    },
    '__PrefabIdToInstanceId': map_a01
}

# Map A02
map_a02 = {}
build_mapping(root, map_a02)
inst_a02 = {
    '__guid': map_a02[root['__guid']],
    '__version': 2,
    '__Prefab': 'prefabs/world/pf_apartment_unit.prefab',
    '__PrefabInstancePatch': {
        'PropertyOverrides': [
            { 'Target': { 'Type': 'GameObject', 'IdValue': root['__guid'] }, 'Property': 'Name', 'Value': 'apartment-a02' },
            { 'Target': { 'Type': 'GameObject', 'IdValue': root['__guid'] }, 'Property': 'Position', 'Value': '0,0,500' },
            { 'Target': { 'Type': 'Component', 'IdValue': '0f58841d-b584-4da9-a783-a5e34917e36c' }, 'Property': 'ApartmentId', 'Value': 'apartment-a02' }
        ]
    },
    '__PrefabIdToInstanceId': map_a02
}

# Load scene
scene = json.load(open('Assets/scenes/main.scene', 'r', encoding='utf-8'))
scene['GameObjects'].append(inst_a01)
scene['GameObjects'].append(inst_a02)

with open('Assets/scenes/main.scene', 'w', encoding='utf-8') as f:
    json.dump(scene, f, indent=2)

print('Injected full prefab instances with mappings for A01 and A02.')

