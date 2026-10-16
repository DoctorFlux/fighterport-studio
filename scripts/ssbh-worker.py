"""Versioned local SSBH worker. No DCC, network, or game-specific fixture substitutions."""
import base64
import hashlib
import json
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parent.parent
sys.path.insert(0, str(ROOT / '.deps/ssbh-python'))
import numpy as np
import ssbh_data_py as ssbh
from importlib.metadata import version


def checked(path):
    path = Path(path).resolve()
    if path.stat().st_size > 8 * 1024 * 1024:
        raise ValueError('Native dependency exceeds 8 MiB quota')
    return path


def resource(path):
    data = checked(path).read_bytes()
    return dict(Name=path.name, Sha256=hashlib.sha256(data).hexdigest().upper(), Base64=base64.b64encode(data).decode())


def dependency(folder, name):
    if Path(name).name != name:
        raise ValueError('Dependency must be a filename in the selected model folder')
    return checked(folder / name)


def model(path):
    path = checked(path); folder = path.parent
    mod = ssbh.modl_data.read_modl(str(path))
    if (mod.major_version, mod.minor_version) != (1, 7):
        raise ValueError('NUMDLB 1.7 required')
    skel_path = dependency(folder, mod.skeleton_file_name)
    mesh_path = dependency(folder, mod.mesh_file_name)
    skel = ssbh.skel_data.read_skel(str(skel_path)); mesh = ssbh.mesh_data.read_mesh(str(mesh_path))
    if (skel.major_version, skel.minor_version) != (1, 0) or (mesh.major_version, mesh.minor_version) != (1, 10):
        raise ValueError('Declared subset is SKEL 1.0 / MESH 1.10')
    if len(skel.bones) > 512 or len(mesh.objects) > 128:
        raise ValueError('Skeleton/mesh quota')
    ordered = []
    def visit(i, stack):
        if i in ordered:
            return
        if i in stack or not 0 <= i < len(skel.bones):
            raise ValueError('Invalid skeleton hierarchy')
        parent = skel.bones[i].parent_index
        if parent is not None:
            visit(parent, stack | {i})
        ordered.append(i)
    for i in range(len(skel.bones)):
        visit(i, set())
    bones = []; worlds = []
    for i in ordered:
        b = skel.bones[i]
        if b.billboard_type.name != 'Disabled':
            raise ValueError('Billboard evaluation unsupported')
        local = np.asarray(b.transform, dtype=np.float64)
        world = np.asarray(skel.calculate_world_transform(b), dtype=np.float64)
        scales = np.linalg.norm(local[:3, :3], axis=1)
        normalized = local[:3, :3] / scales[:, None]
        if not np.allclose(normalized @ normalized.T, np.eye(3), atol=1e-4) or np.linalg.det(normalized) < 0:
            raise ValueError('Sheared/reflected rest transform unsupported')
        if not np.isfinite(world).all():
            raise ValueError('Nonfinite bind')
        bones.append(dict(Id=f'skel-{i}', Name=b.name, NativeIndex=i, Parent=-1 if b.parent_index is None else ordered.index(b.parent_index),
                          Translation=local[3, :3].tolist(), Rotation=[0, 0, 0], Scale=scales.tolist(),
                          RestMatrix=local.reshape(-1).tolist(), InverseBind=np.linalg.inv(world).reshape(-1).tolist()))
        worlds.append(world)
    index = {b['Name']: i for i, b in enumerate(bones)}
    if len(index) != len(bones):
        raise ValueError('Duplicate skeleton names')
    objects = []
    for m in mesh.objects:
        if not m.positions or not m.normals or not m.texture_coordinates:
            raise ValueError('Position/normal/UV0 required')
        positions = np.asarray(m.positions[0].data[:, :3], dtype=np.float64)
        normals = np.asarray(m.normals[0].data[:, :3], dtype=np.float64)
        count = len(positions)
        if count > 200000 or len(m.vertex_indices) > 600000:
            raise ValueError('Mesh quota')
        influences = [[] for _ in range(count)]
        for influence in m.bone_influences:
            bi = index[influence.bone_name]
            for weight in influence.vertex_weights:
                if not 0 <= weight.vertex_index < count:
                    raise ValueError('Weight vertex index')
                if weight.vertex_weight > 0:
                    influences[weight.vertex_index].append((bi, weight.vertex_weight))
        if m.parent_bone_name:
            if any(influences):
                raise ValueError('Mixed rigid parent and weighted mesh requires explicit evaluation')
            bi = index[m.parent_bone_name]; matrix = worlds[bi]
            positions = positions @ matrix[:3, :3] + matrix[3, :3]
            normals = normals @ np.linalg.inv(matrix[:3, :3]).T
            influences = [[(bi, 1.0)] for _ in range(count)]
        vertices = []
        for i, weights in enumerate(influences):
            if not weights or len(weights) > 4 or abs(sum(w for _, w in weights)-1) > .001:
                raise ValueError('Declared subset requires normalized 1-4 influences')
            n = normals[i]; length = np.linalg.norm(n)
            if length < 1e-8:
                raise ValueError('Zero normal')
            vertices.append(dict(Position=positions[i].tolist(), Normal=(n/length).tolist(), UV=m.texture_coordinates[0].data[i, :2].tolist(), Bones=[b for b, _ in weights], Weights=[w for _, w in weights]))
        attrs = {}
        for category in ['positions', 'normals', 'tangents', 'binormals', 'texture_coordinates', 'color_sets']:
            for attribute in getattr(m, category):
                attrs[category+':'+attribute.name] = attribute.data.tolist()
        labels = [e.material_label for e in mod.entries if e.mesh_object_name == m.name and e.mesh_object_subindex == m.subindex]
        if len(labels) != 1:
            raise ValueError('Missing/ambiguous material binding')
        objects.append(dict(Name=m.name, NativeSubIndex=m.subindex, MaterialId=labels[0], NativeAttributes=attrs, Vertices=vertices, Indices=m.vertex_indices.tolist()))
    paths = [path, skel_path, mesh_path] + [dependency(folder, n) for n in mod.material_file_names]
    losses = ['Ultimate neutral geometry preview: native shaders/textures, visibility/material animation and constraints are not evaluated. Exact native dependencies and extra vertex attributes retained; no model rewrite.']
    if (folder/'model.nuhlpb').exists():
        paths.append(checked(folder/'model.nuhlpb')); losses.append('NUSKTB rest hierarchy only; NUHLPB constraints retained but not evaluated.')
    textures = sorted(folder.glob('*.nutexb'))
    # Keep an explicit dependency inventory; do not silently embed hundreds of megabytes or fabricate textures.
    losses.append(f'{len(textures)} NUTEXB texture dependencies remain external in the selected model folder; no textured preview or portable texture packaging yet.')
    return dict(Name=mod.model_name or folder.name, TargetProfile='Ultimate source: NUMDLB 1.7 / MESH 1.10 / SKEL 1.0', Bones=bones, Meshes=objects, Resources=[resource(p) for p in paths], Limitations=losses)


