# -*- coding: utf-8 -*-
"""Modelo 3D procedural do protagonista do Dreamscape.

Gera no Blender (5.x) o estudante da Unifor dos storyboards (docs/storybook):
moletom lilas com capuz e cordoes brancos, camiseta branca por baixo, jeans
azul-acinzentado, tenis de lona escuro com sola branca, mochila bordo, relogio
digital preto no pulso esquerdo, cabelo castanho-escuro baguncado e olhos castanhos.

Convencoes: Z para cima, personagem olhando para -Y, lado esquerdo do personagem
em +X, pes no chao (Z = 0), unidades em metros (altura ~1,76 m).

Uso dentro do Blender:
    import sys; sys.path.insert(0, "<pasta deste arquivo>")
    import build_protagonist as bp
    bp.build()      # gera as pecas
    bp.rig()        # junta as pecas, cria o esqueleto (nomes estilo Mixamo) e os pesos
    bp.export_fbx("<caminho>.fbx")
"""
import math
import random

import bmesh
import bpy
from mathutils import Matrix, Vector
from mathutils.bvhtree import BVHTree

V = Vector
COLL_NAME = "Protagonist"
PARTS_COLL_NAME = "Protagonist_Parts"

# ---------------------------------------------------------------- utilidades


def smoothstep(a, b, x):
    t = max(0.0, min(1.0, (x - a) / (b - a)))
    return t * t * (3.0 - 2.0 * t)


def lerp(a, b, t):
    return a + (b - a) * t


def interp(table, x):
    """Interpolacao linear numa tabela ordenada [(x, y), ...]."""
    if x <= table[0][0]:
        return table[0][1]
    for (x0, y0), (x1, y1) in zip(table, table[1:]):
        if x <= x1:
            return lerp(y0, y1, (x - x0) / (x1 - x0))
    return table[-1][1]


def catmull(points, per_seg=4):
    """Amostra uma spline Catmull-Rom que passa pelos pontos (Vector 2D ou 3D)."""
    pts = [points[0] + (points[0] - points[1])] + list(points) + [points[-1] + (points[-1] - points[-2])]
    out = []
    for i in range(1, len(pts) - 2):
        p0, p1, p2, p3 = pts[i - 1], pts[i], pts[i + 1], pts[i + 2]
        for k in range(per_seg):
            t = k / per_seg
            t2, t3 = t * t, t * t * t
            out.append(0.5 * ((2 * p1) + (-p0 + p2) * t + (2 * p0 - 5 * p1 + 4 * p2 - p3) * t2
                              + (-p0 + 3 * p1 - 3 * p2 + p3) * t3))
    out.append(points[-1].copy())
    return out


# ---------------------------------------------------------------- materiais

PALETTE = {
    # nome: (cor sRGB, rugosidade, metalico)
    "Skin": ("#EDBE9C", 0.55, 0.0),
    "SkinShade": ("#C98A70", 0.6, 0.0),
    "Lips": ("#8A4A40", 0.5, 0.0),
    "Hair": ("#3A251C", 0.45, 0.0),
    "HairLight": ("#4B3125", 0.45, 0.0),
    "Brow": ("#2A1A14", 0.6, 0.0),
    "EyeWhite": ("#F6F3F1", 0.3, 0.0),
    "Iris": ("#6A4128", 0.25, 0.0),
    "Pupil": ("#140C09", 0.2, 0.0),
    "EyeLine": ("#22150F", 0.6, 0.0),
    "Highlight": ("#FFFFFF", 0.2, 0.0),
    "Hoodie": ("#BEA7DF", 0.85, 0.0),
    "HoodieRib": ("#A792C9", 0.9, 0.0),
    "Drawstring": ("#F1EFF4", 0.7, 0.0),
    "Metal": ("#B9BCC4", 0.35, 0.8),
    "Tee": ("#ECEAF0", 0.85, 0.0),
    "Jeans": ("#4A5577", 0.85, 0.0),
    "ShoeCanvas": ("#1F2234", 0.85, 0.0),
    "ShoeRubber": ("#EEEBE6", 0.6, 0.0),
    "Laces": ("#F5F5F3", 0.7, 0.0),
    "Backpack": ("#6E1F35", 0.7, 0.0),
    "BackpackDark": ("#4E1424", 0.75, 0.0),
    "Zipper": ("#2B2B30", 0.4, 0.6),
    "WatchBand": ("#1B1B1F", 0.5, 0.0),
    "WatchScreen": ("#0C0E12", 0.15, 0.0),
    "NotebookCover": ("#283253", 0.6, 0.0),
    "NotebookPages": ("#F3F0E6", 0.8, 0.0),
}
MATS = {}


def _srgb_to_linear(c):
    return c / 12.92 if c <= 0.04045 else ((c + 0.055) / 1.055) ** 2.4


def hex_rgba(h):
    h = h.lstrip("#")
    return tuple(_srgb_to_linear(int(h[i:i + 2], 16) / 255.0) for i in (0, 2, 4)) + (1.0,)


def build_materials():
    for name, (hexcol, rough, metal) in PALETTE.items():
        mat_name = "M_" + name
        mat = bpy.data.materials.get(mat_name) or bpy.data.materials.new(mat_name)
        if mat.node_tree is None:
            mat.use_nodes = True
        nt = mat.node_tree
        bsdf = next((n for n in nt.nodes if n.type == "BSDF_PRINCIPLED"), None)
        if bsdf is None:
            bsdf = nt.nodes.new("ShaderNodeBsdfPrincipled")
            out = next((n for n in nt.nodes if n.type == "OUTPUT_MATERIAL"), None)
            if out is None:
                out = nt.nodes.new("ShaderNodeOutputMaterial")
            nt.links.new(bsdf.outputs[0], out.inputs[0])
        col = hex_rgba(hexcol)
        bsdf.inputs["Base Color"].default_value = col
        bsdf.inputs["Roughness"].default_value = rough
        bsdf.inputs["Metallic"].default_value = metal
        mat.diffuse_color = col
        mat.roughness = rough
        mat.metallic = metal
        MATS[name] = mat


# ---------------------------------------------------------------- malha


def superellipse(a, n):
    c, s = math.cos(a), math.sin(a)
    e = 2.0 / n
    return math.copysign(abs(c) ** e, c), math.copysign(abs(s) ** e, s)


def ring(center, ax, ay, rx, ry, seg, n=2.0, ry_neg=None):
    """Anel (superelipse) no plano (ax, ay). ry_neg permite profundidade diferente no lado -ay."""
    pts = []
    for i in range(seg):
        u, v = superellipse(2.0 * math.pi * i / seg, n)
        r_y = ry if (v >= 0.0 or ry_neg is None) else ry_neg
        pts.append(center + ax * (rx * u) + ay * (r_y * v))
    return pts


def frames_along(points, hint):
    """Referenciais (t, ax, ay) ao longo de um caminho; ay segue a direcao 'hint'."""
    out = []
    n = len(points)
    prev = None
    for i in range(n):
        if i == 0:
            t = points[1] - points[0]
        elif i == n - 1:
            t = points[-1] - points[-2]
        else:
            t = points[i + 1] - points[i - 1]
        t = t.normalized()
        h = hint(i, points[i], t) if callable(hint) else hint
        ay = h - t * h.dot(t)
        if ay.length < 1e-6:
            ay = prev.copy() if prev is not None else t.orthogonal()
        ay.normalize()
        ax = ay.cross(t)
        out.append((t, ax, ay))
        prev = ay
    return out


def tube_rings(points, radii, seg, hint, n=2.0):
    fr = frames_along(points, hint)
    return [ring(p, ax, ay, r[0], r[1], seg, n) for p, r, (t, ax, ay) in zip(points, radii, fr)]


