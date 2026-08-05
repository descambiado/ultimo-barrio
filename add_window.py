# coding=utf-8
import json
import uuid

scene_file = 'Assets/scenes/ultimo_barrio_alpha.scene'
with open(scene_file, 'r', encoding='utf-8') as f:
    data = json.load(f)

def create_window():
    return {
        "__guid": str(uuid.uuid4()),
        "Flags": 0,
        "Name": "Ventana Rompible",
        "Position": "0,0,50",
        "Enabled": True,
        "Components": [
            {
                "__type": "Sandbox.ModelRenderer",
                "__guid": str(uuid.uuid4()),
                "Model": "models/props_c17/window02a.vmdl",
                "Tint": "1,1,1,1"
            },
            {
                "__type": "Sandbox.BoxCollider",
                "__guid": str(uuid.uuid4()),
                "IsTrigger": False,
                "Scale": "50,5,50"
            },
            {
                "__type": "UltimoBarrio.Combat.HealthComponent",
                "__guid": str(uuid.uuid4()),
                "MaxHealth": 20,
                "CurrentHealth": 20
            }
        ],
        "Children": []
    }

def process_node(node):
    if node.get('Name') == 'ApartmentWindowAnchor':
        # Añadir ventana
        if 'Children' not in node:
            node['Children'] = []
        # check si ya tiene la ventana
        has_win = any(c.get('Name') == 'Ventana Rompible' for c in node['Children'])
        if not has_win:
            node['Children'].append(create_window())
            print("Added Ventana Rompible to an ApartmentWindowAnchor")
            
    for child in node.get('Children', []):
        process_node(child)

for go in data.get('GameObjects', []):
    process_node(go)

with open(scene_file, 'w', encoding='utf-8') as f:
    json.dump(data, f, indent=2)
