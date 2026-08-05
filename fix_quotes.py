with open('Code/UltimoBarrio/Crafting/CraftingStation.cs', 'r', encoding='utf-8') as f:
    text = f.read()

text = text.replace('\"\"', '\"')

with open('Code/UltimoBarrio/Crafting/CraftingStation.cs', 'w', encoding='utf-8') as f:
    f.write(text)