class MeshBuilder:
    def __init__(self):
        self.bm = bmesh.new()
        self.mat_names = []

    def mat_index(self, name):
        if name not in self.mat_names:
            self.mat_names.append(name)
        return self.mat_names.index(name)

    def loft(self, rings, mat, caps=(True, True), bulge=(0.0, 0.0), band_mat=None):
        """Liga aneis consecutivos com quads; fecha as pontas com leque (bulge > 0 abaula para fora)."""
        bm = self.bm
        vr = [[bm.verts.new(p) for p in r] for r in rings]
        n = len(rings[0])
        mi = self.mat_index(mat)
        for k, (a, b) in enumerate(zip(vr, vr[1:])):
            m = mi if band_mat is None else self.mat_index(band_mat(k))
            for i in range(n):
                j = (i + 1) % n
                bm.faces.new((a[i], a[j], b[j], b[i])).material_index = m

        def cap(rv, rpts, rpts_prev, b):
            cen = sum(rpts, V()) / n
            out = (cen - sum(rpts_prev, V()) / n).normalized()
            c = bm.verts.new(cen + out * b)
            for i in range(n):
                bm.faces.new((c, rv[(i + 1) % n], rv[i])).material_index = mi

        if caps[0]:
            cap(vr[0], rings[0], rings[1], bulge[0])
        if caps[1]:
            cap(vr[-1], rings[-1], rings[-2], bulge[1])

    def add_mesh(self, other):
        """Copia a geometria de outro MeshBuilder (remapeando materiais)."""
        remap = {i: self.mat_index(name) for i, name in enumerate(other.mat_names)}
        vmap = {}
        for v in other.bm.verts:
            vmap[v] = self.bm.verts.new(v.co)
        for f in other.bm.faces:
            nf = self.bm.faces.new([vmap[v] for v in f.verts])
            nf.material_index = remap.get(f.material_index, 0)
        other.bm.free()


def collection(name=COLL_NAME, parent=None):
    c = bpy.data.collections.get(name)
    if c is None:
        c = bpy.data.collections.new(name)
        (parent or bpy.context.scene.collection).children.link(c)
    return c


def parts_collection():
    return collection(PARTS_COLL_NAME, collection())


def to_object(name, mb, part, side=0, closed=True, smooth=True, subsurf=1):
    bm = mb.bm
    if closed:
        bmesh.ops.recalc_face_normals(bm, faces=bm.faces[:])
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me)
    bm.free()
    for mn in mb.mat_names:
        me.materials.append(MATS[mn])
    ob = bpy.data.objects.new(name, me)
    parts_collection().objects.link(ob)
    if smooth:
        for p in me.polygons:
            p.use_smooth = True
    if subsurf:
        md = ob.modifiers.new("Subdivision", "SUBSURF")
        md.levels = subsurf
        md.render_levels = subsurf
    ob["dreamscape_part"] = part
    ob["dreamscape_side"] = side
    return ob


def mirror_object(src, name, side=-1):
    """Copia espelhada em X (mantendo normais para fora)."""
    me = src.data.copy()
    me.name = name
    bm = bmesh.new()
    bm.from_mesh(me)
    for v in bm.verts:
        v.co.x = -v.co.x
    bmesh.ops.reverse_faces(bm, faces=bm.faces[:])
    bm.to_mesh(me)
    bm.free()
    ob = bpy.data.objects.new(name, me)
    parts_collection().objects.link(ob)
    for m in src.modifiers:
        if m.type == "SUBSURF":
            md = ob.modifiers.new(m.name, "SUBSURF")
            md.levels = m.levels
            md.render_levels = m.render_levels
    ob["dreamscape_part"] = src["dreamscape_part"]
    ob["dreamscape_side"] = side
    return ob


class Surface:
    """Raycast/nearest sobre a malha avaliada (com modificadores) de varios objetos."""

    def __init__(self, objs):
        bpy.context.view_layer.update()
        dg = bpy.context.evaluated_depsgraph_get()
        self.trees = [BVHTree.FromObject(o, dg) for o in objs]

    def ray(self, origin, direction, dist=10.0):
        best = (None, None, None, None)
        for t in self.trees:
            hit = t.ray_cast(origin, direction, dist)
            if hit[0] is not None and (best[0] is None or hit[3] < best[3]):
                best = hit
        return best

    def nearest(self, p):
        best = (None, None, None, None)
        for t in self.trees:
            hit = t.find_nearest(p)
            if hit[0] is not None and (best[0] is None or hit[3] < best[3]):
                best = hit
        return best


# ---------------------------------------------------------------- proporcoes / esqueleto

A_POSE_DEG = 45.0
_a = math.radians(A_POSE_DEG)
S = V((0.182, 0.006, 1.398))                       # ombro (glenoumeral) esquerdo
A = V((math.cos(_a), 0.0, -math.sin(_a)))          # direcao do braco
E = S + A * 0.280                                  # cotovelo
A2 = (A + V((0.0, -0.06, 0.0))).normalized()       # antebraco levemente para frente
WR = E + A2 * 0.250                                # pulso

HIP = V((0.090, 0.000, 0.905))
KNEE = V((0.097, -0.010, 0.490))
ANKLE = V((0.104, 0.016, 0.088))
BALL = V((0.106, -0.125, 0.028))
TOE = V((0.106, -0.200, 0.028))

HEAD_C = V((0.0, 0.004, 1.630))
HEAD_W, HEAD_D, HEAD_H = 0.080, 0.098, 0.114
HEAD_SCALE = 1.06                                  # cabeca um pouco maior (estilo anime dos storyboards)
HEAD_PIVOT = V((0.0, 0.012, 1.500))
HEAD_DROP = 0.012                                  # encurta o pescoco visivel

# ---------------------------------------------------------------- cabeca


def head_shape(u):
    x, y, z = u
    lo = smoothstep(0.0, 1.0, -z)                  # 0 no cranio, 1 no queixo
    X = x * HEAD_W * (1.0 - 0.30 * lo ** 1.6)      # mandibula larga, queixo arredondado
    if y < 0.0:
        Y = y * HEAD_D * 0.93
    else:
        Y = y * HEAD_D * (1.06 - 0.30 * smoothstep(0.1, 1.0, -z))
    Y -= 0.014 * lo ** 1.5                         # queixo para frente
    Z = z * HEAD_H * (0.90 if z < 0.0 else 1.0)
    # macas do rosto levemente cheias
    if y < 0.0:
        X *= 1.0 + 0.04 * math.exp(-((z + 0.35) / 0.25) ** 2)
    return HEAD_C + V((X, Y, Z))


def build_head():
    mb = MeshBuilder()
    bmesh.ops.create_uvsphere(mb.bm, u_segments=32, v_segments=20, radius=1.0)
    mi = mb.mat_index("Skin")
    for v in mb.bm.verts:
        v.co = head_shape(v.co.normalized())
    for f in mb.bm.faces:
        f.material_index = mi
    return to_object("Head", mb, "head")


def build_neck():
    mb = MeshBuilder()
    rows = [(1.400, 0.060, 0.062, 0.014), (1.470, 0.055, 0.056, 0.016),
            (1.530, 0.050, 0.051, 0.014), (1.575, 0.046, 0.047, 0.014)]
    rings = [ring(V((0.0, cy, z)), V((1, 0, 0)), V((0, 1, 0)), rx, ry, 16) for z, rx, ry, cy in rows]
    mb.loft(rings, "Skin")
    return to_object("Neck", mb, "neck")


def build_ears(head):
    surf = Surface([head])
    mb = MeshBuilder()
    loc, nor, _, _ = surf.ray(V((0.4, 0.010, 1.617)), V((-1, 0, 0)))
    base = loc + V((0.004, 0.0, 0.0))
    import mathutils
    rot = (mathutils.Matrix.Rotation(math.radians(-18.0), 3, "Z")
           @ mathutils.Matrix.Rotation(math.radians(-12.0), 3, "X"))
    for mat, scale, offset in (("Skin", (0.0075, 0.017, 0.029), V((0.0, 0.0, 0.0))),
                               ("SkinShade", (0.0045, 0.010, 0.019), V((0.004, -0.002, 0.002)))):
        part = MeshBuilder()
        bmesh.ops.create_uvsphere(part.bm, u_segments=10, v_segments=6, radius=1.0)
        mi = part.mat_index(mat)
        for v in part.bm.verts:
            c = V((v.co.x * scale[0], v.co.y * scale[1], v.co.z * scale[2]))
            v.co = base + rot @ (c + offset)
        for f in part.bm.faces:
            f.material_index = mi
        mb.add_mesh(part)
    ob = to_object("Ear.L", mb, "head", side=1)
    return [ob, mirror_object(ob, "Ear.R")]


def build_nose(head):
    surf = Surface([head])
    pts, radii = [], []
    for z, protrude, r in ((1.616, -0.004, (0.0045, 0.003)), (1.607, 0.002, (0.0048, 0.0040)),
                           (1.5985, 0.0068, (0.0058, 0.0048)), (1.5925, 0.0058, (0.0072, 0.0040)),
                           (1.5890, 0.0005, (0.0055, 0.0030))):
        loc, nor, _, _ = surf.ray(V((0.0, -0.5, z)), V((0, 1, 0)))
        pts.append(loc + V((0.0, -1.0, 0.0)) * protrude)
        radii.append(r)
    mb = MeshBuilder()
    mb.loft(tube_rings(pts, radii, 10, V((0, -1, 0))), "Skin", bulge=(0.002, 0.002))
    return to_object("Nose", mb, "head")


