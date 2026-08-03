
import json
import uuid

with open('Assets/prefabs/player.prefab', 'r', encoding='utf-8') as f:
    data = json.load(f)

# Find the root components
go = data.get('RootObject', data.get('GameObjects', [data])[0] if 'GameObjects' in data else data)
components = go.get('Components', [])
has_screen_panel = any(c.get('__type') == 'Sandbox.ScreenPanel' for c in components)

if not has_screen_panel:
    components.append({
        '__type': 'Sandbox.ScreenPanel',
        '__guid': str(uuid.uuid4()),
        '__enabled': True,
        'AutoScreenScale': True,
        'Opacity': 1,
        'Scale': 1,
        'ScaleStrategy': 'ConsistentHeight',
        'ZIndex': 100
    })
    
    with open('Assets/prefabs/player.prefab', 'w', encoding='utf-8') as f:
        json.dump(data, f, indent=2)
    print('Added ScreenPanel to player GameObject correctly!')
else:
    print('ScreenPanel already exists')

