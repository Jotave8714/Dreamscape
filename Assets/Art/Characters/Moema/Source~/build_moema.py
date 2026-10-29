# -*- coding: utf-8 -*-
"""Modelo 3D procedural da Moema, a ema do Dreamscape.

Gera no Blender (5.x) a Moema da arte conceitual: ema humanizada com penas
azuis em camadas, cabeca e pescoco cinza-azulados, rabo de cavalo azul alto,
olhos grandes castanhos com cilios, bico sorridente aberto, coxas emplumadas,
pernas finas segmentadas e pes de tres dedos.

Convencoes (iguais as do protagonista): Z para cima, personagem olhando para -Y,
lado esquerdo do personagem em +X, pes no chao (Z = 0), metros (altura ~1,9 m
com o rabo de cavalo), pose A para o rig.

Uso dentro do Blender:
    import sys; sys.path.insert(0, "<pasta deste arquivo>")
    import build_moema as bm
    parts = bm.build()          # gera as pecas
    arm, body = bm.rig(parts)   # junta, cria o esqueleto (nomes estilo Mixamo) e os pesos
    bm.export_fbx("<caminho>.fbx", [arm, body])

Ou direto da linha de comando (gera Source~/Moema.blend e ../Moema.fbx):
    blender -b --factory-startup --python build_moema.py
"""
import math
import random

import bmesh
import bpy
from mathutils import Euler, Matrix, Vector

V = Vector
COLL_NAME = "Moema"
PARTS_COLL_NAME = "Moema_Parts"

# ---------------------------------------------------------------- utilidades


def _srgb_to_linear(c):
    return c / 12.92 if c <= 0.04045 else ((c + 0.055) / 1.055) ** 2.4


def hex_rgba(h):
    h = h.lstrip("#")
    return tuple(_srgb_to_linear(int(h[i:i + 2], 16) / 255) for i in (0, 2, 4)) + (1.0,)


PALETTE = {
    # nome: (cor sRGB, rugosidade)
    "Skin": ("#AEB8CC", 0.65),
    "Beak": ("#C4CAD8", 0.45),
    "Mouth": ("#C42A3A", 0.5),
    "Tongue": ("#E86A86", 0.45),
    "EyeWhite": ("#F7F7F5", 0.25),
    "Iris": ("#7A3523", 0.25),
    "Pupil": ("#141012", 0.2),
    "Lash": ("#1E1A22", 0.5),
    "Hair": ("#2E5BE6", 0.45),
    "HairLight": ("#79A3FF", 0.45),
    "HairTie": ("#16267A", 0.5),
    "Feather": ("#2649D6", 0.6),
    "FeatherDark": ("#1A3399", 0.65),
    "FeatherLight": ("#4F7AF0", 0.6),
    "FeatherTip": ("#5B83F2", 0.6),
    "Leg": ("#D6D4CE", 0.7),
    "LegBand": ("#9A9CA6", 0.7),
    "Claw": ("#5A5C66", 0.5),
}
MATS = {}


def build_materials():
    for name, (hexc, rough) in PALETTE.items():
        full = "Moema_" + name
        m = bpy.data.materials.get(full) or bpy.data.materials.new(full)
        m.use_nodes = True
        bsdf = next(n for n in m.node_tree.nodes if n.type == "BSDF_PRINCIPLED")
        bsdf.inputs["Base Color"].default_value = hex_rgba(hexc)
        bsdf.inputs["Roughness"].default_value = rough
        m.diffuse_color = hex_rgba(hexc)
        MATS[name] = m


def collection(name=COLL_NAME, parent=None):
    c = bpy.data.collections.get(name)
    if c is None:
        c = bpy.data.collections.new(name)
        (parent or bpy.context.scene.collection).children.link(c)
    return c


def parts_collection():
    return collection(PARTS_COLL_NAME, collection())


def xform(loc, scale=(1, 1, 1), rot=(0, 0, 0)):
    r = Euler([math.radians(a) for a in rot], "XYZ").to_matrix().to_4x4()
    return Matrix.Translation(V(loc)) @ r @ Matrix.Diagonal((*scale, 1))


def tag(ob, part, side=0):
    ob["moema_part"] = part
    ob["moema_side"] = side
    return ob


def mesh_object(name, bm, mats, part, side=0, smooth=True, subsurf=0):
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me)
    bm.free()
    for mn in mats:
        me.materials.append(MATS[mn])
    if smooth:
        me.polygons.foreach_set("use_smooth", [True] * len(me.polygons))
    ob = bpy.data.objects.new(name, me)
    parts_collection().objects.link(ob)
    if subsurf:
        md = ob.modifiers.new("Subdivision", "SUBSURF")
        md.levels = subsurf
        md.render_levels = subsurf
    return tag(ob, part, side)


def sphere(name, loc, scale, mat, part, side=0, rot=(0, 0, 0), seg=32, rings=16, subsurf=0):
    bm = bmesh.new()
    bmesh.ops.create_uvsphere(bm, u_segments=seg, v_segments=rings, radius=1.0)
    bmesh.ops.transform(bm, matrix=xform(loc, scale, rot), verts=bm.verts)
    return mesh_object(name, bm, [mat], part, side, subsurf=subsurf)


def cone(name, loc, length, w, mat, part, side=0, rot=(0, 0, 0), flat=0.45, seg=14):
    """ponta em -Z local, base larga em +Z local"""
    bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=True, cap_tris=False, segments=seg, radius1=w * 0.12, radius2=w,
                          depth=length)
    bmesh.ops.transform(bm, matrix=xform(loc, (1, flat, 1), rot), verts=bm.verts)
    return mesh_object(name, bm, [mat], part, side)