# ---------------------------------------------------------------- rosto (decalques projetados)

EYE_X, EYE_Z = 0.0295, 1.622
EYE_OUTLINE = [(1.08 * u, 1.10 * v) for u, v in (
    (-0.0135, -0.0010), (-0.0105, 0.0052), (-0.0045, 0.0078), (0.0030, 0.0082), (0.0090, 0.0068),
    (0.0130, 0.0038), (0.0148, 0.0005), (0.0112, -0.0040), (0.0050, -0.0060), (-0.0020, -0.0060),
    (-0.0085, -0.0043))]
EYE_UPPER = EYE_OUTLINE[:7]
EYE_LOWER = EYE_OUTLINE[6:] + EYE_OUTLINE[:1]
BROW_LINE = [(-0.0125, 0.0162), (-0.0060, 0.0180), (0.0020, 0.0186), (0.0100, 0.0176), (0.0165, 0.0150)]


def _area2(poly):
    return sum(x0 * z1 - x1 * z0 for (x0, z0), (x1, z1) in zip(poly, poly[1:] + poly[:1]))


def _circle(cx, cz, r, n=20):
    return [(cx + r * math.cos(2 * math.pi * i / n), cz + r * math.sin(2 * math.pi * i / n)) for i in range(n)]


def _clip(subject, clip):
    """Sutherland-Hodgman (clip convexo, anti-horario)."""
    def inside(p, a, b):
        return (b[0] - a[0]) * (p[1] - a[1]) - (b[1] - a[1]) * (p[0] - a[0]) >= 0.0

    def inter(p, q, a, b):
        x1, y1, x2, y2 = p[0], p[1], q[0], q[1]
        x3, y3, x4, y4 = a[0], a[1], b[0], b[1]
        den = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4)
        t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / den
        return (x1 + t * (x2 - x1), y1 + t * (y2 - y1))

    if _area2(clip) < 0:
        clip = clip[::-1]
    out = subject
    for i in range(len(clip)):
        a, b = clip[i], clip[(i + 1) % len(clip)]
        inp, out = out, []
        if not inp:
            break
        s = inp[-1]
        for e in inp:
            if inside(e, a, b):
                if not inside(s, a, b):
                    out.append(inter(s, e, a, b))
                out.append(e)
            elif inside(s, a, b):
                out.append(inter(s, e, a, b))
            s = e
    return out


class Decals:
    """Formas 2D (x, z) projetadas na frente do rosto ao longo de +Y."""

    def __init__(self, surf):
        self.surf = surf
        self.mb = MeshBuilder()

    def _project(self, verts, offset):
        for v in verts:
            loc, nor, _, _ = self.surf.ray(V((v.co.x, -0.5, v.co.z)), V((0, 1, 0)))
            if loc is not None:
                v.co = loc + nor * offset

    def _orient(self, faces):
        for f in faces:
            f.normal_update()
        bad = [f for f in faces if f.normal.y > 0.0]
        if bad:
            bmesh.ops.reverse_faces(self.mb.bm, faces=bad)

    def polygon(self, poly, mat, offset, layers=4):
        bm = self.mb.bm
        mi = self.mb.mat_index(mat)
        if _area2(poly) < 0:
            poly = poly[::-1]
        n = len(poly)
        cx = sum(p[0] for p in poly) / n
        cz = sum(p[1] for p in poly) / n
        c = bm.verts.new((cx, 0.0, cz))
        rings = [[bm.verts.new((cx + (p[0] - cx) * k / layers, 0.0, cz + (p[1] - cz) * k / layers)) for p in poly]
                 for k in range(1, layers + 1)]
        faces = [bm.faces.new((c, rings[0][i], rings[0][(i + 1) % n])) for i in range(n)]
        for a, b in zip(rings, rings[1:]):
            faces += [bm.faces.new((a[i], b[i], b[(i + 1) % n], a[(i + 1) % n])) for i in range(n)]
        for f in faces:
            f.material_index = mi
        self._orient(faces)
        self._project([c] + [v for r in rings for v in r], offset)

    def stroke(self, line, widths, mat, offset, centered=False):
        """Faixa ao longo de uma polilinha; largura para cima (ou centrada)."""
        bm = self.mb.bm
        mi = self.mb.mat_index(mat)
        pts = [V(p) for p in line]
        dense = catmull(pts, 4) if len(pts) > 2 else pts
        ws = [interp([(i / (len(widths) - 1), w) for i, w in enumerate(widths)], k / (len(dense) - 1))
              for k in range(len(dense))]
        lo, hi = [], []
        for k, p in enumerate(dense):
            t = (dense[min(k + 1, len(dense) - 1)] - dense[max(k - 1, 0)]).normalized()
            nrm = V((-t.y, t.x))
            if nrm.y < 0:
                nrm = -nrm
            a = p - nrm * (ws[k] * 0.5) if centered else p - nrm * 0.0004
            b = p + nrm * (ws[k] * 0.5) if centered else p + nrm * ws[k]
            lo.append(bm.verts.new((a.x, 0.0, a.y)))
            hi.append(bm.verts.new((b.x, 0.0, b.y)))
        faces = [bm.faces.new((lo[i], lo[i + 1], hi[i + 1], hi[i])) for i in range(len(dense) - 1)]
        for f in faces:
            f.material_index = mi
        self._orient(faces)
        self._project(lo + hi, offset)


def build_face(head, nose):
    surf = Surface([head])
    dec = Decals(surf)
    for side in (1, -1):
        def w(u, v):
            return (side * (EYE_X + u), EYE_Z + v)

        sclera_local = EYE_OUTLINE
        dec.polygon([w(*p) for p in sclera_local], "EyeWhite", 0.0007)
        iris_c = (0.0005, 0.0004)
        iris = _clip(_circle(iris_c[0], iris_c[1], 0.0077), sclera_local)
        dec.polygon([w(*p) for p in iris], "Iris", 0.0012, layers=2)
        pupil = _clip(_circle(iris_c[0], iris_c[1] + 0.0002, 0.0036, 16), sclera_local)
        dec.polygon([w(*p) for p in pupil], "Pupil", 0.0017, layers=1)
        hl_u = side * -0.0024                     # brilho sempre do mesmo lado do rosto
        hl = _clip(_circle(iris_c[0] + hl_u, iris_c[1] + 0.0031, 0.0017, 12), sclera_local)
        dec.polygon([w(*p) for p in hl], "Highlight", 0.0022, layers=1)
        hl2 = _clip(_circle(iris_c[0] - hl_u * 0.9, iris_c[1] - 0.0030, 0.0008, 10), sclera_local)
        dec.polygon([w(*p) for p in hl2], "Highlight", 0.0022, layers=1)
        # linha dos cilios superiores (com "flick" no canto externo)
        upper = [w(*p) for p in EYE_UPPER] + [w(0.0165, 0.0021)]
        dec.stroke(upper, [0.0011, 0.0019, 0.0024, 0.0028, 0.0030, 0.0026, 0.0018, 0.0008], "EyeLine", 0.0026)
        lower = [w(*p) for p in EYE_LOWER[:3]]
        dec.stroke(lower[::-1], [0.0004, 0.0007, 0.0004], "EyeLine", 0.0024, centered=True)
        crease = [w(u, v + 0.0038) for (u, v) in EYE_UPPER[1:6]]
        dec.stroke(crease, [0.0003, 0.0006, 0.0006, 0.0005, 0.0003], "SkinShade", 0.0012, centered=True)
        brow = [w(*p) for p in BROW_LINE]
        dec.stroke(brow, [0.0046, 0.0044, 0.0038, 0.0030, 0.0018], "Brow", 0.0011, centered=True)
    mouth = [(-0.0155, 1.5602), (-0.0080, 1.5612), (0.0, 1.5615), (0.0080, 1.5612), (0.0155, 1.5602)]
    dec.stroke(mouth, [0.0006, 0.0012, 0.0015, 0.0012, 0.0006], "Lips", 0.0009, centered=True)
    lip = [(-0.0060, 1.5560), (0.0, 1.5555), (0.0060, 1.5560)]
    dec.stroke(lip, [0.0004, 0.0010, 0.0004], "SkinShade", 0.0007, centered=True)
    return to_object("Face", dec.mb, "head", closed=False, subsurf=0)


# ---------------------------------------------------------------- cabelo

