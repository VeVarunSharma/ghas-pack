"""
✅ SECURE — OS Command Injection Fix (CWE-78)
https://cwe.mitre.org/data/definitions/78.html

Principle: Avoid the shell — use subprocess with a list of arguments and
           validate / allowlist user input.

This file is the fixed version of:
    ../../vulnerable/python/command_injection.py

What changed and why:
    1. Replaced os.system(), os.popen(), and subprocess with shell=True
       by subprocess.run() with a list of arguments and shell=False
       (the default).  When the OS receives the command as a list, each
       element is a separate argv entry — the shell never interprets
       metacharacters like ;, |, or $().
    2. Added input validation that allowlists characters valid in
       hostnames and domain names (alphanumeric, hyphens, dots).  This
       stops payloads like "127.0.0.1; cat /etc/passwd" at the front
       door.

Detected by: CodeQL (python/command-line-injection)
"""

import re
import subprocess
from flask import Flask, request, jsonify

app = Flask(__name__)

# Allowlist pattern: only characters that are valid in hostnames / domains.
HOSTNAME_RE = re.compile(r"^[a-zA-Z0-9._-]+$")


def _is_valid_hostname(value: str) -> bool:
    """Return True if value looks like a plausible hostname or IP address."""
    if not value or len(value) > 253:
        return False
    return HOSTNAME_RE.match(value) is not None


# GOOD: subprocess.run() with a list — no shell involved, so
# metacharacters in `host` cannot trigger command injection.
@app.route("/ping")
def ping():
    host = request.args.get("host", "")

    if not _is_valid_hostname(host):
        return jsonify({"error": "Invalid hostname"}), 400

    result = subprocess.run(
        ["ping", "-c", "4", host],
        capture_output=True,
        text=True,
        timeout=10,
    )
    return jsonify({
        "host": host,
        "exit_code": result.returncode,
        "output": result.stdout,
    })


# GOOD: subprocess.run() with a list for nslookup.
@app.route("/lookup")
def dns_lookup():
    domain = request.args.get("domain", "")

    if not _is_valid_hostname(domain):
        return jsonify({"error": "Invalid domain name"}), 400

    result = subprocess.run(
        ["nslookup", domain],
        capture_output=True,
        text=True,
        timeout=10,
    )
    return jsonify({"domain": domain, "result": result.stdout})


# GOOD: subprocess.run() with a list for whois.
@app.route("/whois")
def whois():
    domain = request.args.get("domain", "")

    if not _is_valid_hostname(domain):
        return jsonify({"error": "Invalid domain name"}), 400

    result = subprocess.run(
        ["whois", domain],
        capture_output=True,
        text=True,
        timeout=10,
    )
    return jsonify({"domain": domain, "result": result.stdout})


if __name__ == "__main__":
    app.run(debug=True, port=5000)
