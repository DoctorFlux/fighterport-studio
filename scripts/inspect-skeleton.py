"""Read-only local reference inspection. Outputs are restricted to ignored project folders.

Optional backend: python -m pip install --target .deps/ssbh-python --only-binary=:all: ssbh_data_py==0.9.1 numpy==2.2.6
No game data is embedded in this script or its synthetic self-test.
"""
import argparse
import hashlib
import json
from pathlib import Path
import subprocess
import sys
import tempfile

ROOT = Path(__file__).resolve().parent.parent
sys.path.insert(0, str(ROOT / '.deps/ssbh-python'))


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def skel_report(path):
    import numpy as np
    from ssbh_data_py import skel_data
    from importlib.metadata import version
    if version('ssbh_data_py') != '0.9.1' or version('numpy') != '2.2.6':
        raise ValueError('Use the pinned local backend versions')
    skel = skel_data.read_skel(str(path))
    if (skel.major_version, skel.minor_version) != (1, 0):
        raise ValueError('Only SKEL 1.0 inspection verified')
    if len(skel.bones) > 4096:
        raise ValueError('Bone quota')
    bones = []
    for i, b in enumerate(skel.bones):
        parent = b.parent_index
        if parent is not None and not 0 <= parent < len(skel.bones):
            raise ValueError('Invalid parent index')
        visited = {i}
        while parent is not None:
            if parent in visited:
                raise ValueError('Parent cycle')
            if not 0 <= parent < len(skel.bones):
                raise ValueError('Invalid ancestor index')
            visited.add(parent)
            parent = skel.bones[parent].parent_index
        world = skel.calculate_world_transform(b)
        if not np.isfinite(b.transform).all() or not np.isfinite(world).all():
            raise ValueError('Nonfinite transform')
        bones.append(dict(index=i, name=b.name, parent=b.parent_index,
                          local_transform=b.transform.tolist(), world_transform=world.tolist(),
                          billboard=b.billboard_type.name))
    return dict(backend='ssbh_data_py 0.9.1 / numpy 2.2.6', format='SKEL', version='1.0',
                scope='Native matrices as returned by backend; no canonical axis conversion or mesh/animation support claim',
                models=[dict(name=path.stem, bones=bones)])


def inspect(kind, path, output):
    if path.stat().st_size > 32 * 1024 * 1024:
        raise ValueError('Input exceeds 32 MiB quota')
    before = digest(path)
    try:
        if kind == 'ultimate':
            report = skel_report(path)
        else:
            worker = ROOT / 'src/FighterPort.NativeWorker/bin/Release/net472/FighterPort.NativeWorker.exe'
            with tempfile.TemporaryDirectory(dir=output.parent) as job:
                response = Path(job) / 'response.json'
                subprocess.run([str(worker), 'v1', 'inspect-bundle', str(path), str(response)],
                               check=True, timeout=45, capture_output=True, creationflags=subprocess.CREATE_NO_WINDOW)
                if response.stat().st_size > 32 * 1024 * 1024:
                    raise ValueError('Output quota')
                report = json.loads(response.read_text(encoding='utf-8-sig'))
    finally:
        if before != digest(path):
            raise RuntimeError('Source hash changed')
    report['source_name'] = path.name
    report['source_sha256'] = before
    report['source_unchanged'] = True
    with output.open('x', encoding='utf-8') as stream:
        json.dump(report, stream, indent=2)
    lines = [f'# {path.name} — local skeleton reference', '', report['scope'], '',
             f'Backend: {report["backend"]}', f'Source SHA-256: {before}', '']
    for model in report['models']:
        lines += [f'## {model["name"]}: {len(model["bones"])} bones', '', '| Index | Bone | Parent |', '|---|---|---|']
        by_index = {b['index']: b['name'] for b in model['bones']}
        for bone in model['bones']:
            name = bone['name'].replace('|', '\\|').replace('\n', ' ')
            parent = by_index.get(bone['parent'], 'root').replace('|', '\\|').replace('\n', ' ')
            lines.append(f'| {bone["index"]} | {name} | {parent} |')
        lines.append('')
    with output.with_suffix('.md').open('x', encoding='utf-8') as stream:
        stream.write('\n'.join(lines))
    print(json.dumps(dict(report=str(output), models=[dict(name=m['name'], bones=len(m['bones'])) for m in report['models']], unchanged=True)))


def self_test(output):
    import numpy as np
    from ssbh_data_py import skel_data
    skel = skel_data.SkelData()
    root = np.eye(4, dtype=np.float32)
    child = np.eye(4, dtype=np.float32)
    child[3, :3] = [1, 2, 3]
    skel.bones = [skel_data.BoneData('OriginalRoot', root), skel_data.BoneData('OriginalChild', child, 0)]
    native = output.parent / 'original-skeleton.nusktb'
    if native.exists():
        raise FileExistsError(native)
    skel.save(str(native))
    report = skel_report(native)
    assert [b['name'] for b in report['models'][0]['bones']] == ['OriginalRoot', 'OriginalChild']
    assert report['models'][0]['bones'][1]['parent'] == 0
    np.testing.assert_array_equal(report['models'][0]['bones'][1]['local_transform'], child)
    np.testing.assert_array_equal(report['models'][0]['bones'][1]['world_transform'], child)
    bad = output.parent / 'invalid.nusktb'
    bad.write_bytes(b'not a native skeleton')
    try:
        skel_report(bad)
    except Exception:
        pass
    else:
        raise AssertionError('Corrupt input accepted')
    inspect('ultimate', native, output)
    print('PASS: genuine original SKEL 1.0 write/read; names, parents, local/world matrices; corrupt input rejection')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('kind', choices=['brawl', 'ultimate', 'self-test'])
    parser.add_argument('input', type=Path)
    parser.add_argument('output', type=Path)
    args = parser.parse_args()
    destination = args.output.resolve()
    if not any(destination.is_relative_to(ROOT / folder) for folder in ['.local', 'artifacts']):
        parser.error('Reports must stay in ignored .local or artifacts folders')
    destination.parent.mkdir(parents=True, exist_ok=True)
    if destination.exists() or destination.with_suffix('.md').exists():
        parser.error('Choose new report filenames')
    if args.kind == 'self-test':
        self_test(destination)
    else:
        inspect(args.kind, args.input.resolve(), destination)
