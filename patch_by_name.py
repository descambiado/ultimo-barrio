import json
import re
import os

logs_file = 'grounding_logs.txt'
scene_file = 'Assets/scenes/ultimo_barrio_alpha.scene'

def parse_logs():
    res = {}
    with open(logs_file, 'r', encoding='utf-8') as f:
        for line in f:
            match = re.search(r'\[Grounding\] (.*) grounded to (.*)', line)
            if match:
                name = match.group(1).strip()
                pos = match.group(2).strip()
                if name not in res:
                    res[name] = []
                res[name].append(pos)
    return res

results = parse_logs()
print("Found items to patch:", results.keys())

with open(scene_file, 'r', encoding='utf-8-sig') as f:
    data = json.load(f)

idx_map = {k: 0 for k in results.keys()}
patched_count = 0

def update_node(node):
    global patched_count
    name = node.get('Name')
    if name in results and idx_map[name] < len(results[name]):
        node['Position'] = results[name][idx_map[name]]
        idx_map[name] += 1
        patched_count += 1
        
    children = node.get('Children')
    if children:
        for c in children:
            update_node(c)

if 'GameObjects' in data:
    for go in data['GameObjects']:
        update_node(go)

with open(scene_file, 'w', encoding='utf-8') as f:
    json.dump(data, f, indent=2)

print(f"Patched {patched_count} GameObjects successfully.")