HAIRLINE = [(0.0, 0.52), (0.5, 0.46), (0.9, 0.30), (1.22, 0.12), (1.38, -0.12), (1.48, 0.17),
            (1.88, 0.17), (2.05, -0.25), (2.40, -0.48), (2.80, -0.60), (math.pi, -0.66)]


def head_dir(theta_deg, phi_deg):
    t, p = math.radians(theta_deg), math.radians(phi_deg)
    return V((math.sin(t) * math.cos(p), -math.cos(t) * math.cos(p), math.sin(p)))


def _scalp(co):
    d = co - HEAD_C
    theta = abs(math.atan2(d.x, -d.y))
    zrel = d.z / HEAD_H
    return zrel - interp(HAIRLINE, theta)


def build_hair_cap(head):
    mb = MeshBuilder()
    bm = mb.bm
    bm.from_mesh(head.data)
    drop = [f for f in bm.faces if min(_scalp(v.co) for v in f.verts) < 0.0]
    bmesh.ops.delete(bm, geom=drop, context="FACES")
    bm.normal_update()
    boundary = {v for e in bm.edges if e.is_boundary for v in e.verts}
    moves = {}
    for v in bm.verts:
        zrel = (v.co.z - HEAD_C.z) / HEAD_H
        off = 0.008 + 0.006 * smoothstep(0.2, 1.0, zrel)
        if v in boundary:
            off = 0.0025
        moves[v] = v.co + v.normal * off
    for v, co in moves.items():
        v.co = co
    mi = mb.mat_index("Hair")
    for f in bm.faces:
        f.material_index = mi
    return to_object("HairCap", mb, "hair", closed=False)


def _strand(mb, pts, nors, w0, th0, mat, seg=6):
    rings = []
    n = len(pts)
    for k, (p, nor) in enumerate(zip(pts, nors)):
        s = k / (n - 1)
        t = (pts[min(k + 1, n - 1)] - pts[max(k - 1, 0)]).normalized()
        ay = nor - t * nor.dot(t)
        if ay.length < 1e-5:
            ay = (p - HEAD_C) - t * (p - HEAD_C).dot(t)
        ay.normalize()
        ax = ay.cross(t)
        width = w0 * (1.0 - s) ** 0.75 + 0.0012
        thick = max(th0 * (1.0 - 0.65 * s), 0.0012)
        rings.append(ring(p, ax, ay, width, thick, seg))
    tip = (pts[-1] - pts[-2]).normalized() * 0.004
    mb.loft(rings, mat, bulge=(0.0, tip.length))


def _hug_path(surf, root, direction, length, steps, offsets, gravity=0.0, drift=None):
    pts, nors = [], []
    p, d = root.copy(), direction.normalized()
    for k in range(steps + 1):
        loc, nor, _, _ = surf.nearest(p)
        p = loc + nor * interp(offsets, k / steps)
        pts.append(p.copy())
        nors.append(nor.copy())
        d = d + V((0.0, 0.0, -gravity))
        if drift is not None:
            d = d + drift
        d = (d - nor * d.dot(nor)).normalized()
        p = p + d * (length / steps)
    return pts, nors


def _spike_path(surf, root, direction, length, steps, gravity, min_off=0.012):
    pts, nors = [], []
    p, d = root.copy(), direction.normalized()
    for k in range(steps + 1):
        loc, nor, _, _ = surf.nearest(p)
        want = min_off * (0.5 if k == 0 else 1.0)
        if (p - loc).dot(nor) < want:
            p = loc + nor * want
        pts.append(p.copy())
        nors.append(nor.copy())
        d = (d + V((0.0, 0.0, -gravity))).normalized()
        p = p + d * (length / steps)
    return pts, nors


def _tangents(nor):
    up = V((0, 0, 1)) - nor * nor.z
    if up.length < 1e-4:
        up = V((0, 1, 0)) - nor * nor.y
    up.normalize()
    return up, up.cross(nor).normalized()        # (meridiano para cima, azimute crescente)


def build_hair(head):
    surf = Surface([head])
    rnd = random.Random(7)
    mb = MeshBuilder()

    def root(theta, phi):
        d = head_dir(theta, phi)
        loc, nor, _, _ = surf.ray(HEAD_C + d * 0.4, -d)
        return loc, nor

    # franja: cai sobre a testa, puxada para o lado direito do personagem
    for i in range(10):
        theta = -48 + i * 10.5 + rnd.uniform(-3, 3)
        loc, nor = root(theta, 57 + rnd.uniform(-4, 4))
        up, az = _tangents(nor)
        d = -up - az * (0.30 + rnd.uniform(-0.1, 0.1))
        pts, nors = _hug_path(surf, loc, d, rnd.uniform(0.080, 0.100), 6,
                              [(0.0, 0.006), (0.18, 0.020), (1.0, 0.0045)], gravity=0.25)
        _strand(mb, pts, nors, rnd.uniform(0.020, 0.027), 0.0065,
                "HairLight" if i % 4 == 1 else "Hair")
    # topo: camada deitada que da volume, escorrendo para tras e para os lados
    for i in range(16):
        theta = -170 + i * 22.5 + rnd.uniform(-6, 6)
        loc, nor = root(theta, rnd.uniform(66, 84))
        up, az = _tangents(nor)
        flow = -up + V((0.0, 0.6, 0.0)) + az * rnd.uniform(-0.25, 0.25)   # escorre a partir do redemoinho
        pts, nors = _hug_path(surf, loc, flow, rnd.uniform(0.070, 0.095), 6,
                              [(0.0, 0.008), (0.2, 0.019), (0.7, 0.015), (1.0, 0.011)], gravity=0.15)
        _strand(mb, pts, nors, rnd.uniform(0.024, 0.030), 0.0075, "HairLight" if i % 4 == 2 else "Hair")
    # topo: pontas soltas e baguncadas, inclinadas para tras
    for i in range(22):
        theta = rnd.uniform(-180, 180)
        phi = rnd.uniform(55, 84)
        loc, nor = root(theta, phi)
        up, az = _tangents(nor)
        back = V((0.0, 1.0, 0.15))
        d = nor * rnd.uniform(0.25, 0.5) + back * rnd.uniform(0.7, 1.0) + az * rnd.uniform(-0.45, 0.45)
        pts, nors = _spike_path(surf, loc, d, rnd.uniform(0.026, 0.042), 4, rnd.uniform(0.25, 0.45), 0.016)
        _strand(mb, pts, nors, rnd.uniform(0.016, 0.022), 0.0070, "HairLight" if i % 5 == 0 else "Hair")
    # laterais (acima das orelhas)
    for side in (1, -1):
        for i in range(8):
            theta = side * (68 + i * 8.5 + rnd.uniform(-3, 3))
            loc, nor = root(theta, rnd.uniform(30, 48))
            up, az = _tangents(nor)
            d = -up + az * (-side * 0.25) + V((0, 0.25, 0))
            pts, nors = _hug_path(surf, loc, d, rnd.uniform(0.045, 0.060), 5,
                                  [(0.0, 0.006), (0.2, 0.017), (1.0, 0.006)], gravity=0.2)
            _strand(mb, pts, nors, rnd.uniform(0.018, 0.024), 0.0075, "Hair")
    # nuca e atras das orelhas (duas fileiras)
    for i in range(15):
        theta = 180 + (i - 7) * 10.0 + rnd.uniform(-3, 3)
        loc, nor = root(theta, rnd.uniform(18, 42))
        up, az = _tangents(nor)
        pts, nors = _hug_path(surf, loc, -up, rnd.uniform(0.065, 0.085), 6,
                              [(0.0, 0.006), (0.2, 0.018), (0.8, 0.010), (1.0, 0.013)], gravity=0.1)
        _strand(mb, pts, nors, rnd.uniform(0.020, 0.026), 0.0085, "Hair")
    for i in range(14):
        theta = 180 + (i - 6.5) * 12.5 + rnd.uniform(-3, 3)
        loc, nor = root(theta, rnd.uniform(-8, 6))
        up, az = _tangents(nor)
        pts, nors = _hug_path(surf, loc, -up, rnd.uniform(0.050, 0.065), 4,
                              [(0.0, 0.004), (0.3, 0.011), (1.0, 0.007)], gravity=0.1)
        _strand(mb, pts, nors, rnd.uniform(0.018, 0.022), 0.0060, "Hair")
    # redemoinho na parte de tras do topo
    for i in range(5):
        loc, nor = root(150 + i * 15 + rnd.uniform(-5, 5), rnd.uniform(62, 72))
        up, az = _tangents(nor)
        d = nor * 0.5 + V((0.0, 0.8, 0.0)) + az * rnd.uniform(-0.4, 0.4)
        pts, nors = _spike_path(surf, loc, d, rnd.uniform(0.028, 0.040), 4, 0.35, 0.014)
        _strand(mb, pts, nors, 0.020, 0.009, "Hair")
    return to_object("Hair", mb, "hair", subsurf=0)


