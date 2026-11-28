# Reject persistent progress storage in runtime sources while allowing certificate export.
"""Enforce the game's session-only storage boundary without launching Unity.

Runtime C# and authored Web scripts may not use durable progress storage. The
user-requested certificate export is the sole reviewed native file-write exception.
This source-level capability check complements, rather than replaces, playtests.
"""
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
CERTIFICATE = "capstone/Assets/Journey/JourneyCertificate.cs"
LEXEMES = re.compile(r'//[^\n]*|/\*[\s\S]*?\*/|@"(?:[^"]|"")*"|"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'')
DURABLE = re.compile(r"\b(?:PlayerPrefs|File|Directory|FileInfo|DirectoryInfo|FileStream|StreamReader|StreamWriter|IsolatedStorageFile|persistentDataPath|localStorage|sessionStorage|indexedDB|IDBFS)\b")
LEGACY = re.compile(r"\b(?:JourneyStorage|SavePath|SavePathOverride|AmbrosReadSave|AmbrosWriteSave|AmbrosResetSave|AmbrosInstallSaveHooks)\b")
PDF_ALLOWED = {"File", "Directory", "persistentDataPath"}
PDF_CALLS = {("File", "WriteAllBytes"), ("Directory", "Exists")}


def code_only(source):
    # Preserve line numbers, but don't treat copy, comments or PDF text as APIs.
    return LEXEMES.sub(lambda match: "\n" * match.group().count("\n"), source)


def violations(path, source):
    code = code_only(source)
    found = []
    for match in LEGACY.finditer(code):
        found.append((code.count("\n", 0, match.start()) + 1, "legacy progress-storage interface: " + match.group()))
    for match in DURABLE.finditer(code):
        if path == CERTIFICATE and match.group() in PDF_ALLOWED:
            continue
        found.append((code.count("\n", 0, match.start()) + 1, "durable-storage API: " + match.group()))
    for match in re.finditer(r"\bFS\s*\.\s*syncfs\s*\(", code):
        found.append((code.count("\n", 0, match.start()) + 1, "browser filesystem synchronization"))
    # Computed browser properties are also commonly used by JavaScript bridges.
    without_comments = re.sub(r"//[^\n]*|/\*[\s\S]*?\*/", "", source)
    if re.search(r"\b(?:window|globalThis|self)\s*\[\s*['\"](?:localStorage|sessionStorage|indexedDB)['\"]\s*\]", without_comments):
        found.append((1, "computed browser storage access"))
    if path == CERTIFICATE:
        for match in re.finditer(r"\b(File|Directory)\s*\.\s*(\w+)\s*\(", code):
            if match.groups() not in PDF_CALLS:
                found.append((code.count("\n", 0, match.start()) + 1, "certificate export exceeds reviewed file capabilities"))
        if re.search(r"\busing\s+(?:static\s+|\w+\s*=\s*)(?:global::)?(?:System\.IO\.)?(?:File|Directory)\b", code):
            found.append((1, "certificate file aliases require explicit capability review"))
    return found


def self_test():
    # Real API forms, including read-only recovery and alternate storage mechanisms.
    rejected = [
        'File.WriteAllText(path, json);', 'System.IO.File.ReadAllText(path);',
        'using Disk = System.IO.File; Disk.ReadAllBytes(path);',
        'new FileStream(path, FileMode.Create);', 'PlayerPrefs.SetInt("chapter", 2);',
        'Application.persistentDataPath;', 'localStorage.setItem("run", value);',
        'window["localStorage"].getItem("run");', 'indexedDB.open("run");',
        'FS.syncfs(false, done);', 'FS.mount(IDBFS, {}, "/run");',
        'static extern int AmbrosWriteSave(string key, string value);',
    ]
    for source in rejected:
        assert violations("capstone/Assets/Journey/Example.cs", source), source
    assert not violations("example.cs", '// File.WriteAllText(path, json);\nvar copy="localStorage is unused";')
    assert not violations("example.cs", 'var json=JsonUtility.ToJson(progress); var memory=new MemoryStream();')
    assert not violations(CERTIFICATE, 'if(!Directory.Exists(downloads))downloads=Application.persistentDataPath; File.WriteAllBytes(path,bytes);')
    assert violations(CERTIFICATE, 'File.ReadAllText(path);')
    assert not violations("download.jslib", "var url=URL.createObjectURL(new Blob([data], {type:'application/pdf'})); link.download=name;")


def main():
    self_test()
    errors = []
    count = 0
    for path in sorted((ROOT / "capstone/Assets").rglob("*")):
        if not path.is_file() or path.suffix not in {".cs", ".jslib", ".js", ".html"}:
            continue
        if "Editor" in path.relative_to(ROOT / "capstone/Assets").parts:
            continue
        relative = path.relative_to(ROOT).as_posix()
        source = path.read_text(encoding="utf-8-sig")
        count += 1
        if path.suffix == ".html":
            source = "\n".join(re.findall(r"<script\b[^>]*>([\s\S]*?)</script\s*>", source, re.IGNORECASE))
        errors.extend(f"{relative}:{line}: {message}" for line, message in violations(relative, source))
    if errors:
        raise SystemExit("FAIL: session-only storage boundary\n" + "\n".join(errors))
    print(f"PASS: {count} runtime source files; no progress-storage capabilities; PDF export remains allowed.")


if __name__ == "__main__":
    main()
