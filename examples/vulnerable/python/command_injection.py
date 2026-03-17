"""
OS Command Injection — CWE-78
https://cwe.mitre.org/data/definitions/78.html

⚠️  DELIBERATELY VULNERABLE — Do not use in production.

This file demonstrates OS command injection in a Flask application where
user-supplied input is passed directly to shell commands.  An attacker can
supply:

    GET /ping?host=127.0.0.1;cat /etc/passwd

to execute arbitrary system commands on the server.

Detected by: CodeQL (python/command-line-injection)
Fixed version: ../../secure/python/command_injection.py
"""

import os
import subprocess
from flask import Flask, request, jsonify

app = Flask(__name__)


# BAD: User input passed directly to os.system()
@app.route("/ping")
def ping():
    host = request.args.get("host", "")
    exit_code = os.system(f"ping -c 4 {host}")
    return jsonify({"host": host, "exit_code": exit_code})


# BAD: User input passed to subprocess with shell=True
@app.route("/lookup")
def dns_lookup():
    domain = request.args.get("domain", "")
    result = subprocess.check_output(
        f"nslookup {domain}", shell=True, text=True
    )
    return jsonify({"domain": domain, "result": result})


# BAD: User input used in os.popen()
@app.route("/whois")
def whois():
    domain = request.args.get("domain", "")
    stream = os.popen(f"whois {domain}")
    output = stream.read()
    return jsonify({"domain": domain, "result": output})


if __name__ == "__main__":
    app.run(debug=True, port=5000)
