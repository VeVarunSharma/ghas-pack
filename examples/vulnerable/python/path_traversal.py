"""
Path Traversal — CWE-22
https://cwe.mitre.org/data/definitions/22.html

⚠️  DELIBERATELY VULNERABLE — Do not use in production.

This file demonstrates path traversal in a Flask application where a
user-controlled filename is used to construct a file path without
validation.  An attacker can supply:

    GET /download?file=../../../etc/passwd

to read arbitrary files on the server.

Detected by: CodeQL (python/path-injection)
Fixed version: ../../secure/python/path_traversal.py
"""

import os
from flask import Flask, request, send_file, jsonify

app = Flask(__name__)

UPLOAD_DIR = "/uploads"


# BAD: User-controlled file path without validation
@app.route("/download")
def download():
    filename = request.args.get("file", "")
    file_path = os.path.join(UPLOAD_DIR, filename)
    return send_file(file_path)


# BAD: User input used to construct path with string formatting
@app.route("/view")
def view_file():
    filename = request.args.get("file", "")
    file_path = f"/uploads/{filename}"

    with open(file_path, "r") as f:
        content = f.read()

    return jsonify({"filename": filename, "content": content})


# BAD: User input in directory listing
@app.route("/list")
def list_files():
    directory = request.args.get("dir", "")
    full_path = os.path.join(UPLOAD_DIR, directory)

    files = os.listdir(full_path)
    return jsonify({"directory": directory, "files": files})


if __name__ == "__main__":
    app.run(debug=True, port=5000)