# ---------------------------------------------------------------- moletom

HOODIE_RINGS = [
    # z, meia-largura, meia-profundidade, centro y, expoente
    (0.835, 0.186, 0.131, 0.000, 2.6),
    (0.848, 0.188, 0.132, 0.000, 2.6),
    (0.872, 0.189, 0.133, 0.000, 2.6),
    (0.886, 0.196, 0.138, 0.000, 2.6),
    (0.935, 0.195, 0.137, -0.002, 2.6),
    (1.010, 0.186, 0.130, -0.002, 2.6),
    (1.090, 0.184, 0.129, -0.003, 2.6),
    (1.170, 0.189, 0.131, -0.005, 2.6),
    (1.245, 0.197, 0.133, -0.006, 2.7),
    (1.315, 0.200, 0.127, -0.003, 2.8),
    (1.365, 0.193, 0.117, 0.002, 2.8),
    (1.405, 0.176, 0.104, 0.006, 2.6),
    (1.435, 0.155, 0.092, 0.009, 2.4),
    (1.455, 0.116, 0.083, 0.011, 2.2),
    (1.468, 0.084, 0.073, 0.012, 2.0),
]


def hoodie_back_y(z):
    return interp([(r[0], r[3] + r[2]) for r in HOODIE_RINGS], z)


def build_hoodie_body():
    mb = MeshBuilder()
    rings = [ring(V((0.0, cy, z)), V((1, 0, 0)), V((0, 1, 0)), hx, hy, 24, ex)
             for z, hx, hy, cy, ex in HOODIE_RINGS]
    mb.loft(rings, "Hoodie", bulge=(-0.05, -0.06), band_mat=lambda k: "HoodieRib" if k < 2 else "Hoodie")
    return to_object("HoodieBody", mb, "hoodie")


def build_sleeve_left():
    mb = MeshBuilder()
    pts = [S - A * 0.050 + V((0, 0, -0.014)), S - A * 0.010, S + A * 0.07, S + A * 0.17, E - A * 0.02,
           E + A2 * 0.06, E + A2 * 0.15, E + A2 * 0.200, E + A2 * 0.212, E + A2 * 0.232, E + A2 * 0.250]
    radii = [(0.042, 0.048), (0.054, 0.058), (0.058, 0.060), (0.057, 0.059), (0.056, 0.058),
             (0.055, 0.057), (0.054, 0.055), (0.052, 0.053), (0.0455, 0.0465), (0.044, 0.045), (0.043, 0.044)]
    mb.loft(tube_rings(pts, radii, 16, V((0, 1, 0))), "Hoodie", bulge=(0.0, -0.02),
            band_mat=lambda k: "HoodieRib" if k >= 8 else "Hoodie")
    ob = to_object("Sleeve.L", mb, "sleeve", side=1)
    return [ob, mirror_object(ob, "Sleeve.R")]


def build_hood():
    mb = MeshBuilder()
    half = [(V((0.052, -0.064, 1.452)), (0.012, 0.012)), (V((0.083, -0.036, 1.470)), (0.016, 0.020)),
            (V((0.101, 0.012, 1.486)), (0.019, 0.030)), (V((0.090, 0.064, 1.497)), (0.025, 0.040)),
            (V((0.049, 0.102, 1.501)), (0.029, 0.046)), ]
    seq = half + [(V((0.0, 0.115, 1.500)), (0.031, 0.048))] + \
        [(V((-p.x, p.y, p.z)), r) for p, r in reversed(half)]
    pts = catmull([p for p, _ in seq], 2)
    radii = catmull([V(r) for _, r in seq], 2)
    mb.loft(tube_rings(pts, radii, 10, V((0, 0, 1))), "Hoodie", bulge=(0.006, 0.006))
    # capuz caido sobre as costas
    rows = [(1.320, 0.050, 0.008), (1.345, 0.090, 0.013), (1.380, 0.115, 0.016),
            (1.420, 0.122, 0.018), (1.455, 0.112, 0.020), (1.480, 0.080, 0.020)]
    rings = [ring(V((0.0, hoodie_back_y(min(z, 1.468)) + th + 0.002, z)), V((1, 0, 0)), V((0, 1, 0)),
                  hx, th, 16, 2.4) for z, hx, th in rows]
    mb.loft(rings, "Hoodie", bulge=(0.004, 0.004))
    return to_object("Hood", mb, "hood")


def build_tee():
    mb = MeshBuilder()
    rows = [(1.415, 0.105, 0.085, 0.012), (1.440, 0.084, 0.074, 0.014), (1.462, 0.063, 0.064, 0.016),
            (1.474, 0.0545, 0.0555, 0.016)]
    rings = [ring(V((0.0, cy, z)), V((1, 0, 0)), V((0, 1, 0)), rx, ry, 16) for z, rx, ry, cy in rows]
    mb.loft(rings, "Tee", bulge=(-0.03, -0.01))
    # gola canelada da camiseta
    pts = [V((0.0575 * math.cos(a), 0.016 + 0.0585 * math.sin(a), 1.471)) for a in
           [2 * math.pi * i / 18 for i in range(18)]]
    pts.append(pts[0].copy())
    rings = tube_rings(pts, [(0.0045, 0.004)] * len(pts), 6, V((0, 0, 1)))
    mb.loft(rings, "Tee", caps=(False, False))
    ob = to_object("Tee", mb, "tee", closed=False)
    return ob


def build_pocket(body):
    surf = Surface([body])
    mb = MeshBuilder()
    bm = mb.bm
    mi_front = mb.mat_index("Hoodie")
    mi_edge = mb.mat_index("HoodieRib")
    nu, nv = 10, 6
    front, back = {}, {}
    for i in range(nu + 1):
        for j in range(nv + 1):
            s, t = i / nu, j / nv
            z = lerp(0.905, 1.075, t)
            half = lerp(0.145, 0.092, t)
            x = lerp(-half, half, s)
            loc, nor, _, _ = surf.ray(V((x, -0.5, z)), V((0, 1, 0)))
            front[i, j] = bm.verts.new(loc + nor * 0.008)
            back[i, j] = bm.verts.new(loc - nor * 0.004)
    for i in range(nu):
        for j in range(nv):
            bm.faces.new((front[i, j], front[i + 1, j], front[i + 1, j + 1], front[i, j + 1])).material_index = mi_front
            bm.faces.new((back[i, j + 1], back[i + 1, j + 1], back[i + 1, j], back[i, j])).material_index = mi_front
    loop = [(i, 0) for i in range(nu)] + [(nu, j) for j in range(nv)] + \
           [(i, nv) for i in range(nu, 0, -1)] + [(0, j) for j in range(nv, 0, -1)]
    for a, b in zip(loop, loop[1:] + loop[:1]):
        bm.faces.new((front[b], front[a], back[a], back[b])).material_index = mi_edge
    return to_object("Pocket", mb, "pocket")


def build_drawstrings(body):
    surf = Surface([body])
    mb = MeshBuilder()
    for side in (1, -1):
        ctrl = [V((side * 0.030, 0.0, 1.448)), V((side * 0.033, 0.0, 1.400)),
                V((side * 0.036, 0.0, 1.350)), V((side * 0.038, 0.0, 1.302))]
        pts, nors = [], []
        for p in catmull(ctrl, 4):
            loc, nor, _, _ = surf.ray(V((p.x, -0.5, p.z)), V((0, 1, 0)))
            pts.append(loc + nor * 0.0045)
            nors.append(nor)
        mb.loft(tube_rings(pts, [(0.0032, 0.0028)] * len(pts), 6, lambda i, p, t: nors[i]),
                "Drawstring", bulge=(0.0, 0.0))
        t = (pts[-1] - pts[-2]).normalized()
        aglet = [pts[-1] + t * 0.002, pts[-1] + t * 0.016]
        mb.loft(tube_rings(aglet, [(0.0042, 0.0038)] * 2, 8, nors[-1]), "Metal", bulge=(0.0, 0.001))
        # ilhos
        loc, nor, _, _ = surf.ray(V((side * 0.030, -0.5, 1.450)), V((0, 1, 0)))
        eye = [loc - nor * 0.002, loc + nor * 0.0025]
        mb.loft(tube_rings(eye, [(0.0055, 0.0055)] * 2, 12, V((0, 0, 1))), "Metal")
    return to_object("Drawstrings", mb, "drawstring", subsurf=0)


