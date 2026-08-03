
import json
import uuid

# 1. Load pf_apartment_unit.prefab to extract GUIDs
with open('Assets/prefabs/world/pf_apartment_unit.prefab', 'r', encoding='utf-8') as f:
    prefab = json.load(f)

# Find root guid
root_guid = prefab.get('RootObject')
if not root_guid and 'GameObjects' in prefab:
    root_guid = prefab['GameObjects'][0]['__guid']

# Find components guids
apt_comp_guid = None
claim_comp_guid = None

def find_comps(node):
    global apt_comp_guid, claim_comp_guid
    for c in node.get('Components', []):
        if c.get('__type') == 'UltimoBarrio.Apartments.ApartmentComponent':
            apt_comp_guid = c.get('__guid')
        elif c.get('__type') == 'UltimoBarrio.Apartments.ApartmentClaimInteractable':
            claim_comp_guid = c.get('__guid')
    for child in node.get('Children', []):
        find_comps(child)

if 'GameObjects' in prefab:
    for go in prefab['GameObjects']:
        find_comps(go)
else:
    find_comps(prefab)

# 2. Load main.scene
with open('Assets/scenes/main.scene', 'r', encoding='utf-8') as f:
    scene = json.load(f)

# Find World root
world_root = None
for go in scene.get('GameObjects', []):
    if go.get('Name') == 'World':
        world_root = go
        break

if world_root:
    # Remove existing Prototype Apartments
    new_children = [c for c in world_root.get('Children', []) if not ('Prototype Apartment' in c.get('Name', '') or 'apartment-a' in c.get('Name', ''))]
    
    # Create prefab instances
    def create_instance(name, position, apartment_id):
        instance_id = str(uuid.uuid4())
        apt_inst_id = str(uuid.uuid4())
        claim_inst_id = str(uuid.uuid4())
        
        return {
            '__guid': instance_id,
            '__version': 2,
            '__Prefab': 'prefabs/world/pf_apartment_unit.prefab',
            '__PrefabInstancePatch': {
                'AddedObjects': [],
                'RemovedObjects': [],
                'PropertyOverrides': [
                    {
                        'Target': { 'Type': 'GameObject', 'IdValue': root_guid },
                        'Property': 'Name',
                        'Value': name
                    },
                    {
                        'Target': { 'Type': 'GameObject', 'IdValue': root_guid },
                        'Property': 'Position',
                        'Value': position
                    },
                    {
                        'Target': { 'Type': 'Component', 'IdValue': apt_comp_guid },
                        'Property': 'ApartmentId',
                        'Value': apartment_id
                    },
                    {
                        'Target': { 'Type': 'Component', 'IdValue': claim_comp_guid },
                        'Property': 'ApartmentId',
                        'Value': apartment_id
                    }
                ],
                'MovedObjects': []
            },
            '__PrefabIdToInstanceId': {
                root_guid: instance_id,
                apt_comp_guid: apt_inst_id,
                claim_comp_guid: claim_inst_id
            }
        }
    
    a01 = create_instance('apartment-a01', '0,0,0', 'apartment-a01')
    a02 = create_instance('apartment-a02', '300,0,0', 'apartment-a02')
    
    new_children.append(a01)
    new_children.append(a02)
    world_root['Children'] = new_children

# 3. Save main.scene
with open('Assets/scenes/main.scene', 'w', encoding='utf-8') as f:
    json.dump(scene, f, indent=2)

print('Successfully replaced raw objects with prefab instances in main.scene')