def torus(name, loc, big_r, small_r, mat, part, side=0, rot=(0, 0, 0), seg=24, mseg=8):
    bm = bmesh.new()
    rings = []
    for i in range(seg):
        a = 2 * math.pi * i / seg
        rings.append([bm.verts.new(((big_r + small_r * math.cos(b)) * math.cos(a),
                                    (big_r + small_r * math.cos(b)) * math.sin(a), small_r * math.sin(b)))
                      for b in (2 * math.pi * j / mseg for j in range(mseg))])
    for i in range(seg):
        for j in range(mseg):
            bm.faces.new((rings[i][j], rings[(i + 1) % seg][j], rings[(i + 1) % seg][(j + 1) % mseg],
                          rings[i][(j + 1) % mseg]))
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces[:])
    bmesh.ops.transform(bm, matrix=xform(loc, (1, 1, 1), rot), verts=bm.verts)
    return mesh_object(name, bm, [mat], part, side)


def lathe(name, rings, mats, part, side=0, seg=32, ring_mats=None, cap=True, subsurf=1, mod=None):
    """rings: (z, rx, ry[, cx, cy]) de baixo para cima; mod(i, j, co) deforma cada vertice."""
    bm = bmesh.new()
    vr = []
    for i, r in enumerate(rings):
        z, rx, ry = r[:3]
        cx, cy = (r[3], r[4]) if len(r) > 3 else (0.0, 0.0)
        row = []
        for j in range(seg):
            t = 2 * math.pi * j / seg
            co = V((cx + math.sin(t) * rx, cy - math.cos(t) * ry, z))
            row.append(bm.verts.new(mod(i, j, co) if mod else co))
        vr.append(row)
    for i in range(len(vr) - 1):
        a, b = vr[i], vr[i + 1]
        for j in range(seg):
            f = bm.faces.new((a[j], a[(j + 1) % seg], b[(j + 1) % seg], b[j]))
            if ring_mats:
                f.material_index = ring_mats[i]
    if cap:
        bm.faces.new(list(reversed(vr[0])))
        bm.faces.new(vr[-1])
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces[:])
    return mesh_object(name, bm, mats, part, side, subsurf=subsurf)


def tube(name, pts, radii, mat, part, side=0, res=6):
    cu = bpy.data.curves.new(name, "CURVE")
    cu.dimensions = "3D"
    cu.bevel_depth = 1.0
    cu.bevel_resolution = res
    cu.use_fill_caps = True
    cu.resolution_u = 10
    sp = cu.splines.new("BEZIER")
    sp.bezier_points.add(len(pts) - 1)
    for bp, p, r in zip(sp.bezier_points, pts, radii):
        bp.co = V(p)
        bp.radius = r
        bp.handle_left_type = bp.handle_right_type = "AUTO"
    cu.materials.append(MATS[mat])
    ob = bpy.data.objects.new(name, cu)
    parts_collection().objects.link(ob)
    return tag(ob, part, side)


def bake(ob):
    """Troca o objeto (curva ou mesh com modificadores) por um mesh com tudo aplicado."""
    dg = bpy.context.evaluated_depsgraph_get()
    me = bpy.data.meshes.new_from_object(ob.evaluated_get(dg))
    name, props = ob.name, {k: ob[k] for k in ob.keys()}
    old = ob.data
    bpy.data.objects.remove(ob, do_unlink=True)
    if old is not None and old.users == 0:
        (bpy.data.curves if isinstance(old, bpy.types.Curve) else bpy.data.meshes).remove(old)
    me.name = name
    new = bpy.data.objects.new(name, me)
    parts_collection().objects.link(new)
    for k, v in props.items():
        new[k] = v
    return new


# ---------------------------------------------------------------- esqueleto

HEAD_PIVOT = V((0.0, -0.02, 1.60))
HEAD_SCALE = 1.2


def hs(p):
    """posicao de um ponto da cabeca depois da escala cartoon"""
    return HEAD_PIVOT + (V(p) - HEAD_PIVOT) * HEAD_SCALE


NECK_PTS = [(0, 0.005, 1.27), (0, -0.005, 1.40), (0, -0.025, 1.52), (0, -0.02, 1.61)]
PONY_PTS = [(0, 0.02, 1.74), (0.01, 0.035, 1.83), (0.035, 0.10, 1.915), (0.06, 0.19, 1.90), (0.075, 0.24, 1.81),
            (0.07, 0.235, 1.69), (0.05, 0.20, 1.60)]

# braco esquerdo em pose A (o direito e espelhado em X)
SHOULDER_IN = V((0.03, 0.0, 1.235))
S = V((0.15, 0.0, 1.245))
E = V((0.285, 0.012, 1.095))
WR = V((0.385, -0.01, 0.955))
HIP = V((0.075, 0.0, 0.80))
KNEE = V((0.075, -0.008, 0.43))
ANKLE = V((0.075, 0.0, 0.05))
BALL = V((0.075, -0.065, 0.024))
TOE = V((0.075, -0.14, 0.018))

FINGER_NAMES = ("Thumb", "Index", "Middle", "Ring", "Pinky")


def hand_axes():
    """d: ao longo da mao; n: palma (virada para o corpo); w: largura (indicador em -Y)"""
    d = (WR - E).normalized()
    n = V((-1.0, 0.0, 0.0))
    n = (n - d * n.dot(d)).normalized()
    w = d.cross(n).normalized()
    if w.y > 0:
        w = -w
    return d, n, w


