import argparse
import json
from pathlib import Path
import stat
import sys
import tarfile
import tempfile
import unittest
import zipfile

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
import build_candidate as candidate


class CandidatePackageTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.base = Path(self.temporary.name)
        self.backup = self.base / "backup"
        self.version = "0.1.0-preview.1"

    def tearDown(self):
        self.temporary.cleanup()

    def build(self, target, **report_changes):
        root = self.base / "output" / f"Dreamscape-{self.version}-{candidate.TARGETS[target]['label']}"
        root.mkdir(parents=True)
        if target == "macos":
            entry = root / "Dreamscape.app/Contents/MacOS/Dreamscape"
            entry.parent.mkdir(parents=True)
            entry.write_bytes(b"test executable")
            entry.chmod(0o755)
            (entry.parent / "launcher").symlink_to("Dreamscape")
        else:
            entry = root / candidate.TARGETS[target]["entry"]
            entry.write_bytes(b"test executable")
            entry.chmod(0o755)
            (root / "Dreamscape_Data").mkdir()
            (root / "Dreamscape_Data/level0").write_bytes(b"scene data")
            library = "UnityPlayer.dll" if target == "windows" else "UnityPlayer.so"
            (root / library).write_bytes(b"test library")
        report = {"version": self.version, "platform": target, "result": "Succeeded", "errors": 0}
        report.update(report_changes)
        (root / "build-report.json").write_text(json.dumps(report))
        return root

    def package(self, root, target):
        return candidate.package(root, self.backup, self.version, target,
                                 {"commit": "test-commit"}, "known limitations", "run instructions")

    def test_versions_cannot_escape_output_directory(self):
        for value in ("../release", "0.1.0/../../other", "latest", "0.1.0 rc 1"):
            with self.subTest(value=value), self.assertRaises(argparse.ArgumentTypeError):
                candidate.version_name(value)
        self.assertEqual(candidate.version_name(self.version), self.version)

    def test_windows_bundle_contains_runtime_and_verified_backup(self):
        root = self.build("windows")
        archive, reserve = self.package(root, "windows")
        self.assertEqual(candidate.checksum(archive), candidate.checksum(reserve))
        self.assertEqual(archive.with_suffix(".zip.sha256").read_bytes(), reserve.with_suffix(".zip.sha256").read_bytes())
        with zipfile.ZipFile(archive) as bundle:
            for relative in ("Dreamscape.exe", "UnityPlayer.dll", "Dreamscape_Data/level0", "LEIA-ME.md", "manifest.json"):
                self.assertIn(f"{root.name}/{relative}", bundle.namelist())
            self.assertEqual(json.loads(bundle.read(f"{root.name}/manifest.json"))["commit"], "test-commit")

    def test_linux_archive_preserves_execution_permissions(self):
        root = self.build("linux")
        archive, reserve = self.package(root, "linux")
        with tarfile.open(archive) as bundle:
            executable = bundle.getmember(f"{root.name}/Dreamscape.x86_64")
            self.assertEqual(executable.mode & 0o111, 0o111)
        self.assertEqual(candidate.checksum(archive), candidate.checksum(reserve))

    @unittest.skipUnless(sys.platform == "darwin", "O empacotamento .app usa ditto no macOS.")
    def test_macos_archive_preserves_app_symlinks(self):
        root = self.build("macos")
        archive, _ = self.package(root, "macos")
        with zipfile.ZipFile(archive) as bundle:
            entry = bundle.getinfo(f"{root.name}/Dreamscape.app/Contents/MacOS/launcher")
            self.assertTrue(stat.S_ISLNK(entry.external_attr >> 16))

    def test_failed_build_is_not_packaged_or_copied_to_backup(self):
        root = self.build("windows", result="Failed", errors=1)
        with self.assertRaises(RuntimeError):
            self.package(root, "windows")
        self.assertFalse(self.backup.exists())
        self.assertFalse(list(root.parent.glob("*.zip")))

    def test_wrong_platform_report_is_rejected(self):
        root = self.build("windows", platform="linux")
        with self.assertRaises(RuntimeError):
            self.package(root, "windows")
        self.assertFalse(self.backup.exists())

    def test_wrong_version_report_is_rejected(self):
        root = self.build("windows", version="0.1.0-rc.2")
        with self.assertRaises(RuntimeError):
            self.package(root, "windows")

    def test_full_campaign_cannot_be_packaged_as_two_phase_prototype(self):
        root = self.build("windows", scope="full")
        with self.assertRaises(RuntimeError):
            candidate.package(root, self.backup, self.version, "windows",
                              {"scope": "prototype"}, "notes", "instructions")
        self.assertFalse(self.backup.exists())

    def test_existing_package_and_backup_are_not_overwritten(self):
        root = self.build("windows")
        archive, reserve = self.package(root, "windows")
        before = candidate.checksum(archive)
        with self.assertRaises(FileExistsError):
            self.package(root, "windows")
        self.assertEqual(candidate.checksum(archive), before)
        self.assertEqual(candidate.checksum(reserve), before)

    def test_missing_windows_runtime_is_rejected(self):
        root = self.build("windows")
        (root / "UnityPlayer.dll").unlink()
        with self.assertRaises(RuntimeError):
            self.package(root, "windows")

    def test_linux_file_without_execute_permission_is_rejected(self):
        root = self.build("linux")
        (root / "Dreamscape.x86_64").chmod(0o644)
        with self.assertRaises(RuntimeError):
            self.package(root, "linux")

    def test_diagnostic_backups_and_raw_build_log_are_not_distributed(self):
        root = self.build("windows")
        diagnostics = root / "Dreamscape_BackUpThisFolder_ButDontShipItWithYourGame"
        diagnostics.mkdir()
        (diagnostics / "debug-data").write_text("private build diagnostics")
        (root / "unity-build.log").write_text("local machine paths")
        archive, _ = self.package(root, "windows")
        with zipfile.ZipFile(archive) as bundle:
            self.assertFalse(any("ButDontShip" in name or name.endswith("unity-build.log") for name in bundle.namelist()))
        self.assertTrue(diagnostics.exists())
        self.assertTrue((root / "unity-build.log").exists())


if __name__ == "__main__":
    unittest.main()
