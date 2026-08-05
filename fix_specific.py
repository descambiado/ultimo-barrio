files = [
    'Code/UltimoBarrio/Crafting/CraftingStation.cs',
    'Code/UltimoBarrio/Items/WorldItemPickup.cs',
    'Code/UltimoBarrio/Items/HeldConsumable.cs'
]
for f in files:
    with open(f, 'r', encoding='utf-8') as file:
        text = file.read()
    text = text.replace('\"\"', '\"')
    with open(f, 'w', encoding='utf-8') as file:
        file.write(text)
