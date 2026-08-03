import json

with open("Assets/scenes/main.scene", "r", encoding="utf-8") as f:
    data = json.load(f)

world = None
systems = None
for node in data.get("GameObjects", []):
    if node.get("Name") == "World":
        world = node
    if node.get("Name") == "Systems":
        systems = node

def count_component(node, comp_type):
    count = 0
    for c in node.get("Components", []):
        if c.get("__type") == comp_type:
            count += 1
    return count

print("SERVICES IN SYSTEMS:")
if systems:
    print(f"ApartmentClaimService: {count_component(systems, 'UltimoBarrio.Apartments.ApartmentClaimService')}")
    print(f"WorldClock: {count_component(systems, 'UltimoBarrio.WorldTime.WorldClock')}")
    print(f"RaidManager: {count_component(systems, 'UltimoBarrio.Raids.RaidManager')}")
    print(f"Trader: {count_component(systems, 'UltimoBarrio.Trading.Trader')}")
else:
    print("Systems GameObject not found!")

traders_in_world = 0
for c in world.get("Children", []):
    if count_component(c, 'UltimoBarrio.Trading.Trader') > 0:
        traders_in_world += 1

print(f"Traders in World (direct children): {traders_in_world}")
