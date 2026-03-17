"""
✅ SECURE — Path Traversal Fix (CWE-22)
https://cwe.mitre.org/data/definitions/22.html

Principle: Canonicalize the path and verify it stays inside the allowed
           directory before any file operation.

This file is the fixed version of:
    ../../vulnerable/python/path_traversal.py

What changed and why:
    1. Resolved the requested path to its absolute, canonical form with
       os.path.realpath() — this collapses "..", symlinks, and other
       tricks an attacker might use.
    2. Verified that the canonical path starts with the canonical upload
       directory (os.path.commonpath check).  If the resolved path escapes
       the allowed directory, the request is rejected.
    3. Rejected filenames that contain path separators or are empty.

Detected by: CodeQL (python/path-injection)
"""

import os
from flask import Flask, request, send_file, jsonify

app = Flask(__name__)

UPLOAD_DIR = os.path.realpath("/uploads")


def _safe_path(base_dir: str, user_input: str) -> str | None:
    """Return the canonical path if it is inside base_dir, else None."""
    if not user_input:
        return None
    # Resolve the full path and collapse any ".." or symlink tricks.
    requested = os.path.realpath(os.path.join(base_dir, user_input))
    # Verify the resolved path is still inside the allowed directory.
    if os.path.commonpath([base_dir, requested]) != base_dir:
        return None
    return requested


# GOOD: Path is canonicalized and confined to UPLOAD_DIR before serving.
@app.route("/download")
def download():
    filename = request.args.get("file", "")

    file_path = _safe_path(UPLOAD_DIR, filename)
    if file_path is None:
        return jsonify({"error": "Invalid file path"}), 400

    if not os.path.isfile(file_path):
        return jsonify({"error": "File not found"}), 404

    return send_file(file_path)


# GOOD: Path is canonicalized and confined before reading.
@app.route("/view")
def view_file():
    filename = request.args.get("file", "")

    file_path = _safe_path(UPLOAD_DIR, filename)
    if file_path is None:
        return jsonify({"error": "Invalid file path"}), 400

    if not os.path.isfile(file_path):
        return jsonify({"error": "File not found"}), 404

    with open(file_path, "r") as f:
        content = f.read()

    return jsonify({"filename": filename, "content": content})


# GOOD: Directory path is canonicalized and confined before listing.
@app.route("/list")
def list_files():
    directory = request.args.get("dir", "")

    dir_path = _safe_path(UPLOAD_DIR, directory) if directory else UPLOAD_DIR
    if dir_path is None:
        return jsonify({"error": "Invalid directory path"}), 400

    if not os.path.isdir(dir_path):
        return jsonify({"error": "Directory not found"}), 404

    files = os.listdir(dir_path)
    return jsonify({"directory": directory, "files": files})


if __name__ == "__main__":
    app.run(debug=True, port=5000)