def finger_joints():
    """4 juntas por dedo (3 ossos), mao esquerda."""
    d, n, w = hand_axes()
    out = {}
    for fname, off in zip(FINGER_NAMES[1:], (0.019, 0.0065, -0.0065, -0.019)):
        base = WR + d * 0.045 + w * off
        L = 0.02 if fname in ("Middle", "Ring") else 0.018
        out[fname] = [base + d * (L * k) + n * (0.0025 * k * k) for k in range(4)]
    tb = WR + d * 0.012 + w * 0.024 + n * 0.008
    td = (d * 0.55 + w * 0.6 + n * 0.35).normalized()
    out["Thumb"] = [tb + td * (0.017 * k) + n * (0.002 * k * k) for k in range(4)]
    return out


def skeleton():
    """Lista (nome, cabeca, cauda, pai) em pose A, ja com os dois lados."""
    neck = [V(p) for p in NECK_PTS]
    head_base = neck[-1].copy()
    head_base.z = 1.60
    pony = [hs(p) for p in PONY_PTS]
    bones = [
        ("Hips", V((0.0, 0.01, 0.80)), V((0.0, 0.01, 0.95)), None),
        ("Spine", V((0.0, 0.01, 0.95)), V((0.0, 0.005, 1.06)), "Hips"),
        ("Spine1", V((0.0, 0.005, 1.06)), V((0.0, 0.0, 1.17)), "Spine"),
        ("Spine2", V((0.0, 0.0, 1.17)), neck[0], "Spine1"),
        ("Neck", neck[0], V((0.0, -0.004, 1.385)), "Spine2"),
        ("Neck1", V((0.0, -0.004, 1.385)), V((0.0, -0.02, 1.495)), "Neck"),
        ("Neck2", V((0.0, -0.02, 1.495)), head_base, "Neck1"),
        ("Head", head_base, hs((0.0, -0.02, 1.775)), "Neck2"),
        ("Jaw", hs((0.0, -0.05, 1.625)), hs((0.0, -0.17, 1.60)), "Head"),
        ("Ponytail1", pony[0], pony[2], "Head"),
        ("Ponytail2", pony[2], pony[4], "Ponytail1"),
        ("Ponytail3", pony[4], pony[6], "Ponytail2"),
    ]
    fj = finger_joints()
    d, _, _ = hand_axes()
    for side, pre in ((1, "Left"), (-1, "Right")):
        def m(v, s=side):
            return V((v.x * s, v.y, v.z))

        bones += [
            (pre + "Shoulder", m(SHOULDER_IN), m(S), "Spine2"),
            (pre + "Arm", m(S), m(E), pre + "Shoulder"),
            (pre + "ForeArm", m(E), m(WR), pre + "Arm"),
            (pre + "Hand", m(WR), m(WR + d * 0.045), pre + "ForeArm"),
        ]
        for fname in FINGER_NAMES:
            j = fj[fname]
            parent = pre + "Hand"
            for k in range(3):
                name = "%sHand%s%d" % (pre, fname, k + 1)
                bones.append((name, m(j[k]), m(j[k + 1]), parent))
                parent = name
        bones += [
            (pre + "UpLeg", m(HIP), m(KNEE), "Hips"),
            (pre + "Leg", m(KNEE), m(ANKLE), pre + "UpLeg"),
            (pre + "Foot", m(ANKLE), m(BALL), pre + "Leg"),
            (pre + "ToeBase", m(BALL), m(TOE), pre + "Foot"),
        ]
    return bones


BONE_RADIUS = {"Hips": 0.15, "Spine": 0.14, "Spine1": 0.15, "Spine2": 0.13, "Neck": 0.035, "Neck1": 0.03,
               "Neck2": 0.03, "Head": 0.10, "Jaw": 0.03, "Ponytail1": 0.04, "Ponytail2": 0.04, "Ponytail3": 0.035,
               "Shoulder": 0.05, "Arm": 0.035, "ForeArm": 0.03, "Hand": 0.028, "UpLeg": 0.045, "Leg": 0.03,
               "Foot": 0.025, "ToeBase": 0.02}
FINGER_BONES = ["Hand%s%d" % (f, k) for f in FINGER_NAMES for k in (1, 2, 3)]
PART_BONES = {
    # "*" = osso do lado do vertice (Left/Right)
    "head": ["Head"],
    "jaw": ["Jaw"],
    "mouth": ["Head", "Jaw"],  # metade de cima na cabeca, de baixo na mandibula (ver _skin_part)
    "ponytail": ["Head", "Ponytail1", "Ponytail2", "Ponytail3"],
    "neck": ["Spine2", "Neck", "Neck1", "Neck2", "Head"],
    "body": ["Hips", "Spine", "Spine1", "Spine2", "*Shoulder"],
    "skirt": ["Hips", "Spine", "*UpLeg"],
    "collar": ["Spine2", "Neck"],
    "arm": ["Spine2", "*Shoulder", "*Arm", "*ForeArm", "*Hand"],
    "hand": ["*ForeArm", "*Hand"] + ["*" + b for b in FINGER_BONES],
    "thigh": ["Hips", "*UpLeg"],
    "leg": ["*UpLeg", "*Leg", "*Foot"],
    "foot": ["*Leg", "*Foot", "*ToeBase"],
    "claw": ["*ToeBase"],
}


def _bone_radius(name):
    base = name.replace("Left", "").replace("Right", "")
    if base.startswith("Hand") and base != "Hand":
        return 0.009
    return BONE_RADIUS.get(base, 0.012)


def _seg_dist(p, a, b):
    ab = b - a
    t = max(0.0, min(1.0, (p - a).dot(ab) / ab.length_squared))
    return (a + ab * t - p).length


