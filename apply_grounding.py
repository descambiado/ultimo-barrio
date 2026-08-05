import json
import os

scene_file = 'Assets/scenes/ultimo_barrio_alpha.scene'
results_file = 'grounding_results.json'

if not os.path.exists(results_file):
    print("No grounding results found.")
    exit(1)

with open(results_file, 'r', encoding='utf-8') as f:
    results = json.load(f)

with open(scene_file, 'r', encoding='utf-8') as f:
    scene = json.load(f)

def update_node(node):
    node_id = node.get('Id')
    if node_id in results:
        vec_str = results[node_id]
        if isinstance(vec_str, str):
            node['WorldPosition'] = vec_str
        elif isinstance(vec_str, dict):
            # sbox Sandbox.Vector3 serialization
            x = vec_str.get('x', 0)
            y = vec_str.get('y', 0)
            z = vec_str.get('z', 0)
            node['WorldPosition'] = f"{x},{y},{z}"
            
    children = node.get('Children')
    if children:
        for c in children:
            update_node(c)

if 'GameObjects' in scene:
    for go in scene['GameObjects']:
        update_node(go)
else:
    update_node(scene)

with open(scene_file, 'w', encoding='utf-8') as f:
    json.dump(scene, f, indent=2)

print("Scene updated successfully with grounded positions.")
