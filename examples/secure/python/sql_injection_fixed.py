"""
✅ SECURE — SQL Injection Fix (CWE-89)
https://cwe.mitre.org/data/definitions/89.html

Principle: Use parameterized queries to separate code from data.

This file is the fixed version of:
    ../../vulnerable/python/sql_injection.py

What changed and why:
    1. Replaced f-strings, .format(), and %-formatting in SQL statements
       with parameterized placeholders ("?").  The database driver sends
       the query template and the values separately, so the database
       engine never interprets user data as SQL syntax.
    2. Added input validation to reject empty or oversized values before
       they reach the database layer.

Detected by: CodeQL (python/sql-injection)
"""

import sqlite3
from flask import Flask, request, jsonify

app = Flask(__name__)

DATABASE = "myapp.db"


def get_db():
    db = sqlite3.connect(DATABASE)
    db.row_factory = sqlite3.Row
    return db


def init_db():
    db = get_db()
    db.execute(
        "CREATE TABLE IF NOT EXISTS users "
        "(id INTEGER PRIMARY KEY, name TEXT, email TEXT)"
    )
    db.commit()
    db.close()


# GOOD: Parameterized query with "?" placeholder — the database driver
# handles escaping, so user input can never alter the query structure.
@app.route("/users")
def get_users():
    name = request.args.get("name", "")

    # Input validation: reject empty or oversized names.
    if not name or len(name) > 100:
        return jsonify({"error": "Invalid 'name' parameter"}), 400

    db = get_db()
    cursor = db.cursor()

    cursor.execute("SELECT * FROM users WHERE name = ?", (name,))

    results = [dict(row) for row in cursor.fetchall()]
    db.close()
    return jsonify(results)


# GOOD: Parameterized query for numeric id — cast to int for extra safety.
@app.route("/users/<user_id>")
def get_user(user_id):
    # Input validation: ensure user_id is a positive integer.
    try:
        uid = int(user_id)
        if uid <= 0:
            raise ValueError
    except ValueError:
        return jsonify({"error": "Invalid user ID"}), 400

    db = get_db()
    cursor = db.cursor()

    cursor.execute("SELECT * FROM users WHERE id = ?", (uid,))

    row = cursor.fetchone()
    db.close()
    if row:
        return jsonify(dict(row))
    return jsonify({"error": "User not found"}), 404


# GOOD: Parameterized LIKE query — use "?" for the whole pattern and
# build the wildcard pattern in Python, not inside SQL.
@app.route("/search")
def search_users():
    term = request.args.get("q", "")

    if not term or len(term) > 100:
        return jsonify({"error": "Invalid search term"}), 400

    db = get_db()
    cursor = db.cursor()

    # Build the LIKE pattern in Python, then pass it as a parameter.
    like_pattern = f"%{term}%"
    cursor.execute("SELECT * FROM users WHERE name LIKE ?", (like_pattern,))

    results = [dict(row) for row in cursor.fetchall()]
    db.close()
    return jsonify(results)


if __name__ == "__main__":
    init_db()
    app.run(debug=True, port=5000)