def _weights(p, names, segs, power=6.0):
    if len(names) == 1:
        return [(names[0], 1.0)]
    ws = []
    for n in names:
        a, b = segs[n]
        q = _seg_dist(p, a, b) / _bone_radius(n)
        ws.append((n, 1.0 / (q ** power + 1e-9)))
    ws.sort(key=lambda x: -x[1])
    ws = ws[:3]
    tot = sum(w for _, w in ws)
    ws = [(n, w / tot) for n, w in ws if w / tot > 0.02]
    tot = sum(w for _, w in ws)
    return [(n, w / tot) for n, w in ws]


def _bone_names(rule, s):
    names = []
    for r in rule:
        if r.startswith("*"):
            pres = {1: ["Left"], -1: ["Right"], 0: ["Left", "Right"]}[s]
            names += [p + r[1:] for p in pres]
        else:
            names.append(r)
    return names


SEGS = None


def segs():
    global SEGS
    if SEGS is None:
        SEGS = {n: (h, t) for n, h, t, _ in skeleton()}
    return SEGS


# ---------------------------------------------------------------- penas


class FeatherBatch:
    """Varias penas num mesh so; cada pena e rigida, com os pesos do ponto de raiz."""

    def __init__(self, name, part):
        self.name = name
        self.part = part
        self.bm = bmesh.new()
        self.mats = []
        self.groups = []  # (verts, [(osso, peso)])

    def mi(self, mat):
        if mat not in self.mats:
            self.mats.append(mat)
        return self.mats.index(mat)

    def feather(self, root, down, out, length, width, mat, tip_mat=None, lift=0.3, cup=0.25, thick=0.004,
                twist=0.0, nu=6, nv=4, rule=None):
        t = V(down).normalized()
        o = V(out)
        o = (o - t * o.dot(t)).normalized()
        s = t.cross(o)
        if twist:
            s = (s * math.cos(twist) + o * math.sin(twist)).normalized()
        top, bot = [], []
        for i in range(nu + 1):
            u = i / nu
            # contorno arredondado: largo ate ~70% e fecha numa ponta romba
            w = width * math.sin(math.pi * (0.22 + 0.78 * u ** 1.6)) ** 0.5 * (1.0 - 0.15 * u)
            c = V(root) + t * (length * u) + o * (lift * length * u * u)
            th = thick * (1.0 - 0.7 * u)
            rt, rb = [], []
            for j in range(nv + 1):
                v = -1.0 + 2.0 * j / nv
                p = c + s * (w * v) - o * (cup * w * v * v)
                rt.append(self.bm.verts.new(p))
                rb.append(self.bm.verts.new(p - o * th))
            top.append(rt)
            bot.append(rb)
        mi, ti = self.mi(mat), self.mi(tip_mat or mat)
        faces = []
        for i in range(nu):
            fm = ti if i >= nu - 2 else mi
            for j in range(nv):
                faces.append((self.bm.faces.new((top[i][j], top[i][j + 1], top[i + 1][j + 1], top[i + 1][j])), fm))
                faces.append((self.bm.faces.new((bot[i][j], bot[i + 1][j], bot[i + 1][j + 1], bot[i][j + 1])), fm))
        # costura do contorno (raiz, lados, ponta)
        loop = [(0, j) for j in range(nv + 1)] + [(i, nv) for i in range(1, nu + 1)] + \
               [(nu, j) for j in range(nv - 1, -1, -1)] + [(i, 0) for i in range(nu - 1, 0, -1)]
        rim = []
        for k in range(len(loop)):
            a, b = loop[k], loop[(k + 1) % len(loop)]
            f = self.bm.faces.new((top[b[0]][b[1]], top[a[0]][a[1]], bot[a[0]][a[1]], bot[b[0]][b[1]]))
            faces.append((f, mi))
            rim.append(f)
        for f, m in faces:
            f.material_index = m
            f.smooth = True
        # quina entre a face e a espessura fica dura (senao o smooth escurece a pena)
        for f in rim:
            f.smooth = False
            for e in f.edges:
                e.smooth = False
        verts = [v for row in top + bot for v in row]
        side = 1 if root[0] > 0.002 else (-1 if root[0] < -0.002 else 0)
        names = _bone_names(rule or PART_BONES[self.part], side)
        self.groups.append((verts, _weights(V(root), names, segs())))

    def finish(self):
        bmesh.ops.recalc_face_normals(self.bm, faces=self.bm.faces[:])
        self.bm.verts.index_update()
        groups = [([v.index for v in verts], ws) for verts, ws in self.groups]
        ob = mesh_object(self.name, self.bm, self.mats, self.part)
        for idxs, ws in groups:
            for n, w in ws:
                vg = ob.vertex_groups.get(n) or ob.vertex_groups.new(name=n)
                vg.add(idxs, w, "REPLACE")
        ob["moema_weighted"] = True
        return ob


def _pick(rng, weights):
    r = rng.random() * sum(w for _, w in weights)
    for k, w in weights:
        r -= w
        if r <= 0:
            return k
    return weights[-1][0]


FEATHER_MIX = [("Feather", 0.74), ("FeatherLight", 0.14), ("FeatherDark", 0.12)]


def feather_colors(rng):
    m = _pick(rng, FEATHER_MIX)
    tip = "FeatherTip" if (m == "Feather" and rng.random() < 0.35) else None
    return m, tip


# ---------------------------------------------------------------- corpo

