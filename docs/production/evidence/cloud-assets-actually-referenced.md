# Real Cloud & Package Asset Reference Audit

Auditoría directa de referencias serializadas en `Assets/scenes/ultimo_barrio_alpha.scene`, `Assets/prefabs/player.prefab` y `Assets/prefabs/weapons/ub_usp.prefab`.

---

## 1. Paquetes y Recursos Realmente Referenciados en Escena / Prefabs

| Package / Resource Ident | Ruta Exacta de Asset | Archivo que lo Referencia | Objeto / Componente | Estado de Carga Runtime |
|---|---|---|---|---|
| `thieves.rpdowntown3t` | `thieves.rpdowntown3t` | `Assets/scenes/ultimo_barrio_alpha.scene` | `MapInstance` (`Sandbox.MapInstance.MapName`) | Carga remota en s&box |
| `models/sbox_props/wooden_door/wooden_door.vmdl` | `models/sbox_props/wooden_door/wooden_door.vmdl` | `Assets/scenes/ultimo_barrio_alpha.scene` | `Claim Portal` (`Sandbox.ModelRenderer.Model`) | Asset local s&box |
| `models/sbox_props/plastic_crate/plastic_crate.vmdl` | `models/sbox_props/plastic_crate/plastic_crate.vmdl` | `Assets/scenes/ultimo_barrio_alpha.scene` | `Stash Anchor` (`Sandbox.ModelRenderer.Model`) | Asset local s&box |
| `models/sbox_props/cash_register/cash_register.vmdl` | `models/sbox_props/cash_register/cash_register.vmdl` | `Assets/scenes/ultimo_barrio_alpha.scene` | `Kiosko Comerciante` (`Sandbox.ModelRenderer.Model`) | Asset local s&box |
| `models/sbox_props/metal_wheely_bin/metal_wheely_bin.vmdl` | `models/sbox_props/metal_wheely_bin/metal_wheely_bin.vmdl` | `Assets/scenes/ultimo_barrio_alpha.scene` | `Chatarra 1..20` (`Sandbox.ModelRenderer.Model`) | Asset local s&box |
| `models/citizen/citizen.vmdl` | `models/citizen/citizen.vmdl` | `Assets/prefabs/player.prefab` | `Body` (`Sandbox.SkinnedModelRenderer.Model`) | Asset local s&box |
| `models/dev/box.vmdl` | `models/dev/box.vmdl` | `Assets/prefabs/weapons/ub_usp.prefab` | `UB USP Pistol` (`Sandbox.ModelRenderer.Model`) | Asset dev (Cube) |

---

## 2. Paquetes Declarados pero NO Referenciados Serializadamente

Los siguientes idents del Workshop figuran en catálogos de documentación pero **NO** poseen referencias directas serializadas en la escena ni en los prefabs actuales:
- `facepunch.sboxassets`
- `facepunch.sboxweapons`
- `facepunch.v_usp`
- `facepunch.w_usp`
- `facepunch.ammobox9mm`

---

## 3. Contenido Real de `Assets/prefabs/weapons/ub_usp.prefab`

```json
{
  "RootObject": {
    "__guid": "a7b8c9d0-1e2f-3a4b-5c6d-7e8f9a0b1c2d",
    "Name": "UB USP Pistol",
    "Components": [
      {
        "__type": "UltimoBarrio.Combat.UltimoBarrioWeaponAdapter",
        "WeaponId": "weapon_usp",
        "AmmoItemId": "ammo_9mm",
        "ClipSize": 12,
        "DamagePerShot": 25
      },
      {
        "__type": "Sandbox.BoxCollider",
        "Scale": "15,15,15"
      },
      {
        "__type": "Sandbox.ModelRenderer",
        "Model": "models/dev/box.vmdl",
        "Tint": "0.1,0.1,0.1,1"
      }
    ]
  }
}
```
*Nota*: Contiene un placeholder `models/dev/box.vmdl`.
