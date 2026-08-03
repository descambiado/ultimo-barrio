# Cloud Asset Catalog — Último Barrio (Alpha 0.1)

Catálogo oficial de Cloud Assets y UGC integrados desde s&box Workshop para la producción visual y auditiva de Último Barrio.

---

## 1. Paquetes Base Oficiales (Facepunch)

| Package Ident | Nombre | Autor | Tipo | Licencia | Uso | Fallback |
|---|---|---|---|---|---|---|
| `facepunch.sboxassets` | s&box Standard Assets | Facepunch | Models, Materials, Sounds | Official | Props urbanos, contenedores, puertas, luces, texturas | Local default |
| `facepunch.sboxweapons` | s&box Official Weapons | Facepunch | Prefabs, Animations | Official | Animaciones en 1ª persona, brazos y armas | Modelos dev |
| `facepunch.v_usp` | Viewmodel USP | Facepunch | Model / Animation | Official | Viewmodel de la pistola USP | Default viewmodel |
| `facepunch.w_usp` | Worldmodel USP | Facepunch | Model | Official | Worldmodel y pickup de la pistola USP | Default worldmodel |
| `facepunch.v_first_person_arms_human` | First Person Arms | Facepunch | Model / Rig | Official | Rig de brazos en primera persona | Citizen arms |
| `facepunch.ammobox9mm` | 9mm Ammo Box | Facepunch | Model | Official | Modelo visual para cajas de munición de 9mm | Box model |

---

## 2. Modelos & Props de Entorno (`facepunch.sboxassets`)

| Elemento | Identificador / Ruta | Uso | Zona | Colisión |
|---|---|---|---|---|
| Contenedores Basura | `models/sbox_props/metal_wheely_bin/metal_wheely_bin.vmdl` | Decoración y looteo de chatarra | Chatarrería / Callejón | Convex Physics |
| Cajas Plásticas | `models/sbox_props/plastic_crate/plastic_crate.vmdl` | Nodos de recursos | Plaza / Taller | Box Collider |
| Caja Registradora | `models/sbox_props/cash_register/cash_register.vmdl` | Mostrador del Comerciante | Kiosco Trader | Static Mesh |
| Puertas de Madera | `models/sbox_props/wooden_door/wooden_door.vmdl` | Accesos a viviendas | Apartamentos A01 - A06 | Door Collider |
| Farolas Ámbar | `models/sbox_props/street_lamp/street_lamp.vmdl` | Iluminación nocturna | Calle Principal | Capsule Collider |
| Pilas de Chatarra | `models/sbox_props/trash_pile/trash_pile.vmdl` | Nodos de recolección | Chatarrería | Mesh Collider |

---

## 3. Catálogo de Audio (Cloud Sounds CC0 / Official)

| Identificador SoundEvent | Tipo | Rango 3D | Uso |
|---|---|---|---|
| `ui.button.press` | UI | Local | Click de interfaz y navegación |
| `ui.button.deny` | UI | Local | Rechazo de acción o transacción |
| `pickup.scrap` | World | 300u | Recolección de chatarra |
| `pickup.water` | World | 300u | Recolección de agua |
| `pickup.medicine` | World | 300u | Recolección de medicinas |
| `pickup.ammo` | World | 300u | Recolección de munición |
| `trader.buy` | World / UI | Local | Transacción monetaria con éxito |
| `door.open` | World | 800u | Apertura de puerta de apartamento |
| `door.close` | World | 800u | Cierre de puerta |
| `door.locked` | World | 500u | Intento de apertura de puerta bloqueada |
| `stash.open` | World | 400u | Apertura del alijo de vivienda |
| `weapon.usp.shoot` | Weapons | 2000u | Disparo de pistola USP |
| `weapon.usp.reload` | Weapons | 600u | Recarga de cargador |
| `raid.siren` | Ambience | 5000u | Sirena de inicio de fase Noche / Raid |
