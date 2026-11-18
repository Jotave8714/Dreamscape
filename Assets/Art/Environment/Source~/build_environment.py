# -*- coding: utf-8 -*-
"""Kit de ambiente procedural do Dreamscape (Blender 5.x).

Continua o visual das fases existentes (DS02/DS03): formas low-poly de cor chapada,
paleta das pastas Assets/Design. Cada peca vira um FBX proprio em Assets/Art/Environment.

Convencoes: Z para cima no Blender; o exportador converte para Y para cima da Unity.
Unidades em metros. Origem de cada peca na base (Z = 0), centrada em X/Y, para encaixar
em grade de 1 m. Pecas modulares usam 2 x 2 m de planta.

Uso dentro do Blender:
    import sys; sys.path.insert(0, "<pasta deste arquivo>")
    import build_environment as be
    be.build_all("<pasta de saida>")
"""
import math
import os

import bmesh
import bpy
from mathutils import Vector

V = Vector

# cores (sRGB linear aproximado = mesmas do _BaseColor dos .mat existentes)
PALETTE = {
    "Seguro": (0.22, 0.51, 0.57, 1),
    "Parede": (0.22, 0.23, 0.31, 1),
    "Chegada": (0.43, 0.72, 0.42, 1),
    "Armadilha": (0.85, 0.35, 0.25, 1),
    "Poder": (0.95, 0.74, 0.22, 1),
    "Velocidade": (1.0, 0.8, 0.2, 1),
    "Temporario": (0.7, 0.5, 0.85, 1),
    "PistaAzul": (0.05, 0.95, 1.0, 1),
    "Metal": (0.35, 0.37, 0.42, 1),
    "MetalEscuro": (0.13, 0.14, 0.18, 1),
    "Pufe": (0.55, 0.25, 0.6, 1),
    "Gravidade": (0.55, 0.85, 1.0, 1),
    "Escudo": (0.4, 0.9, 0.7, 1),
    "Brilho": (1.0, 0.95, 0.75, 1),
}

_materials = {}


def mat(name):
    if name in _materials and _materials[name].name in bpy.data.materials:
        return _materials[name]
    m = bpy.data.materials.new(name)
    m.use_nodes = True
    bsdf = next(n for n in m.node_tree.nodes if n.type == "BSDF_PRINCIPLED")
    bsdf.inputs["Base Color"].default_value = PALETTE[name]
    bsdf.inputs["Roughness"].default_value = 0.85
    if name in ("Poder", "Velocidade", "PistaAzul", "Gravidade", "Escudo", "Brilho"):
        # coletaveis e pistas brilham levemente, como a PistaAzul do DS02
        for key in ("Emission Color", "Emission"):
            if key in bsdf.inputs:
                bsdf.inputs[key].default_value = PALETTE[name]
                break
        if "Emission Strength" in bsdf.inputs:
            bsdf.inputs["Emission Strength"].default_value = 0.6
    _materials[name] = m
    return m


# ---------------------------------------------------------------- primitivas


def _finish(obj, material, bevel=0.0):
    obj.data.materials.append(mat(material))
    if bevel:
        mod = obj.modifiers.new("bevel", "BEVEL")
        mod.width = bevel
        mod.segments = 1
        mod.limit_method = "ANGLE"
    return obj


def box(name, size, loc=(0, 0, 0), material="Seguro", bevel=0.0, rot=(0, 0, 0)):
    """Caixa com centro em `loc` e dimensoes `size`."""
    bpy.ops.mesh.primitive_cube_add(size=1, location=loc, rotation=rot)
    o = bpy.context.object
    o.name = name
    o.scale = size
    bpy.ops.object.transform_apply(scale=True)
    return _finish(o, material, bevel)