# ---------------------------------------------------------------- maos e relogio

FINGERS = {
    # nome: (posicao na largura, comprimentos, raio, abertura (graus), curvatura por junta (graus))
    "Index": (0.027, (0.036, 0.022, 0.019), 0.0098, 5.0, (10, 14, 10)),
    "Middle": (0.009, (0.040, 0.025, 0.020), 0.0101, 0.0, (12, 16, 10)),
    "Ring": (-0.009, (0.037, 0.023, 0.019), 0.0096, -4.0, (14, 18, 12)),
    "Pinky": (-0.026, (0.029, 0.018, 0.016), 0.0085, -9.0, (16, 20, 12)),
}
FINGER_BASE_L = 0.088
THUMB = [(0.008, 0.021, 0.006), (0.040, 0.044, 0.018), (0.063, 0.052, 0.027), (0.083, 0.053, 0.033)]
THUMB_R = (0.0130, 0.0115, 0.0102, 0.0085)
PALM = [  # l, meia-largura, meia-espessura, deslocamento na normal, expoente
    (-0.040, 0.026, 0.0200, 0.000, 2.2),
    (-0.008, 0.027, 0.0195, 0.000, 2.2),
    (0.022, 0.035, 0.0170, 0.001, 2.8),
    (0.058, 0.041, 0.0150, 0.001, 3.0),
    (0.090, 0.0415, 0.0135, 0.000, 3.0),
]


def hand_axes():
    """(L, W, N) da mao esquerda: comprimento, largura (lado do polegar) e normal da palma."""
    L = A2.copy()
    N = V((L.z, 0.0, -L.x))
    N = (N - L * N.dot(L)).normalized()
    W = N.cross(L).normalized()
    return L, W, N


def hand_point(l, w, n):
    L, W, N = hand_axes()
    return WR + L * l + W * w + N * n


def finger_joints():
    """Juntas (base, j1, j2, ponta) dos dedos da mao esquerda, no mundo."""
    out = {}
    for name, (w, lens, _r, spread, curls) in FINGERS.items():
        s = math.radians(spread)
        pts = [V((FINGER_BASE_L, w, -0.002))]
        ang = 0.0
        for seg_len, c in zip(lens, curls):
            ang += math.radians(c)
            d = V((math.cos(s) * math.cos(ang), math.sin(s) * math.cos(ang), math.sin(ang)))
            pts.append(pts[-1] + d * seg_len)
        out[name] = [hand_point(*p) for p in pts]
    out["Thumb"] = [hand_point(*p) for p in THUMB]
    return out


def build_hand_left():
    L, W, N = hand_axes()
    mb = MeshBuilder()
    rings = [ring(hand_point(l, 0.0, off), W, N, hw, ht, 12, ex) for l, hw, ht, off, ex in PALM]
    mb.loft(rings, "Skin", bulge=(0.0, 0.010))
    for name, joints in finger_joints().items():
        if name == "Thumb":
            radii = THUMB_R
        else:
            r = FINGERS[name][2]
            radii = (r, r * 0.95, r * 0.9, r * 0.78)
        start = joints[0] - (joints[1] - joints[0]).normalized() * 0.012
        pts = [start]
        rr = [radii[0] * 0.95]
        for k in range(3):
            pts += [joints[k], (joints[k] + joints[k + 1]) * 0.5]
            rr += [radii[k], (radii[k] + radii[k + 1]) * 0.5]
        pts.append(joints[3])
        rr.append(radii[3])
        mb.loft(tube_rings(pts, [(r, r * 0.88) for r in rr], 8, N), "Skin", bulge=(0.0, rr[-1] * 0.9))
    ob = to_object("Hand.L", mb, "hand", side=1)
    return [ob, mirror_object(ob, "Hand.R")]


def build_watch():
    L, W, N = hand_axes()
    mb = MeshBuilder()
    band = [hand_point(0.006, 0.0, 0.0), hand_point(0.020, 0.0, 0.0)]
    mb.loft(tube_rings(band, [(0.0355, 0.0225)] * 2, 16, N), "WatchBand", bulge=(0.0, 0.0))
    base = hand_point(0.013, 0.0, -0.0225)
    rows = [(-0.001, 0.90), (0.004, 1.0), (0.0085, 0.96)]
    rings = [ring(base - N * h, W, L, 0.0135 * s, 0.0150 * s, 16, 4.0) for h, s in rows]
    mb.loft(rings, "WatchBand", bulge=(0.0, 0.0005))
    screen = [ring(base - N * 0.0090, W, L, 0.0102, 0.0110, 16, 4.0),
              ring(base - N * 0.0094, W, L, 0.0099, 0.0107, 16, 4.0)]
    mb.loft(screen, "WatchScreen", bulge=(0.0, 0.0))
    return to_object("Watch", mb, "watch", side=1, subsurf=0)


# ---------------------------------------------------------------- calca e tenis

PELVIS_RINGS = [(1.000, 0.170, 0.118, 0.000), (0.940, 0.178, 0.122, 0.000), (0.880, 0.174, 0.120, 0.002),
                (0.835, 0.152, 0.110, 0.004), (0.805, 0.095, 0.078, 0.004)]
JEANS_LEG = [
    (V((0.080, 0.000, 1.000)), (0.090, 0.112)),
    (V((0.088, 0.000, 0.930)), (0.088, 0.106)),
    (V((0.091, -0.003, 0.860)), (0.086, 0.098)),
    (V((0.094, -0.006, 0.760)), (0.081, 0.089)),
    (V((0.096, -0.008, 0.640)), (0.075, 0.081)),
    (V((0.097, -0.009, 0.540)), (0.069, 0.073)),
    (V((0.097, -0.010, 0.490)), (0.067, 0.071)),
    (V((0.099, -0.002, 0.380)), (0.064, 0.068)),
    (V((0.101, 0.006, 0.260)), (0.062, 0.065)),
    (V((0.103, 0.011, 0.165)), (0.061, 0.064)),
    (V((0.104, 0.012, 0.118)), (0.064, 0.067)),
    (V((0.104, 0.010, 0.090)), (0.061, 0.064)),
    (V((0.104, 0.008, 0.062)), (0.066, 0.069)),
]


def build_pelvis():
    mb = MeshBuilder()
    rings = [ring(V((0.0, cy, z)), V((1, 0, 0)), V((0, 1, 0)), hx, hy, 20, 2.4) for z, hx, hy, cy in PELVIS_RINGS]
    mb.loft(rings, "Jeans", bulge=(0.0, 0.025))
    return to_object("Pelvis", mb, "pelvis")


def build_leg_left():
    mb = MeshBuilder()
    pts = [p for p, _ in JEANS_LEG]
    rings = tube_rings(pts, [r for _, r in JEANS_LEG], 16, V((0, 1, 0)))
    c = pts[-1]
    rings[-1] = [p + V((0.0, 0.0, -0.10 * (p.y - c.y))) for p in rings[-1]]   # barra mais baixa atras
    mb.loft(rings, "Jeans", bulge=(0.0, -0.03))
    ob = to_object("Leg.L", mb, "jeans", side=1)
    return [ob, mirror_object(ob, "Leg.R")]


SHOE_X = 0.106
SHOE_UPPER = [  # y, meia-largura, topo, base
    (0.072, 0.026, 0.070, 0.020), (0.066, 0.036, 0.080, 0.020), (0.045, 0.042, 0.088, 0.020),
    (0.010, 0.045, 0.090, 0.020), (-0.030, 0.047, 0.083, 0.020), (-0.070, 0.050, 0.071, 0.020),
    (-0.110, 0.052, 0.060, 0.020), (-0.145, 0.051, 0.053, 0.020), (-0.170, 0.048, 0.048, 0.020),
    (-0.188, 0.041, 0.043, 0.020), (-0.198, 0.030, 0.038, 0.021), (-0.203, 0.016, 0.034, 0.023),
]