BODY_RINGS = [
    (0.70, 0.205, 0.175, 0.0, 0.02),
    (0.77, 0.215, 0.18, 0.0, 0.02),
    (0.85, 0.20, 0.17, 0.0, 0.02),
    (0.93, 0.165, 0.14, 0.0, 0.01),
    (1.00, 0.142, 0.118, 0.0, 0.0),
    (1.08, 0.152, 0.122, 0.0, -0.005),
    (1.16, 0.162, 0.128, 0.0, -0.008),
    (1.23, 0.155, 0.118, 0.0, 0.0),
    (1.28, 0.125, 0.095, 0.0, 0.01),
    (1.315, 0.05, 0.048, 0.0, 0.005),
]


def body_profile(z):
    rs = BODY_RINGS
    if z <= rs[0][0]:
        return rs[0][1:]
    for a, b in zip(rs, rs[1:]):
        if a[0] <= z <= b[0]:
            t = (z - a[0]) / (b[0] - a[0])
            return tuple(x + (y - x) * t for x, y in zip(a[1:], b[1:]))
    return rs[-1][1:]


def body_point(z, t):
    rx, ry, cx, cy = body_profile(z)
    return V((cx + math.sin(t) * rx, cy - math.cos(t) * ry, z))


def body_frame(z, t):
    p = body_point(z, t)
    dz = (body_point(z + 0.005, t) - body_point(z - 0.005, t)).normalized()
    dt = (body_point(z, t + 0.01) - body_point(z, t - 0.01)).normalized()
    n = dt.cross(dz).normalized()
    if n.dot(p - V((0, 0, z))) < 0:
        n = -n
    return p, -dz, n


