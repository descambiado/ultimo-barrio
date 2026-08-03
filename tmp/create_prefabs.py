
import os
import json
import uuid

def create_prefab(path, name, components):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    comp_list = []
    for c in components:
        comp = {
            '__guid': str(uuid.uuid4()),
            '__enabled': True
        }
        comp.update(c)
        comp_list.append(comp)

    data = {
        '__guid': str(uuid.uuid4()),
        '__version': 2,
        'Flags': 0,
        'Name': name,
        'Position': '0,0,0',
        'Rotation': '0,0,0,1',
        'Scale': '1,1,1',
        'Tags': '',
        'Enabled': True,
        'NetworkMode': 1,
        'NetworkFlags': 0,
        'NetworkOrphaned': 0,
        'NetworkTransmit': True,
        'OwnerTransfer': 1,
        'Components': comp_list,
        'Children': []
    }
    with open(path, 'w', encoding='utf-8') as f:
        json.dump({'RootObject': str(uuid.uuid4()), 'GameObjects': [data]}, f, indent=2)

create_prefab('Assets/prefabs/inventory/pf_apartment_stash.prefab', 'Apartment Stash', [
    {'__type': 'Sandbox.BoxCollider', 'Scale': '60,60,60', 'Static': True},
    {'__type': 'Sandbox.ModelRenderer', 'Model': 'models/dev/box.vmdl'}
])

create_prefab('Assets/prefabs/items/pf_scrap_pickup.prefab', 'Scrap Pickup', [
    {'__type': 'UltimoBarrio.Items.WorldItemPickup', 'ItemId': 'item-scrap', 'Amount': 1},
    {'__type': 'Sandbox.BoxCollider', 'Scale': '10,10,10'},
    {'__type': 'Sandbox.ModelRenderer', 'Model': 'models/dev/box.vmdl', 'Tint': '0.5,0.5,0.5,1'}
])

create_prefab('Assets/prefabs/player/pf_player_hud.prefab', 'Player HUD', [
    {'__type': 'UltimoBarrio.UI.PlayerHud'},
    {'__type': 'Sandbox.ScreenPanel'}
])
print('Prefabs created.')

