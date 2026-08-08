#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Genera las escenas de laboratorio (spike) del nodo portátil:
  Assets/scenes/spikes/weapon_lab.scene
  Assets/scenes/spikes/enemy_lab.scene
  Assets/scenes/spikes/building_lab.scene
  Assets/scenes/spikes/vehicle_lab.scene

Siguen el formato serializado verificado de main.scene / ultimo_barrio_alpha.scene
(__version 2, NetworkHelper StartServer, prefab jugador player.prefab).

Uso:  python scripts/labs/generate_lab_scenes.py
Re-ejecutable: regenera GUIDs nuevos; NO editar a mano si se va a re-generar.
"""
import json
import os
import uuid

SCENES_DIR = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "scenes", "spikes")


def g():
    return str(uuid.uuid4())


def go(name, components, position="0,0,0", rotation="0,0,0,1", scale="1,1,1",
       tags="", children=None, network_mode=2):
    return {
        "__guid": g(),
        "__version": 2,
        "Flags": 0,
        "Name": name,
        "Position": position,
        "Rotation": rotation,
        "Scale": scale,
        "Tags": tags,
        "Enabled": True,
        "NetworkMode": network_mode,
        "NetworkFlags": 0,
        "NetworkOrphaned": 0,
        "NetworkTransmit": True,
        "OwnerTransfer": 1,
        "Components": components,
        "Children": children or [],
    }


def comp(ctype, **props):
    c = {"__type": ctype, "__guid": g(), "__enabled": True}
    c.update(props)
    return c


def go_ref(guid):
    return {"_type": "gameobject", "go": guid}


def model_renderer(model):
    return comp("Sandbox.ModelRenderer", Model=model)


def plane_floor():
    return go("PrototypeFloor", [
        comp("Sandbox.ModelRenderer", Model="models/dev/plane.vmdl",
             MaterialOverride="materials/default.vmat", Tint="0.3,0.38,0.27,1"),
        comp("Sandbox.PlaneCollider", Normal="0,0,1", Scale="100,100", Static=True),
    ], scale="8,8,1")


def sun():
    return go("Sun", [comp("Sandbox.DirectionalLight")], position="0,0,200",
              rotation="0.0990457684,0.369643837,-0.239117607,0.892399073",
              tags="light_directional,light")


def sky():
    return go("Sky", [comp("Sandbox.SkyBox2D", SkyMaterial="materials/skybox/skybox_day_01.vmat")],
              tags="skybox")


def network_systems():
    return go("Network", [comp("Sandbox.NetworkHelper",
                               PlayerPrefab={"_type": "gameobject", "prefab": "prefabs/player.prefab"},
                               SpawnPoints=[], StartServer=True)])


def network_systems_no_player():
    return go("Network", [comp("Sandbox.NetworkHelper", SpawnPoints=[], StartServer=True)])


def spawn_points():
    return go("SpawnPoints", [], children=[
        go("Primary Spawn", [comp("Sandbox.SpawnPoint", Color="0.2,0.8,0.3,1")])
    ])


def world_root(children):
    return go("World", [], children=children)


def systems_root(children):
    return go("Systems", [], children=children)


def write_scene(name, game_objects):
    os.makedirs(SCENES_DIR, exist_ok=True)
    path = os.path.join(SCENES_DIR, name)
    with open(path, "w", encoding="utf-8") as f:
        json.dump({"__guid": g(), "GameObjects": game_objects}, f, indent=2, ensure_ascii=False)
    print("generada: " + path)


def weapon_lab():
    write_scene("weapon_lab.scene", [
        world_root([plane_floor(), sun(), sky()]),
        systems_root([network_systems(), go("Lab Systems", [comp("UltimoBarrio.Content.Dev.LabWeaponSpawner")])]),
        spawn_points(),
    ])


def enemy_lab():
    # MapInstance real para que NavMeshAgent tenga navmesh (mismo patrón que la alpha).
    map_instance = go("MapInstance", [comp("Sandbox.MapInstance", MapName="thieves.rpdowntown3t",
                                           UseMapFromLaunch=True)])
    dummy = go("Lab Dummy", [
        model_renderer("models/citizen_props/crate01.vmdl"),
        comp("Sandbox.BoxCollider", Scale="80,80,120", Static=False),
        comp("UltimoBarrio.Content.Fortification.BuildStructureHost", DefinitionId="fort_barricade_wood"),
    ], position="300,0,0", tags="lab_dummy")
    marker = go("Enemy Spawn Marker", [], position="100,0,0", tags="lab_marker")

    spawner = go("Lab Systems", [
        comp("UltimoBarrio.Content.Dev.LabEnemySpawner",
             SpawnMarker=go_ref(marker["__guid"]),
             TargetDummy=go_ref(dummy["__guid"]))
    ])

    write_scene("enemy_lab.scene", [
        map_instance,
        world_root([dummy, marker]),
        systems_root([network_systems(), spawner]),
        spawn_points(),
    ])


def building_lab():
    # Autotest BuildingTestRig: NetworkHelper SIN PlayerPrefab (el rig sustituye
    # input humano y target humano). La suite data-driven vive en el JSON de la escena.
    rig = go("Building Test Rig", [
        comp("UltimoBarrio.Content.Dev.BuildingTestRig",
             AutoTest=True,
             Tests=[{
                 "Label": "WoodenBarricade",
                 "BuildId": "fort_barricade_wood",
                 "ValidDistance": 160.0,
                 "InvalidDistance": 500.0,
                 "BlockedDistance": 220.0,
                 "SpawnYaw": 45.0,
                 "ExpectedMaxHp": 150.0,
                 "DamageAmount": 50.0,
                 "FixtureBalance": 100,
                 "ExpectedUpgradeMaxHp": 400.0,
             }]),
        comp("Sandbox.CameraComponent", IsMainCamera=True, Priority=10),
    ], position="0,0,8")
    write_scene("building_lab.scene", [
        world_root([plane_floor(), sun(), sky()]),
        systems_root([network_systems_no_player(), go("Lab Systems", [rig])]),
        spawn_points(),
    ])


def vehicle_lab():
    marker = go("Vehicle Spawn Marker", [], position="200,0,0", tags="lab_marker")
    spawner = go("Lab Systems", [
        comp("UltimoBarrio.Content.Dev.LabVehicleSpawner", SpawnMarker=go_ref(marker["__guid"]))
    ])
    write_scene("vehicle_lab.scene", [
        world_root([plane_floor(), sun(), sky(), marker]),
        systems_root([network_systems(), spawner]),
        spawn_points(),
    ])


if __name__ == "__main__":
    weapon_lab()
    enemy_lab()
    building_lab()
    vehicle_lab()
    print("OK: 4 escenas de laboratorio generadas.")
