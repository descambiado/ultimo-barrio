import os
import re

def replace_in_file(path, replacements):
    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()
    
    for old, new in replacements:
        content = content.replace(old, new)
        
    # Ensure using UltimoBarrio.Core; is present
    if 'using UltimoBarrio.Core;' not in content and ('IInteractable' in content or 'IDamageable' in content or 'IInventory' in content or 'InteractionRequest' in content or 'DamageEvent' in content):
        if 'using System;' in content:
            content = content.replace('using System;', 'using System;\nusing UltimoBarrio.Core;')
        else:
            content = 'using UltimoBarrio.Core;\n' + content
            
    with open(path, 'w', encoding='utf-8') as f:
        f.write(content)

base_dir = 'Code/UltimoBarrio'

replace_in_file(os.path.join(base_dir, 'Inventory/StashComponent.cs'), [
    ('IInventoryOwner', 'IInventory'),
    ('public string GetInteractionPrompt()', 'public string GetInteractionPrompt(InteractionRequest request)'),
    ('public bool CanInteract(Guid playerId)', 'public bool CanInteract(InteractionRequest request)'),
    ('public void OnInteract(Guid playerId)', 'public void OnInteract(InteractionRequest request)'),
    ('using Sandbox;', 'using Sandbox;\nusing UltimoBarrio.Core;')
])

replace_in_file(os.path.join(base_dir, 'Items/WorldItemPickup.cs'), [
    ('public string GetInteractionPrompt()', 'public string GetInteractionPrompt(InteractionRequest request)'),
    ('public bool CanInteract(Guid playerId)', 'public bool CanInteract(InteractionRequest request)'),
    ('public void OnInteract(Guid playerId)', 'public void OnInteract(InteractionRequest request)'),
])

replace_in_file(os.path.join(base_dir, 'PlayerInteractor.cs'), [
    ('interactable.CanInteract( player.GameObject.Id )', 'interactable.CanInteract( new InteractionRequest { InteractorId = player.GameObject.Network.OwnerId.ToString(), InteractorObject = player.GameObject } )'),
    ('interactable.GetInteractionPrompt()', 'interactable.GetInteractionPrompt(new InteractionRequest { InteractorId = player.GameObject.Network.OwnerId.ToString(), InteractorObject = player.GameObject })'),
    ('interactable.OnInteract( player.GameObject.Id )', 'interactable.OnInteract( new InteractionRequest { InteractorId = player.GameObject.Network.OwnerId.ToString(), InteractorObject = player.GameObject } )')
])

replace_in_file(os.path.join(base_dir, 'Trading/Trader.cs'), [
    ('IInventoryOwner', 'IInventory'),
    ('public string GetInteractionPrompt()', 'public string GetInteractionPrompt(InteractionRequest request)'),
    ('public bool CanInteract(Guid playerId)', 'public bool CanInteract(InteractionRequest request)'),
    ('public void OnInteract(Guid playerId)', 'public void OnInteract(InteractionRequest request)')
])

replace_in_file(os.path.join(base_dir, 'AI/AIBase.cs'), [
    ('public void TakeDamage(float amount, Vector3 position, Vector3 force, Guid attackerId)', 'public void TakeDamage(DamageEvent damageEvent)')
])

replace_in_file(os.path.join(base_dir, 'Combat/BaseCombatWeapon.cs'), [
    ('UltimoBarrio.IDamageable', 'IDamageable'),
    ('damageable.TakeDamage( Damage, tr.HitPosition, force, ownerId );', 'damageable.TakeDamage( new DamageEvent { Amount = Damage, Position = tr.HitPosition, Force = force, AttackerId = ownerId.ToString() } );')
])

replace_in_file(os.path.join(base_dir, 'Combat/HealthComponent.cs'), [
    ('UltimoBarrio.IDamageable', 'IDamageable'),
    ('public void TakeDamage(float amount, Vector3 position, Vector3 force, Guid attackerId)', 'public void TakeDamage(DamageEvent damageEvent)'),
    ('Health -= amount;', 'Health -= damageEvent.Amount;')
])

replace_in_file(os.path.join(base_dir, 'Combat/BaseInventoryComponent.cs'), [
    ('IInventoryOwner', 'IInventory')
])

replace_in_file(os.path.join(base_dir, 'Inventory/InventoryComponent.cs'), [
    ('IInventoryOwner', 'IInventory')
])

# Also fix attributes
def fix_attributes(path):
    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()
    content = content.replace('[Authority]', '[Rpc.Owner]')
    content = content.replace('[Broadcast]', '[Rpc.Broadcast]')
    content = content.replace('[GameResource(', '[AssetType(')
    with open(path, 'w', encoding='utf-8') as f:
        f.write(content)

import glob
for filepath in glob.glob('Code/UltimoBarrio/**/*.cs', recursive=True):
    fix_attributes(filepath)

print('Refactor done.')