def build_shoe_left():
    mb = MeshBuilder()
    up = mb.bm
    rings = []
    for y, hw, top, bot in SHOE_UPPER:
        rings.append(ring(V((SHOE_X, y, (top + bot) * 0.5)), V((-1, 0, 0)), V((0, 0, 1)), hw, (top - bot) * 0.5,
                          16, 2.8))
    mb.loft(rings, "ShoeCanvas", bulge=(0.004, 0.004))
    rub = mb.mat_index("ShoeRubber")
    for f in up.faces:
        c = f.calc_center_median()
        if c.z < 0.031 or (c.y < -0.166 and c.z < 0.050):
            f.material_index = rub
    # sola
    sole = []
    ys = [0.078] + [r[0] for r in SHOE_UPPER[1:-1]] + [-0.209]
    for k, y in enumerate(ys):
        hw = SHOE_UPPER[min(k, len(SHOE_UPPER) - 1)][1] + 0.005
        sole.append(ring(V((SHOE_X, y, 0.0135)), V((-1, 0, 0)), V((0, 0, 1)), hw, 0.0135, 16, 4.0))
    mb.loft(sole, "ShoeRubber", bulge=(0.003, 0.004))
    ob = to_object("Shoe.L", mb, "shoe", side=1)
    surf = Surface([ob])
    laces = MeshBuilder()
    for y in (-0.030, -0.048, -0.066, -0.084, -0.102):
        pts = []
        for k in range(7):
            x = SHOE_X + lerp(-0.024, 0.024, k / 6)
            loc, nor, _, _ = surf.ray(V((x, y, 0.3)), V((0, 0, -1)))
            pts.append(loc + nor * 0.0025)
        laces.loft(tube_rings(pts, [(0.0034, 0.0022)] * len(pts), 6, V((0, 0, 1))), "Laces", bulge=(0.001, 0.001))
    loc, nor, _, _ = surf.ray(V((SHOE_X, -0.022, 0.3)), V((0, 0, -1)))
    knot = MeshBuilder()
    bmesh.ops.create_uvsphere(knot.bm, u_segments=8, v_segments=6, radius=1.0)
    mi = knot.mat_index("Laces")
    for v in knot.bm.verts:
        v.co = loc + nor * 0.004 + V((v.co.x * 0.008, v.co.y * 0.006, v.co.z * 0.004))
    for f in knot.bm.faces:
        f.material_index = mi
    laces.add_mesh(knot)
    for side in (1, -1):
        loop = [loc + nor * 0.004, loc + V((side * 0.012, -0.004, 0.006)), loc + V((side * 0.020, 0.004, 0.003)),
                loc + V((side * 0.010, 0.002, 0.003))]
        laces.loft(tube_rings(catmull(loop, 3), [(0.0028, 0.0018)] * 10, 6, V((0, 0, 1))), "Laces")
    lob = to_object("Laces.L", laces, "shoe", side=1, subsurf=0)
    return [ob, mirror_object(ob, "Shoe.R"), lob, mirror_object(lob, "Laces.R")]


# ---------------------------------------------------------------- mochila

PACK_RINGS = [  # z, meia-largura, meia-profundidade, centro y, expoente
    (0.985, 0.110, 0.050, 0.215, 4.0), (0.995, 0.135, 0.064, 0.215, 4.0), (1.015, 0.148, 0.071, 0.214, 4.0),
    (1.120, 0.150, 0.073, 0.212, 4.0), (1.240, 0.147, 0.071, 0.208, 4.0), (1.310, 0.140, 0.067, 0.205, 3.6),
    (1.350, 0.125, 0.060, 0.203, 3.0), (1.375, 0.095, 0.048, 0.202, 2.6), (1.388, 0.050, 0.028, 0.201, 2.2),
]


def build_backpack(body, sleeves, hood):
    mb = MeshBuilder()
    rings = [ring(V((0.0, cy, z)), V((1, 0, 0)), V((0, 1, 0)), hx, hy, 24, ex) for z, hx, hy, cy, ex in PACK_RINGS]
    mb.loft(rings, "Backpack", bulge=(-0.004, 0.006))
    # bolso frontal (do lado de fora)
    rows = [(0.270, 1.0), (0.296, 0.99), (0.309, 0.93), (0.316, 0.78)]
    rings = [ring(V((0.0, y, 1.105)), V((0, 0, 1)), V((1, 0, 0)), 0.078 * s, 0.112 * s, 20, 3.6) for y, s in rows]
    mb.loft(rings, "Backpack", bulge=(0.0, 0.004))
    pack = to_object("Backpack", mb, "backpack")
    surf = Surface([pack])
    det = MeshBuilder()
    # ziper do bolso e ziper principal
    zp = []
    for k in range(9):
        x = lerp(-0.095, 0.095, k / 8)
        loc, nor, _, _ = surf.ray(V((x, 0.6, 1.168)), V((0, -1, 0)))
        zp.append(loc + nor * 0.0015)
    det.loft(tube_rings(zp, [(0.0035, 0.0022)] * len(zp), 6, V((0, 1, 0))), "Zipper", bulge=(0.002, 0.002))
    zm = []
    for k in range(15):
        a = lerp(-1.25, 1.25, k / 14)
        d = V((math.sin(a), 0.55, math.cos(a))).normalized()
        o = V((0.0, 0.215, 1.20))
        loc, nor, _, _ = surf.ray(o + d * 0.5, -d)
        zm.append(loc + nor * 0.0015)
    det.loft(tube_rings(zm, [(0.0038, 0.0024)] * len(zm), 6, lambda i, p, t: (p - V((0, 0.215, 1.2)))),
             "Zipper", bulge=(0.002, 0.002))
    for p, nor in ((zp[-2], V((0, 1, 0))), (zm[3], (zm[3] - V((0, 0.215, 1.2))).normalized())):
        pull = [p + nor * 0.002, p + nor * 0.004 + V((0, 0, -0.022))]
        det.loft(tube_rings(pull, [(0.005, 0.0016)] * 2, 8, nor, 4.0), "Zipper", bulge=(0.001, 0.003))
    # alca de mao no topo
    loop = [V((-0.032, 0.200, 1.382)), V((-0.020, 0.200, 1.410)), V((0.0, 0.200, 1.418)),
            V((0.020, 0.200, 1.410)), V((0.032, 0.200, 1.382))]
    det.loft(tube_rings(catmull(loop, 3), [(0.008, 0.0035)] * 13, 8, V((0, 1, 0))), "BackpackDark")
    details = to_object("BackpackDetails", det, "backpack", subsurf=0)
    straps = build_straps(body, sleeves, hood)
    return [pack, details] + straps


STRAP_CTRL = [V((0.075, 0.150, 1.360)), V((0.100, 0.115, 1.432)), V((0.122, 0.060, 1.462)),
              V((0.132, 0.000, 1.462)), V((0.136, -0.060, 1.432)), V((0.135, -0.108, 1.375)),
              V((0.134, -0.132, 1.300)), V((0.140, -0.136, 1.220)), V((0.152, -0.124, 1.150)),
              V((0.172, -0.086, 1.095)), V((0.188, -0.022, 1.060)), V((0.182, 0.058, 1.036)),
              V((0.150, 0.150, 1.010))]


def build_straps(body, sleeves, hood):
    # mangas so contam em cima do ombro; mais abaixo o raio acertaria o braco em pose A
    surf_shoulder = Surface([body] + sleeves + [hood])
    surf_torso = Surface([body, hood])
    mb = MeshBuilder()
    pts, nors = [], []
    path = catmull(STRAP_CTRL, 3)
    for k, p in enumerate(path):
        axis = V((0.0, 0.0, min(p.z, 1.36)))
        if p.y > 0.12 and p.z < 1.40:
            axis = V((0.0, 0.0, p.z))
        d = (p - axis).normalized()
        surf = surf_shoulder if p.z > 1.40 else surf_torso
        loc, nor, _, _ = surf.ray(p + d * 0.4, -d)
        if loc is None:
            loc, nor = p, d
        pts.append(loc)
        nors.append(nor)
    smooth = []
    for k in range(len(nors)):
        acc = V()
        for j in range(max(0, k - 2), min(len(nors), k + 3)):
            acc += nors[j]
        smooth.append(acc.normalized())
    thick = [interp([(0.0, 0.004), (0.15, 0.008), (0.55, 0.008), (0.7, 0.005), (1.0, 0.004)], k / (len(pts) - 1))
             for k in range(len(pts))]
    centers = [p + n * (t + 0.004) for p, n, t in zip(pts, smooth, thick)]
    rings = tube_rings(centers, [(0.024, t) for t in thick], 8, lambda i, p, t: smooth[i], 4.0)
    mb.loft(rings, "BackpackDark", bulge=(0.0, 0.0))
    # fivela de ajuste
    k = int(len(centers) * 0.62)
    t = (centers[k + 1] - centers[k - 1]).normalized()
    b0 = centers[k] + smooth[k] * 0.003
    mb.loft(tube_rings([b0 - t * 0.012, b0 + t * 0.012], [(0.028, 0.009)] * 2, 8, smooth[k], 4.0), "Zipper",
            bulge=(0.001, 0.001))
    ob = to_object("Strap.L", mb, "strap", side=1, subsurf=0)
    return [ob, mirror_object(ob, "Strap.R")]