def cyl(name, radius, depth, loc=(0, 0, 0), material="Metal", verts=12, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cylinder_add(vertices=verts, radius=radius, depth=depth, location=loc, rotation=rot)
    o = bpy.context.object
    o.name = name
    return _finish(o, material)


def cone(name, r1, r2, depth, loc=(0, 0, 0), material="Armadilha", verts=8, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cone_add(vertices=verts, radius1=r1, radius2=r2, depth=depth, location=loc, rotation=rot)
    o = bpy.context.object
    o.name = name
    return _finish(o, material)


def ico(name, radius, loc=(0, 0, 0), material="Poder", subdiv=1):
    bpy.ops.mesh.primitive_ico_sphere_add(radius=radius, subdivisions=subdiv, location=loc)
    o = bpy.context.object
    o.name = name
    return _finish(o, material)


def torus(name, major, minor, loc=(0, 0, 0), material="Poder", rot=(0, 0, 0), seg=16, minseg=6):
    bpy.ops.mesh.primitive_torus_add(
        major_radius=major, minor_radius=minor, major_segments=seg, minor_segments=minseg,
        location=loc, rotation=rot)
    o = bpy.context.object
    o.name = name
    return _finish(o, material)


def prism_from_points(name, pts2d, thickness, loc=(0, 0, 0), material="Velocidade", rot=(90, 0, 0)):
    """Extrusao de um poligono 2D (plano XZ) com espessura em Y."""
    me = bpy.data.meshes.new(name)
    bm = bmesh.new()
    verts = [bm.verts.new((x, -thickness / 2, z)) for x, z in pts2d]
    face = bm.faces.new(verts)
    res = bmesh.ops.extrude_face_region(bm, geom=[face])
    for v in [e for e in res["geom"] if isinstance(e, bmesh.types.BMVert)]:
        v.co.y += thickness
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    bm.to_mesh(me)
    bm.free()
    o = bpy.data.objects.new(name, me)
    bpy.context.collection.objects.link(o)
    o.location = loc
    bpy.context.view_layer.objects.active = o
    return _finish(o, material)


# ---------------------------------------------------------------- cena/export


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete()
    for block in (bpy.data.meshes, bpy.data.materials):
        for item in list(block):
            if item.users == 0:
                block.remove(item)
    _materials.clear()


def join_and_export(name, outdir, objs):
    bpy.ops.object.select_all(action="DESELECT")
    for o in objs:
        o.select_set(True)
        for mod in list(o.modifiers):
            bpy.context.view_layer.objects.active = o
            bpy.ops.object.modifier_apply(modifier=mod.name)
    bpy.context.view_layer.objects.active = objs[0]
    bpy.ops.object.join()
    final = bpy.context.object
    final.name = name
    bpy.ops.object.shade_flat()
    bpy.ops.object.select_all(action="DESELECT")
    final.select_set(True)
    path = os.path.join(outdir, name + ".fbx")
    bpy.ops.export_scene.fbx(
        filepath=path,
        use_selection=True,
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z",
        axis_up="Y",
        bake_space_transform=True,
        mesh_smooth_type="FACE",
        add_leaf_bones=False,
        path_mode="COPY",
        embed_textures=False,
    )
    tris = sum(len(p.vertices) - 2 for p in final.data.polygons)
    return path, tris


# ---------------------------------------------------------------- pecas


def piece_plataforma():
    """Base 2 x 2 x 0,5 m com friso fino de acento inset na borda de cima (o miolo fica na cor do piso)."""
    parts = [box("p", (2, 2, 0.5), (0, 0, 0.25), "Seguro", 0.04)]
    inset, w, h = 0.14, 0.06, 0.03
    for sx, sy, lx, ly in ((1, 0, 0.0, 1 - inset), (1, 0, 0.0, -(1 - inset)),
                           (0, 1, 1 - inset, 0.0), (0, 1, -(1 - inset), 0.0)):
        length = 2 * (1 - inset) + w if sx else 2 * (1 - inset) - w
        size = (length, w, h) if sx else (w, length, h)
        parts.append(box("f", size, (lx, ly, 0.5 + h / 2), "PistaAzul"))
    return parts


def piece_plataforma_estreita():
    return [box("p", (1, 4, 0.4), (0, 0, 0.2), "Seguro", 0.03),
            box("l", (0.08, 3.6, 0.06), (0, 0, 0.42), "PistaAzul")]


def piece_parede():
    return [box("w", (2, 0.3, 3), (0, 0, 1.5), "Parede", 0.03),
            box("r", (2, 0.36, 0.18), (0, 0, 0.09), "MetalEscuro"),
            box("t", (2, 0.36, 0.12), (0, 0, 2.94), "MetalEscuro")]


def piece_pilar():
    return [box("c", (0.6, 0.6, 3), (0, 0, 1.5), "Parede", 0.03),
            box("b", (0.8, 0.8, 0.2), (0, 0, 0.1), "MetalEscuro"),
            box("t", (0.8, 0.8, 0.2), (0, 0, 2.9), "MetalEscuro")]


def piece_porta_conclusao():
    parts = [box("l", (0.4, 0.5, 3.2), (-1.0, 0, 1.6), "Parede", 0.03),
             box("r", (0.4, 0.5, 3.2), (1.0, 0, 1.6), "Parede", 0.03),
             box("t", (2.4, 0.5, 0.4), (0, 0, 3.4), "Parede", 0.03),
             box("v", (1.6, 0.1, 3.0), (0, 0, 1.5), "Chegada")]
    parts[3].modifiers.clear()
    return parts


def piece_checkpoint_pufe():
    """Pufe da Unifor: o sono do protagonista e o ponto de retorno do checkpoint."""
    body = cyl("p", 0.55, 0.6, (0, 0, 0.3), "Pufe", 16)
    top = cyl("t", 0.45, 0.14, (0, 0, 0.66), "Pufe", 16)
    ring = torus("a", 0.5, 0.05, (0, 0, 0.62), "Brilho", seg=20)
    return [body, top, ring]


def piece_pickup_pulo_duplo():
    """Dois aneis cruzados (salto sobre salto) em torno de um nucleo."""
    return [ico("n", 0.16, (0, 0, 0.7), "Poder"),
            torus("a1", 0.32, 0.05, (0, 0, 0.7), "Poder", rot=(math.radians(90), 0, 0)),
            torus("a2", 0.32, 0.05, (0, 0, 0.7), "Poder", rot=(0, math.radians(90), 0)),
            cyl("base", 0.18, 0.06, (0, 0, 0.03), "MetalEscuro", 12)]


def piece_pickup_velocidade():
    bolt = [(-0.12, 0.45), (0.1, 0.45), (0.0, 0.18), (0.16, 0.18), (-0.1, -0.2), (-0.02, 0.05), (-0.18, 0.05)]
    o = prism_from_points("raio", [(x * 1.2, z * 1.2) for x, z in bolt], 0.14, (0, 0, 0.7), "Velocidade")
    o.rotation_euler = (math.radians(90), 0, 0)
    return [o, cyl("base", 0.18, 0.06, (0, 0, 0.03), "MetalEscuro", 12)]


def piece_pickup_gravidade():
    """Pluma: esfera leve e dois aneis inclinados; reduz a gravidade."""
    return [ico("n", 0.2, (0, 0, 0.7), "Gravidade", 2),
            torus("a1", 0.34, 0.035, (0, 0, 0.7), "Gravidade", rot=(math.radians(60), 0, 0)),
            torus("a2", 0.34, 0.035, (0, 0, 0.7), "Gravidade", rot=(math.radians(-60), 0, math.radians(90))),
            cyl("base", 0.18, 0.06, (0, 0, 0.03), "MetalEscuro", 12)]


def piece_pickup_escudo():
    """Escudo hexagonal: protege de uma armadilha."""
    hexa = [(math.cos(a) * 0.32, math.sin(a) * 0.32 + 0.0) for a in [math.radians(30 + 60 * i) for i in range(6)]]
    o = prism_from_points("hex", hexa, 0.1, (0, 0, 0.7), "Escudo")
    o.rotation_euler = (math.radians(90), 0, 0)
    inner = prism_from_points("hexi", [(x * 0.5, z * 0.5) for x, z in hexa], 0.16, (0, 0, 0.7), "Brilho")
    inner.rotation_euler = (math.radians(90), 0, 0)
    return [o, inner, cyl("base", 0.18, 0.06, (0, 0, 0.03), "MetalEscuro", 12)]


def piece_trap_espinhos():
    parts = [box("b", (2, 2, 0.14), (0, 0, 0.07), "MetalEscuro")]
    for i in range(4):
        for j in range(4):
            x, y = -0.75 + i * 0.5, -0.75 + j * 0.5
            parts.append(cone(f"e{i}{j}", 0.17, 0.0, 0.55, (x, y, 0.14 + 0.275), "Armadilha", 6))
    return parts


def piece_trap_serra():
    """Disco de serra com dentes, origem no CENTRO do disco (gira em torno dele na Unity).

    Na fase, o objeto fica a 0,85 m do piso e desliza sobre ele; o eixo do disco e o Z da Unity.
    """
    parts = [cyl("disco", 0.7, 0.08, (0, 0, 0), "Metal", 24, rot=(math.radians(90), 0, 0)),
             cyl("hub", 0.18, 0.14, (0, 0, 0), "Armadilha", 12, rot=(math.radians(90), 0, 0))]
    for i in range(12):
        a = math.radians(i * 30)
        x, z = math.cos(a) * 0.74, math.sin(a) * 0.74
        parts.append(cone(f"d{i}", 0.12, 0.0, 0.2, (x, 0, z), "Armadilha", 3,
                          rot=(math.radians(90), 0, 0)))
        parts[-1].rotation_euler = (0, math.radians(90) - a, 0)
    return parts


def piece_trap_piso_falso():
    """Piso que cede: mesma cor do Temporario do DS03 com rachaduras escuras."""
    parts = [box("p", (2, 2, 0.4), (0, 0, 0.2), "Temporario", 0.03)]
    for i, (x, y, w, h, r) in enumerate([(-0.3, 0.1, 1.3, 0.05, 20), (0.4, -0.4, 0.9, 0.05, -35),
                                         (0.1, 0.55, 0.8, 0.05, 70)]):
        parts.append(box(f"c{i}", (w, h, 0.03), (x, y, 0.41), "MetalEscuro", rot=(0, 0, math.radians(r))))
    return parts


def piece_trap_pendulo():
    parts = [box("t", (0.3, 0.3, 0.3), (0, 0, 3.8), "MetalEscuro"),
             cyl("haste", 0.04, 2.6, (0, 0, 2.5), "Metal", 8),
             ico("massa", 0.45, (0, 0, 1.15), "Armadilha", 2)]
    for i in range(6):
        a = math.radians(i * 60)
        parts.append(cone(f"e{i}", 0.1, 0.0, 0.3, (math.cos(a) * 0.5, math.sin(a) * 0.5, 1.15),
                          "Metal", 4, rot=(0, math.radians(90), a)))
    return parts


def piece_trap_laser():
    """Dois postes com emissores; o feixe e o colisor sao criados na Unity (o feixe pisca)."""
    return [box("pa", (0.3, 0.3, 1.4), (-1.5, 0, 0.7), "MetalEscuro", 0.03),
            box("pb", (0.3, 0.3, 1.4), (1.5, 0, 0.7), "MetalEscuro", 0.03),
            box("ea", (0.34, 0.34, 0.14), (-1.5, 0, 1.0), "Armadilha"),
            box("eb", (0.34, 0.34, 0.14), (1.5, 0, 1.0), "Armadilha")]


def piece_trap_prensa():
    """Bloco que desce sobre o piso (parede esmagadora curta)."""
    return [box("g", (2, 0.3, 0.3), (0, 0, 3.4), "MetalEscuro"),
            box("b", (1.6, 1.6, 0.9), (0, 0, 2.4), "Armadilha", 0.04),
            box("l1", (0.12, 0.12, 1.8), (-0.7, 0, 3.3), "Metal"),
            box("l2", (0.12, 0.12, 1.8), (0.7, 0, 3.3), "Metal")]


def piece_poste_luz():
    return [cyl("p", 0.06, 2.6, (0, 0, 1.3), "MetalEscuro", 8),
            box("b", (0.4, 0.4, 0.1), (0, 0, 0.05), "MetalEscuro"),
            box("c", (0.5, 0.5, 0.12), (0, 0, 2.65), "MetalEscuro"),
            ico("lamp", 0.2, (0, 0, 2.5), "Brilho", 1)]


def piece_luminaria_teto():
    return [box("p", (1.6, 0.5, 0.08), (0, 0, 0.2), "MetalEscuro"),
            box("l", (1.4, 0.3, 0.06), (0, 0, 0.14), "Brilho")]


def piece_rampa():
    # cunha simples: caixa fina inclinada (2 m de planta, 0,5 m de subida)
    ang = math.atan2(0.5, 2.0)
    return [box("r", (math.hypot(2, 0.5), 2, 0.2), (0, 0, 0.25), "Seguro", 0.02, rot=(0, -ang, 0))]


PIECES = {
    "Plataforma_2x2": piece_plataforma,
    "Plataforma_Estreita": piece_plataforma_estreita,
    "Parede_Modulo": piece_parede,
    "Pilar": piece_pilar,
    "Porta_Conclusao": piece_porta_conclusao,
    "Checkpoint_Pufe": piece_checkpoint_pufe,
    "Pickup_PuloDuplo": piece_pickup_pulo_duplo,
    "Pickup_Velocidade": piece_pickup_velocidade,
    "Pickup_Gravidade": piece_pickup_gravidade,
    "Pickup_Escudo": piece_pickup_escudo,
    "Trap_Espinhos": piece_trap_espinhos,
    "Trap_Serra": piece_trap_serra,
    "Trap_PisoFalso": piece_trap_piso_falso,
    "Trap_Pendulo": piece_trap_pendulo,
    "Trap_Laser": piece_trap_laser,
    "Trap_Prensa": piece_trap_prensa,
    "Poste_Luz": piece_poste_luz,
    "Luminaria_Teto": piece_luminaria_teto,
    "Rampa": piece_rampa,
}


def build_all(outdir, only=None):
    os.makedirs(outdir, exist_ok=True)
    report = {}
    for name, fn in PIECES.items():
        if only and name not in only:
            continue
        clear_scene()
        objs = fn()
        path, tris = join_and_export(name, outdir, objs)
        report[name] = tris
    return report