def build_body():
    def hem(i, j, co, seg=56, teeth=14, depth=0.07):
        if i == 0:
            k = (j % (seg // teeth)) / (seg // teeth)
            tri = 1 - abs(2 * k - 1)
            co.z += depth * tri
        return co
    return lathe("Body", BODY_RINGS, ["Feather"], "body", seg=56, mod=hem, subsurf=1)


def build_body_feathers():
    rng = random.Random(7)
    fb = FeatherBatch("BodyFeathers", "body")
    rows = [  # (z da raiz, comprimento, largura, levantamento) -- deitadas em cima, armadas na barra
        (1.250, 0.080, 0.055, 0.05), (1.190, 0.090, 0.060, 0.05), (1.120, 0.095, 0.062, 0.06),
        (1.050, 0.100, 0.064, 0.07), (0.980, 0.105, 0.066, 0.10), (0.910, 0.120, 0.068, 0.16),
        (0.850, 0.140, 0.066, 0.28), (0.800, 0.160, 0.062, 0.42),
    ]
    for r, (z, L, W, lift) in enumerate(rows):
        rx, ry, _, _ = body_profile(z)
        circ = math.pi * (3 * (rx + ry) - math.sqrt((3 * rx + ry) * (rx + 3 * ry)))
        n = max(8, int(circ / (W * 0.62)))
        rule = PART_BONES["skirt"] if z < 0.9 else None
        for k in range(n):
            t = 2 * math.pi * (k + 0.5 * (r % 2) + rng.uniform(-0.15, 0.15)) / n
            p, down, out = body_frame(z + rng.uniform(-0.018, 0.018), t)
            # nos ombros as penas saem por baixo do braco: puxa um pouco para fora
            m, tip = feather_colors(rng)
            fb.feather(p + out * 0.002, down + out * 0.08, out, L * rng.uniform(0.85, 1.2),
                       W * rng.uniform(0.9, 1.1), m, tip, lift=lift * rng.uniform(0.85, 1.2),
                       cup=0.12, twist=rng.uniform(-0.12, 0.12), rule=rule)
    return fb.finish()


def build_collar():
    rng = random.Random(11)
    fb = FeatherBatch("CollarFeathers", "collar")
    # fileiras sobre a cupula dos ombros, cobrindo a base do pescoco
    for z, L, W, n in ((1.306, 0.06, 0.04, 12), (1.29, 0.075, 0.05, 16), (1.27, 0.08, 0.055, 20)):
        for k in range(n):
            t = 2 * math.pi * (k + 0.5 * (n % 2) + rng.uniform(-0.2, 0.2)) / n
            p, down, out = body_frame(z, t)
            fb.feather(p + out * 0.003, down, out, L * rng.uniform(0.9, 1.1), W,
                       "FeatherLight" if rng.random() < 0.3 else "Feather", "FeatherTip", lift=0.12, cup=0.12,
                       twist=rng.uniform(-0.15, 0.15))
    return fb.finish()


# ---------------------------------------------------------------- pescoco e cabeca


def build_neck():
    return tube("Neck", NECK_PTS, [0.04, 0.03, 0.028, 0.034], "Skin", "neck", res=8)


def build_head():
    objs = [sphere("Head", (0, -0.02, 1.675), (0.074, 0.084, 0.086), "Skin", "head", subsurf=1),
            sphere("Cheek", (0, -0.06, 1.645), (0.068, 0.06, 0.05), "Skin", "head", subsurf=1)]

    def smile(o, k):
        for v in o.data.vertices:
            v.co.z += k * v.co.x * v.co.x

    ub = sphere("BeakUpper", (0, -0.128, 1.648), (0.056, 0.088, 0.024), "Beak", "head", rot=(7, 0, 0), subsurf=1)
    smile(ub, 5.0)
    lb = sphere("BeakLower", (0, -0.11, 1.606), (0.047, 0.07, 0.017), "Beak", "jaw", rot=(-17, 0, 0), subsurf=1)
    smile(lb, 7.5)
    mo = sphere("MouthInside", (0, -0.112, 1.624), (0.046, 0.07, 0.026), "Mouth", "mouth", rot=(-6, 0, 0),
                subsurf=1)
    smile(mo, 6.0)
    objs += [ub, lb, mo, sphere("Tongue", (0, -0.118, 1.616), (0.024, 0.04, 0.009), "Tongue", "jaw",
                                rot=(-10, 0, 0))]
    for s in (1, -1):
        objs.append(sphere("Nostril", (s * 0.012, -0.195, 1.664), (0.005, 0.009, 0.003), "Lash", "head",
                           seg=12, rings=6))

    ew = (0.036, 0.026, 0.043)
    for s in (1, -1):
        c = V((s * 0.037, -0.078, 1.705))
        objs += [
            sphere("EyeWhite", c, ew, "EyeWhite", "head", rot=(0, 0, s * -10)),
            sphere("Iris", (c.x - s * 0.003, -0.098, c.z - 0.004), (0.022, 0.009, 0.027), "Iris", "head"),
            sphere("Pupil", (c.x - s * 0.003, -0.104, c.z - 0.004), (0.012, 0.006, 0.016), "Pupil", "head"),
            sphere("Glint", (c.x - s * 0.009, -0.109, c.z + 0.008), (0.006, 0.003, 0.007), "EyeWhite", "head",
                   seg=12, rings=6),
        ]
        # contorno da palpebra superior
        pts = []
        for a in range(12, 172, 20):
            ar = math.radians(a)
            dx, dz = math.cos(ar) * ew[0] * 1.02, math.sin(ar) * ew[2] * 1.02
            y = c.y - ew[1] * math.sqrt(max(0.0, 1 - (dx / (ew[0] * 1.08)) ** 2 - (dz / (ew[2] * 1.08)) ** 2))
            pts.append((c.x + dx, y - 0.002, c.z + dz))
        objs.append(tube("LidLine", pts, [0.0035] * len(pts), "Lash", "head", res=3))
        for k in range(3):
            a = math.radians(30 + k * 18) if s > 0 else math.radians(150 - k * 18)
            bx, bz = c.x + math.cos(a) * ew[0], c.z + math.sin(a) * ew[2]
            objs.append(tube("Lash", [(bx, c.y - 0.012, bz), (bx + s * 0.012, c.y - 0.008, bz + 0.014),
                                      (bx + s * 0.02, c.y - 0.002, bz + 0.016)], [0.003, 0.0022, 0.0008],
                             "Lash", "head", res=2))
        objs.append(tube("Brow", [(s * 0.016, -0.094, 1.758), (s * 0.04, -0.088, 1.768), (s * 0.062, -0.07, 1.762)],
                         [0.003, 0.0045, 0.002], "Lash", "head", res=3))

    # cabelo
    objs += [
        sphere("HairBun", (0, 0.02, 1.748), (0.04, 0.045, 0.026), "Hair", "head", subsurf=1),
        torus("HairTie", (0, 0.03, 1.765), 0.024, 0.009, "HairTie", "head", rot=(-25, 0, 0)),
        tube("Ponytail", PONY_PTS, [0.026, 0.038, 0.05, 0.05, 0.042, 0.026, 0.006], "Hair", "ponytail", res=8),
        tube("PonytailStrand", [(0, 0.03, 1.76), (-0.01, 0.06, 1.86), (0.0, 0.13, 1.90), (0.02, 0.2, 1.85),
                                (0.03, 0.215, 1.76), (0.015, 0.19, 1.71)],
             [0.02, 0.03, 0.032, 0.026, 0.016, 0.004], "Hair", "ponytail", res=6),
        tube("PonytailStreak", [(0.012, 0.03, 1.85), (0.036, 0.095, 1.94), (0.062, 0.18, 1.925),
                                (0.078, 0.235, 1.85)], [0.008, 0.02, 0.018, 0.004], "HairLight", "ponytail"),
    ]
    return objs


def scale_head(objs):
    m = Matrix.Translation(HEAD_PIVOT) @ Matrix.Scale(HEAD_SCALE, 4) @ Matrix.Translation(-HEAD_PIVOT)
    for ob in objs:
        if ob.type == "MESH":
            ob.data.transform(m)
        else:
            for sp in ob.data.splines:
                for bp in sp.bezier_points:
                    bp.co = m @ bp.co
                    bp.handle_left = m @ bp.handle_left
                    bp.handle_right = m @ bp.handle_right
                    bp.radius *= HEAD_SCALE


# ---------------------------------------------------------------- bracos


def build_arm(side):
    s = side

    def m(v):
        return V((v.x * s, v.y, v.z))

    pre = "L" if s > 0 else "R"
    elbow_mid = (S + E) * 0.5 + V((0.006, 0.0, 0.0))
    objs = [
        sphere("Shoulder" + pre, m(S), (0.04, 0.038, 0.04), "Feather", "arm", side, subsurf=1),
        tube("Arm" + pre, [m(S), m(elbow_mid), m(E), m(WR)], [0.036, 0.033, 0.03, 0.026], "Feather", "arm",
             side),
    ]
    # mao: palma + dedos seguindo as juntas do esqueleto
    d, n, w = hand_axes()
    palm_c = WR + d * 0.026
    bm = bmesh.new()
    bmesh.ops.create_uvsphere(bm, u_segments=20, v_segments=10, radius=1.0)
    rot = Matrix((w, n, d)).transposed().to_4x4()
    bmesh.ops.transform(bm, matrix=Matrix.Translation(palm_c) @ rot @ Matrix.Diagonal((0.03, 0.017, 0.032, 1)),
                        verts=bm.verts)
    if s < 0:
        for v in bm.verts:
            v.co.x = -v.co.x
        bmesh.ops.reverse_faces(bm, faces=bm.faces[:])
    objs.append(mesh_object("Palm" + pre, bm, ["Feather"], "hand", side, subsurf=1))
    for fname, j in finger_joints().items():
        r = 0.0085 if fname != "Thumb" else 0.0095
        objs.append(tube("Finger" + fname + pre, [m(p) for p in j], [r, r * 0.95, r * 0.9, r * 0.8], "Feather",
                         "hand", side, res=4))
    return objs


def build_arm_feathers():
    rng = random.Random(23)
    fb = FeatherBatch("ArmFeathers", "arm")
    for s in (1, -1):
        def m(v):
            return V((v.x * s, v.y, v.z))

        for a, b, rad, L, W, ns in ((S, E, 0.034, 0.08, 0.045, 3), (E, WR, 0.028, 0.07, 0.04, 3)):
            a, b = m(a), m(b)
            ax = (b - a).normalized()
            ref = V((0, 0, 1)) if abs(ax.z) < 0.9 else V((1, 0, 0))
            e1 = ax.cross(ref).normalized()
            e2 = ax.cross(e1).normalized()
            for i in range(ns):
                u = 0.12 + 0.3 * i
                c = a + (b - a) * u
                for k in range(6):
                    ang = 2 * math.pi * (k + 0.5 * (i % 2)) / 6
                    out = e1 * math.cos(ang) + e2 * math.sin(ang)
                    mat, tip = feather_colors(rng)
                    fb.feather(c + out * rad, ax, out, L * rng.uniform(0.9, 1.1), W, mat, tip,
                               lift=0.06, cup=0.15, twist=rng.uniform(-0.1, 0.1))
        # franja de asa: penas longas penduradas atras do antebraco
        a, b = m(E), m(WR)
        for i in range(5):
            u = 0.1 + 0.19 * i
            c = a + (b - a) * u + V((0.0, 0.022, -0.006))
            down = (V((0, 0.35, -1.0)) + (b - a).normalized() * 0.5).normalized()
            out = V((s * 0.9, 0.4, 0.0))
            mat, tip = feather_colors(rng)
            fb.feather(c, down, out, 0.09 + 0.012 * i, 0.034, mat, tip or "FeatherTip", lift=0.25,
                       twist=rng.uniform(-0.15, 0.15))
    return fb.finish()


# ---------------------------------------------------------------- pernas e pes


def leg_rings():
    prof = [(0.045, 0.024), (0.10, 0.023), (0.25, 0.024), (0.40, 0.031), (0.44, 0.032),
            (0.55, 0.029), (0.66, 0.034), (0.74, 0.045), (0.80, 0.05)]

    def r_at(z):
        for (z0, r0), (z1, r1) in zip(prof, prof[1:]):
            if z0 <= z <= z1:
                return r0 + (r1 - r0) * (z - z0) / (z1 - z0)
        return prof[-1][1]

    rings, mats = [(0.045, r_at(0.045), r_at(0.045))], [0]
    grooves = [0.09 + i * 0.048 for i in range(7)] + [0.48 + i * 0.05 for i in range(5)]
    for zg in grooves:
        for dz, k, mi in ((-0.0025, 1.0, 1), (0.0, 0.92, 1), (0.0025, 1.0, 0)):
            rr = r_at(zg + dz) * k
            rings.append((zg + dz, rr, rr))
            mats.append(mi)
    for zz in (0.74, 0.80):
        rings.append((zz, r_at(zz), r_at(zz)))
        mats.append(0)
    return rings, mats[:-1]


def build_leg(side):
    x = side * HIP.x
    pre = "L" if side > 0 else "R"
    rings, mats = leg_rings()
    objs = [lathe("Leg" + pre, [(z, rx, ry, x, 0.0) for z, rx, ry in rings], ["Leg", "LegBand"], "leg", side,
                  seg=24, ring_mats=mats, subsurf=0),
            sphere("Ankle" + pre, (x, 0.0, 0.05), (0.03, 0.03, 0.028), "Leg", "foot", side, seg=16, rings=8)]
    for k, ang in enumerate((-26, 0, 26)):
        a = math.radians(ang + side * 4)
        L = 0.135 if ang == 0 else 0.11
        dx, dy = math.sin(a) * L, -math.cos(a) * L
        objs.append(tube("Toe" + pre, [(x, 0.0, 0.045), (x + dx * 0.5, dy * 0.5, 0.026), (x + dx, dy, 0.018)],
                         [0.02, 0.016, 0.012], "Leg", "foot", side))
        objs.append(cone("Claw" + pre, (x + dx * 1.12, dy * 1.12, 0.016), 0.03, 0.011, "Claw", "claw", side,
                         rot=(-90, 0, ang + side * 4), flat=0.8, seg=10))
    return objs


def build_thigh_fluff():
    rng = random.Random(31)
    fb = FeatherBatch("ThighFeathers", "thigh")
    for s in (1, -1):
        x = s * HIP.x
        for z, rad, L, W, n in ((0.72, 0.040, 0.085, 0.04, 9), (0.675, 0.034, 0.075, 0.036, 9)):
            for k in range(n):
                t = 2 * math.pi * (k + rng.uniform(-0.2, 0.2) + (0.5 if z < 0.7 else 0)) / n
                out = V((math.sin(t), -math.cos(t), 0.0))
                p = V((x, 0.0, z)) + out * rad
                mat, tip = feather_colors(rng)
                fb.feather(p, V((out.x * 0.25, out.y * 0.25, -1.0)), out, L * rng.uniform(0.9, 1.1), W, mat, tip,
                           lift=0.35)
    return fb.finish()


# ---------------------------------------------------------------- montagem


def clear():
    for name in (PARTS_COLL_NAME, COLL_NAME):
        c = bpy.data.collections.get(name)
        if c is None:
            continue
        for ob in list(c.objects):
            data = ob.data
            bpy.data.objects.remove(ob, do_unlink=True)
            if data is not None and data.users == 0:
                if isinstance(data, bpy.types.Mesh):
                    bpy.data.meshes.remove(data)
                elif isinstance(data, bpy.types.Armature):
                    bpy.data.armatures.remove(data)
                elif isinstance(data, bpy.types.Curve):
                    bpy.data.curves.remove(data)
        bpy.data.collections.remove(c)
    for me in list(bpy.data.meshes):
        if me.users == 0:
            bpy.data.meshes.remove(me)


def build():
    global SEGS
    SEGS = None
    clear()
    build_materials()
    head = build_head()
    scale_head(head)
    objs = [build_body(), build_neck()] + head
    for side in (1, -1):
        objs += build_arm(side) + build_leg(side)
    objs += [build_body_feathers(), build_collar(), build_arm_feathers(), build_thigh_fluff()]
    bpy.context.view_layer.update()
    return [bake(o) if (o.type != "MESH" or o.modifiers) else o for o in objs]


# ---------------------------------------------------------------- rig


def _skin_part(ob, sg):
    if ob.get("moema_weighted"):
        return
    rule = PART_BONES[ob["moema_part"]]
    side = ob["moema_side"]
    groups = {}
    if ob["moema_part"] == "mouth":
        zs = [v.co.z for v in ob.data.vertices]
        zc, half = (max(zs) + min(zs)) * 0.5, (max(zs) - min(zs)) * 0.5
        for v in ob.data.vertices:
            wj = min(1.0, max(0.0, 0.5 - (v.co.z - zc) / half))
            groups.setdefault("Jaw", []).append((v.index, wj))
            groups.setdefault("Head", []).append((v.index, 1.0 - wj))
        rule = []
    for v in (ob.data.vertices if rule else ()):
        s = side if side else (1 if v.co.x > 0.002 else (-1 if v.co.x < -0.002 else 0))
        for n, w in _weights(v.co, _bone_names(rule, s), sg):
            groups.setdefault(n, []).append((v.index, w))
    for n, items in groups.items():
        vg = ob.vertex_groups.get(n) or ob.vertex_groups.new(name=n)
        for idx, w in items:
            vg.add([idx], w, "REPLACE")


def _select_only(objs, active):
    for o in bpy.context.view_layer.objects:
        o.select_set(False)
    for o in objs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = active


def rig(parts):
    """Calcula pesos, junta tudo num unico mesh e cria a armature."""
    bones = skeleton()
    sg = segs()
    for ob in parts:
        _skin_part(ob, sg)

    main = collection()
    arm_data = bpy.data.armatures.new("MoemaRig")
    arm = bpy.data.objects.new("MoemaRig", arm_data)
    main.objects.link(arm)
    arm.show_in_front = True
    _select_only([arm], arm)
    bpy.ops.object.mode_set(mode="EDIT")
    for name, h, t, parent in bones:
        eb = arm_data.edit_bones.new(name)
        eb.head, eb.tail = h, t
        eb.roll = 0.0
        if parent:
            eb.parent = arm_data.edit_bones[parent]
            eb.use_connect = (eb.parent.tail - h).length < 1e-5
    bpy.ops.object.mode_set(mode="OBJECT")

    target = next(o for o in parts if o.name == "Body")
    _select_only(parts, target)
    bpy.ops.object.join()
    body = target
    body.name = "Moema"
    body.data.name = "Moema"
    for c in list(body.users_collection):
        c.objects.unlink(body)
    main.objects.link(body)
    for k in ("moema_part", "moema_side", "moema_weighted"):
        if k in body:
            del body[k]
    body.parent = arm
    md = body.modifiers.new("Armature", "ARMATURE")
    md.object = arm
    pc = bpy.data.collections.get(PARTS_COLL_NAME)
    if pc is not None and not pc.objects:
        bpy.data.collections.remove(pc)
    _select_only([arm], arm)
    return arm, body


def export_fbx(path, objs):
    _select_only(objs, objs[0])
    bpy.ops.export_scene.fbx(
        filepath=path, use_selection=True, object_types={"ARMATURE", "MESH"},
        apply_unit_scale=True, apply_scale_options="FBX_SCALE_ALL", axis_forward="-Z", axis_up="Y",
        use_mesh_modifiers=True, mesh_smooth_type="FACE", add_leaf_bones=False,
        primary_bone_axis="Y", secondary_bone_axis="X", armature_nodetype="NULL",
        use_armature_deform_only=True, bake_anim=False, path_mode="AUTO")


# ---------------------------------------------------------------- linha de comando


def main():
    """blender -b --factory-startup --python build_moema.py
    Gera Source~/Moema.blend e ../Moema.fbx (pasta do personagem no Unity)."""
    import os
    here = os.path.dirname(os.path.abspath(__file__))
    cube = bpy.data.objects.get("Cube")
    if cube is not None:
        bpy.data.objects.remove(cube, do_unlink=True)
    arm, body = rig(build())
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(here, "Moema.blend"))
    export_fbx(os.path.join(os.path.dirname(here), "Moema.fbx"), [arm, body])


if __name__ == "__main__":
    main()
