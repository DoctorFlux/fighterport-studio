"""FighterPort modification 2026-10-17: compare imported application with pinned manifest."""
from pathlib import Path
import hashlib, json
root=Path(__file__).resolve().parents[1]
manifest=json.loads((root/'docs/upstream-manifest.json').read_text(encoding='utf-8'))
changed=[]
missing=[]
for entry in manifest['files']:
    path=root/'application'/entry['path']
    if entry['status']=='excluded':
        if path.exists(): raise SystemExit('Excluded upstream file has reappeared: '+entry['path'])
    elif not path.exists(): missing.append(entry['path'])
    elif hashlib.sha256(path.read_bytes()).hexdigest()!=entry['sha256']: changed.append(entry['path'])
print(json.dumps({'commit':manifest['commit'],'baselineEntries':len(manifest['files']),'missing':missing,'modified':changed},indent=2))
if missing: raise SystemExit(1)