def animation(path):
    path = checked(path); anim = ssbh.anim_data.read_anim(str(path))
    if (anim.major_version, anim.minor_version) != (2, 0) or anim.final_frame_index != int(anim.final_frame_index):
        raise ValueError('ANIM 2.0 integer final frame required')
    count = int(anim.final_frame_index)+1
    if not 1 <= count <= 100000:
        raise ValueError('Frame quota')
    tracks = []; losses = []
    for group in anim.groups:
        if group.group_type.name != 'Transform':
            losses.append(f'{group.group_type.name}: {len(group.nodes)} native nodes retained opaque; not evaluated')
            continue
        for node in group.nodes:
            if len(node.tracks) != 1:
                raise ValueError('Multiple transform tracks require explicit binding')
            t = node.tracks[0]; f = t.transform_flags
            if t.compensate_scale or any([f.override_translation, f.override_rotation, f.override_scale, f.override_compensate_scale]):
                raise ValueError('Scale compensation/override transform flags unsupported')
            if len(t.values) not in (1, count):
                raise ValueError('Unexpected transform sample count')
            tracks.append(dict(Bone=node.name, Samples=[dict(Scale=v.scale, Rotation=v.rotation, Translation=v.translation) for v in t.values]))
    return dict(Name=path.stem, NativeVersion=20, FrameCount=count, FramesPerSecond=60, Loop=False, TransformTracks=tracks, Losses=losses+['60 fps display default; loop not inferred from filename. Quaternion normalized linear interpolation matches inspected ssbh_wgpu; no inferred Euler winding.'])


def write_animation(source, destination):
    clip = json.loads(Path(source).read_text(encoding='utf-8-sig'))
    if clip.get('Curves') or clip.get('Transfer') or not clip.get('TransformTracks'):
        raise ValueError('Native writer requires explicit quaternion transform samples')
    if clip['Loop']:
        raise ValueError('Loop metadata is not encoded by this NUANMB subset')
    anim = ssbh.anim_data.AnimData(); anim.final_frame_index = clip['FrameCount']-1
    nodes = []
    for track in clip['TransformTracks']:
        values = [ssbh.anim_data.Transform(v['Scale'], v['Rotation'], v['Translation']) for v in track['Samples']]
        nodes.append(ssbh.anim_data.NodeData(track['Bone'], [ssbh.anim_data.TrackData('Transform', values=values)]))
    anim.groups = [ssbh.anim_data.GroupData(ssbh.anim_data.GroupType.Transform, nodes)]
    anim.save(destination)


if __name__ == '__main__':
    try:
        if version('ssbh_data_py') != '0.9.1' or version('numpy') != '2.2.6':
            raise ValueError('Pinned ssbh_data_py/numpy required')
        if len(sys.argv) != 5 or sys.argv[1] != 'v1':
            raise ValueError('Protocol: v1 operation input output')
        operation, source, destination = sys.argv[2:]
        if Path(destination).exists():
            raise ValueError('Output must be new')
        if operation == 'export-clip':
            write_animation(source, destination)
            print('{"protocol":1,"ok":true}')
            sys.exit(0)
        result = model(Path(source)) if operation == 'import-model' else animation(Path(source)) if operation == 'import-clip' else None
        if result is None:
            raise ValueError('Unknown operation')
        encoded = json.dumps(result, allow_nan=False).encode()
        if len(encoded) > 32*1024*1024:
            raise ValueError('Response quota')
        with open(destination, 'xb') as stream:
            stream.write(encoded)
        print('{"protocol":1,"ok":true}')
    except Exception as error:
        print(type(error).__name__+': '+str(error), file=sys.stderr)
        sys.exit(1)
