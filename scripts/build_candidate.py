#!/usr/bin/env python3
"""Gera candidatas Windows, Linux e macOS, com versão, origem e reserva verificável."""

import argparse
import hashlib
import json
from pathlib import Path
import re
import shutil
import subprocess
import sys
import tarfile
import tempfile
import zipfile
from datetime import datetime, timezone


TARGETS = {
    "windows": {"label": "Windows-x64", "entry": "Dreamscape.exe", "archive": ".zip", "unity": "win64"},
    "linux": {"label": "Linux-x64", "entry": "Dreamscape.x86_64", "archive": ".tar.gz", "unity": "linux64"},
    "macos": {"label": "macOS-ARM64", "entry": "Dreamscape.app", "archive": ".zip", "unity": "osxuniversal"},
}


def version_name(value):
    if not re.fullmatch(r"\d+\.\d+\.\d+(?:-(?:rc|preview)\.\d+)?", value):
        raise argparse.ArgumentTypeError("Use uma versão como 0.1.0-preview.1 ou 0.1.0-rc.1.")
    return value


def checksum(path):
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def source_manifest(project):
    hashes = {}
    for folder in ("Assets", "Packages", "ProjectSettings"):
        for path in sorted((project / folder).rglob("*")):
            if path.is_file() and not any(part.endswith("~") for part in path.parts):
                hashes[path.relative_to(project).as_posix()] = checksum(path)
    return hashes


def git_output(project, *args):
    result = subprocess.run(["git", "-C", str(project), *args], capture_output=True, text=True, check=True)
    return result.stdout.strip()


