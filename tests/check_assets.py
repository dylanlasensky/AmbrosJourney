"""Check tracked gameplay assets without importing or modifying the Unity project."""
import hashlib
import json
from pathlib import Path
import re
import subprocess

root = Path(__file__).resolve().parents[1]
tracked = set(subprocess.check_output(["git", "ls-files", "-z"], cwd=root).decode().split("\0")) - {""}
errors = []
guids = {}
for name in sorted(tracked):
    if not name.endswith(".meta"):
        continue
    match = re.search(r"^guid: ([0-9a-f]+)$", (root / name).read_text(), re.M)
    if not match:
        errors.append(f"Missing GUID: {name}")
        continue
    guid = match[1]
    if guid in guids:
        errors.append(f"Duplicate GUID: {name} and {guids[guid]}")
    guids[guid] = name[:-5]

folders = ("SpringPrototype", "SummerLevel", "FallLevel", "WinterLevel", "SeasonPresentation", "Opening", "Journey")
gameplay = tuple(f"capstone/Assets/{folder}/" for folder in folders)
for name in sorted(tracked):
    if not name.startswith(gameplay) or name.endswith(".meta"):
        continue
    if name + ".meta" not in tracked:
        errors.append(f"Missing tracked metadata: {name}")
    if name.endswith((".prefab", ".unity")):
        text = (root / name).read_text()
        # Runtime world artwork must resolve from the checkout, not a local Library cache.
        for guid in re.findall(r"texture: \{fileID: \d+, guid: ([0-9a-f]+),", text):
            if guid not in guids or guids[guid] not in tracked:
                errors.append(f"Unresolved artwork {guid}: {name}")
        for guid in re.findall(r"m_Clip: \{fileID: \d+, guid: ([0-9a-f]+),", text):
            if guid not in guids or guids[guid] not in tracked:
                errors.append(f"Unresolved video clip {guid}: {name}")
        for guid in re.findall(r"m_SourcePrefab: \{fileID: \d+, guid: ([0-9a-f]+),", text):
            if guid not in guids:
                errors.append(f"Unresolved scene prefab {guid}: {name}")

for name in ("capstone/Packages/manifest.json", "capstone/Packages/packages-lock.json"):
    json.loads((root / name).read_text())
for name in ("SummerLevel", "FallLevel", "WinterLevel"):
    path = f"capstone/Assets/{name}/Resources/{name}.prefab"
    if path not in tracked:
        errors.append(f"Missing portal resource: {path}")
for path in re.findall(r"^\s+path: (.+)$", (root / "capstone/ProjectSettings/EditorBuildSettings.asset").read_text(), re.M):
    if "capstone/" + path not in tracked:
        errors.append(f"Missing build scene: {path}")
# These are the supplied PR18 movies, not merely whichever MP4 currently occupies
# the Resources path. Update the recorded provenance deliberately for future clips.
video_sources = {
    "Intro.mp4": "de4dba62d3607650b8157d0d7f29b027ac2574717bd896d6cd14f691ef4e14cf",
    "Outro.mp4": "e2a89f9ea854fc0293d8acc25e4b9a8961dcf5271c38d386410bd436c7494a4f",
}
for name, expected in video_sources.items():
    path = root / "capstone/Assets/Journey/Resources/Journey" / name
    if not path.is_file():
        errors.append(f"Missing teammate video: {name}")
        continue
    digest = hashlib.sha256()
    with path.open("rb") as source:
        for block in iter(lambda: source.read(1024 * 1024), b""):
            digest.update(block)
    if digest.hexdigest() != expected:
        errors.append(f"Incorrect source video: {name}; see docs/IntroVideoSource.md")
if errors:
    raise SystemExit("\n".join(errors))
print(f"PASS: {len(guids)} tracked GUIDs; gameplay metadata, artwork references, portal resources, build scenes and teammate video identity.")