# ---------------------------------------------------------------- caderno (prop separado)


def build_notebook():
    mb = MeshBuilder()
    w, h, t = 0.105, 0.140, 0.0065
    cover = [ring(V((0.0, 0.0, z)), V((1, 0, 0)), V((0, 1, 0)), w, h, 24, 8.0) for z in (-t, -t + 0.0015)]
    mb.loft(cover, "NotebookCover")
    pages = [ring(V((0.0, 0.0, z)), V((1, 0, 0)), V((0, 1, 0)), w - 0.003, h - 0.003, 24, 8.0)
             for z in (-t + 0.0015, t - 0.0015)]
    mb.loft(pages, "NotebookPages")
    cover2 = [ring(V((0.0, 0.0, z)), V((1, 0, 0)), V((0, 1, 0)), w, h, 24, 8.0) for z in (t - 0.0015, t)]
    mb.loft(cover2, "NotebookCover")
    for k in range(13):
        y = lerp(-h + 0.014, h - 0.014, k / 12)
        pts = [V((-w + 0.008 + 0.010 * math.cos(a), y, 0.011 * math.sin(a))) for a in
               [math.pi * (0.5 + 1.5 * i / 8) for i in range(9)]]
        mb.loft(tube_rings(pts, [(0.0012, 0.0012)] * len(pts), 6, V((0, 1, 0))), "Metal")
    ob = to_object("Notebook", mb, "notebook", subsurf=0)
    ob.location = (0.55, -0.25, t)
    return ob


# ---------------------------------------------------------------- esqueleto e pesos
# Nomes no padrao Mixamo: o Unity mapeia sozinho para Humanoid e animacoes do Mixamo servem direto.

FINGER_NAMES = ("Thumb", "Index", "Middle", "Ring", "Pinky")
FINGER_BONES = ["Hand%s%d" % (f, k) for f in FINGER_NAMES for k in (1, 2, 3)]
NECK_BASE = V((0.0, 0.010, 1.430))
HEAD_BASE = V((0.0, 0.016, 1.535))


def skeleton():
    """Lista (nome, cabeca, cauda, pai) em pose A, ja com os dois lados."""
    fj = finger_joints()
    bones = [
        ("Hips", V((0.0, 0.004, 0.905)), V((0.0, 0.004, 1.000)), None),
        ("Spine", V((0.0, 0.004, 1.000)), V((0.0, 0.000, 1.120)), "Hips"),
        ("Spine1", V((0.0, 0.000, 1.120)), V((0.0, 0.000, 1.260)), "Spine"),
        ("Spine2", V((0.0, 0.000, 1.260)), NECK_BASE, "Spine1"),
        ("Neck", NECK_BASE, HEAD_BASE, "Spine2"),
        ("Head", HEAD_BASE, V((0.0, 0.016, 1.760)), "Neck"),
    ]
    for side, pre in ((1, "Left"), (-1, "Right")):
        def m(v, s=side):
            return V((v.x * s, v.y, v.z))

        bones += [
            (pre + "Shoulder", m(V((0.025, 0.004, 1.405))), m(S), "Spine2"),
            (pre + "Arm", m(S), m(E), pre + "Shoulder"),
            (pre + "ForeArm", m(E), m(WR), pre + "Arm"),
            (pre + "Hand", m(WR), m(hand_point(FINGER_BASE_L, 0.009, -0.002)), pre + "ForeArm"),
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


BONE_RADIUS = {"Hips": 0.16, "Spine": 0.16, "Spine1": 0.17, "Spine2": 0.17, "Neck": 0.06, "Head": 0.12,
               "Shoulder": 0.09, "Arm": 0.065, "ForeArm": 0.055, "Hand": 0.045, "UpLeg": 0.09, "Leg": 0.065,
               "Foot": 0.05, "ToeBase": 0.04}
PART_BONES = {
    # "*" = osso do lado do vertice (Left/Right)
    "head": ["Head"],
    "hair": ["Head"],
    "neck": ["Neck", "Head", "Spine2"],
    "tee": ["Spine2", "Neck"],
    "hoodie": ["Hips", "Spine", "Spine1", "Spine2", "*Shoulder", "*Arm"],
    "hood": ["Spine2", "Neck", "*Shoulder"],
    "pocket": ["Hips", "Spine", "Spine1"],
    "drawstring": ["Spine2", "Spine1"],
    "sleeve": ["Spine2", "*Shoulder", "*Arm", "*ForeArm"],
    "hand": ["*ForeArm", "*Hand"] + ["*" + b for b in FINGER_BONES],
    "watch": ["*ForeArm"],
    "pelvis": ["Hips", "Spine", "*UpLeg"],
    "jeans": ["Hips", "*UpLeg", "*Leg", "*Foot"],
    "shoe": ["*Foot", "*ToeBase"],
    "backpack": ["Spine2"],
    "strap": ["Spine1", "Spine2", "*Shoulder"],
}


def _bone_radius(name):
    base = name.replace("Left", "").replace("Right", "")
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


def _apply_modifiers(ob):
    bpy.context.view_layer.update()
    dg = bpy.context.evaluated_depsgraph_get()
    me = bpy.data.meshes.new_from_object(ob.evaluated_get(dg))
    old = ob.data
    ob.modifiers.clear()
    ob.data = me
    me.name = ob.name
    if old.users == 0:
        bpy.data.meshes.remove(old)


def _skin_part(ob, segs):
    part = ob["dreamscape_part"]
    side = ob["dreamscape_side"]
    rule = PART_BONES[part]
    groups = {}
    for v in ob.data.vertices:
        s = side if side else (1 if v.co.x > 0.002 else (-1 if v.co.x < -0.002 else 0))
        names = []
        for r in rule:
            if r.startswith("*"):
                pres = {1: ["Left"], -1: ["Right"], 0: ["Left", "Right"]}[s]
                names += [p + r[1:] for p in pres]
            else:
                names.append(r)
        for n, w in _weights(v.co, names, segs):
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
    """Aplica modificadores, calcula pesos, junta tudo num unico mesh e cria a armature."""
    bones = skeleton()
    segs = {n: (h, t) for n, h, t, _ in bones}
    for ob in parts:
        _apply_modifiers(ob)
        _skin_part(ob, segs)

    main = collection()
    arm_data = bpy.data.armatures.new("ProtagonistRig")
    arm = bpy.data.objects.new("ProtagonistRig", arm_data)
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

    target = next(o for o in parts if o.name == "HoodieBody")
    _select_only(parts, target)
    bpy.ops.object.join()
    body = target
    body.name = "Protagonist"
    body.data.name = "Protagonist"
    for c in list(body.users_collection):
        c.objects.unlink(body)
    main.objects.link(body)
    for k in ("dreamscape_part", "dreamscape_side"):
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
        bpy.data.collections.remove(c)
    for me in list(bpy.data.meshes):
        if me.users == 0:
            bpy.data.meshes.remove(me)


def build():
    clear()
    build_materials()
    head = build_head()
    neck = build_neck()
    ears = build_ears(head)
    nose = build_nose(head)
    face = build_face(head, nose)
    cap = build_hair_cap(head)
    hair = build_hair(head)
    m = (Matrix.Translation(HEAD_PIVOT - V((0.0, 0.0, HEAD_DROP))) @ Matrix.Scale(HEAD_SCALE, 4)
         @ Matrix.Translation(-HEAD_PIVOT))
    for ob in [head, nose, face, cap, hair] + ears:
        ob.data.transform(m)
    tee = build_tee()
    body = build_hoodie_body()
    hood = build_hood()
    pocket = build_pocket(body)
    strings = build_drawstrings(body)
    sleeves = build_sleeve_left()
    hands = build_hand_left()
    watch = build_watch()
    pelvis = build_pelvis()
    legs = build_leg_left()
    shoes = build_shoe_left()
    pack = build_backpack(body, sleeves, hood)
    notebook = build_notebook()
    bpy.context.view_layer.update()
    objs = [head, neck, nose, face, cap, hair, tee, body, hood, pocket, strings, watch, pelvis] + ears + sleeves \
        + hands + legs + shoes + pack
    return objs, notebook