def package(root, backup, version, target, metadata, notes, instructions):
    config = TARGETS[target]
    entry = root / config["entry"]
    if not entry.exists():
        raise RuntimeError(f"A build não produziu {config['entry']}; nenhum pacote foi criado.")
    if target == "macos" and not (entry / "Contents/MacOS/Dreamscape").is_file():
        raise RuntimeError("O bundle macOS não contém o executável.")
    if target != "macos":
        library = "UnityPlayer.dll" if target == "windows" else "UnityPlayer.so"
        if not (root / library).is_file() or not (root / "Dreamscape_Data").is_dir():
            raise RuntimeError("A build está incompleta: faltam a biblioteca UnityPlayer ou os dados.")
    if target == "linux" and not entry.stat().st_mode & 0o111:
        raise RuntimeError("O executável Linux não possui permissão de execução.")
    report = json.loads((root / "build-report.json").read_text())
    if report.get("result") != "Succeeded" or report.get("errors") != 0:
        raise RuntimeError("A build não foi aprovada; nenhum pacote ou reserva foi criado.")
    if report.get("version") != version:
        raise RuntimeError("A versão do relatório não corresponde à versão solicitada.")
    if report.get("platform") != target:
        raise RuntimeError("A plataforma do relatório não corresponde ao pacote solicitado.")
    if "scope" in metadata and report.get("scope") != metadata["scope"]:
        raise RuntimeError("O escopo do relatório não corresponde ao pacote solicitado.")

    archive = root.parent / f"Dreamscape-{version}-{config['label']}{config['archive']}"
    reserve = backup / archive.name
    if archive.exists() or reserve.exists():
        raise FileExistsError("Já existe um pacote ou reserva desta versão. Use outra versão.")

    (root / "LEIA-ME.md").write_text(instructions)
    (root / "NOTAS_DA_VERSAO.md").write_text(notes)
    (root / "manifest.json").write_text(json.dumps({**metadata, "build": report}, indent=2, ensure_ascii=False) + "\n")
    with tempfile.TemporaryDirectory(prefix="dreamscape-package-") as temporary:
        staged = Path(temporary) / root.name
        shutil.copytree(root, staged, symlinks=True, ignore=shutil.ignore_patterns(
            "unity-build.log", "*_BackUpThisFolder_ButDontShipItWithYourGame", "*_BurstDebugInformation_DoNotShip"))
        if target == "macos":
            subprocess.run(["ditto", "-c", "-k", "--sequesterRsrc", "--keepParent", str(staged), str(archive)], check=True)
        elif target == "linux":
            with tarfile.open(archive, "w:gz") as bundle:
                bundle.add(staged, arcname=root.name)
        else:
            with zipfile.ZipFile(archive, "w", zipfile.ZIP_DEFLATED) as bundle:
                for path in sorted(staged.rglob("*")):
                    bundle.write(path, path.relative_to(staged.parent))
    archive_hash = checksum(archive)
    backup.mkdir(parents=True, exist_ok=True)
    shutil.copy2(archive, reserve)
    if checksum(reserve) != archive_hash:
        raise RuntimeError("A cópia de reserva não passou na verificação SHA-256.")
    for path in (archive, reserve):
        path.with_suffix(path.suffix + ".sha256").write_text(f"{archive_hash}  {path.name}\n")
    return archive, reserve


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--unity", required=True, type=Path, help="Caminho do executável do Unity Editor.")
    parser.add_argument("--project", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--version", required=True, type=version_name)
    parser.add_argument("--target", choices=["all", *TARGETS], default="all")
    parser.add_argument("--scope", choices=["full", "prototype"], default="full",
                        help="full inclui toda a campanha; prototype inclui menu, fases 1-2 e encerramento.")
    parser.add_argument("--output", type=Path)
    parser.add_argument("--backup", type=Path)
    args = parser.parse_args()
    project = args.project.resolve()
    unity = args.unity.resolve()
    if not unity.is_file():
        parser.error("O executável do Unity Editor não foi encontrado.")
    targets = list(TARGETS) if args.target == "all" else [args.target]
    if "macos" in targets and sys.platform != "darwin":
        parser.error("O empacotamento macOS usa ditto e deve ser executado no macOS.")

    output = (args.output or project / "Builds/Candidates").resolve()
    backup = (args.backup or project / "Builds/Backup").resolve()
    for target in targets:
        config = TARGETS[target]
        name = f"Dreamscape-{args.version}-{config['label']}"
        if any(path.exists() for path in (output / name, output / (name + config['archive']), backup / (name + config['archive']))):
            parser.error("Esta versão já possui arquivos de saída. Use outra versão para preservar a anterior.")
    instructions = (project / "docs/EXECUCAO.md").read_text()
    notes = (project / "docs/NOTAS_CANDIDATA.md").read_text()
    metadata = {
        "version": args.version,
        "scope": args.scope,
        "createdUtc": datetime.now(timezone.utc).isoformat(),
        "commit": git_output(project, "rev-parse", "HEAD"),
        "workingTree": git_output(project, "status", "--short"),
        "sourceSha256": source_manifest(project),
        "status": "candidata para revisão; consultar NOTAS_DA_VERSAO.md",
    }
    for target in targets:
        config = TARGETS[target]
        root = output / f"Dreamscape-{args.version}-{config['label']}"
        root.mkdir(parents=True)
        print(f"Gerando {target}: {args.version}", flush=True)
        subprocess.run([
            str(unity), "-batchmode", "-nographics", "-quit", "-projectPath", str(project),
            "-buildTarget", config["unity"], "-executeMethod", "Dreamscape.EditorTools.CandidateBuild.Build",
            "-candidateOutput", str(root / config["entry"]), "-candidateVersion", args.version,
            "-candidateTarget", target, "-candidateReport", str(root / "build-report.json"),
            "-candidateScope", args.scope,
            "-logFile", str(root / "unity-build.log"),
        ], check=True)
        archive, reserve = package(root, backup, args.version, target, metadata, notes, instructions)
        print(f"Candidata: {archive}\nReserva: {reserve}\nSHA-256: {checksum(archive)}", flush=True)


if __name__ == "__main__":
    try:
        main()
    except (OSError, RuntimeError, subprocess.CalledProcessError, ValueError) as error:
        print(f"DS-22: {error}", file=sys.stderr)
        sys.exit(1)
