"""
SQL Injection — CWE-89
https://cwe.mitre.org/data/definitions/89.html

⚠️  DELIBERATELY VULNERABLE — Do not use in production.

This file demonstrates SQL injection in a Flask application where user input
from query parameters is interpolated directly into SQL statements using
f-strings.  An attacker can supply:

    GET /users?name=' OR '1'='1' --

to dump the entire users table, or use UNION-based injection to extract
data from other tables.

Detected by: CodeQL (python/sql-injection)
Fixed version: ../../secure/python/sql_injection.py
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


# BAD: f-string with user input in SQL query
@app.route("/users")
def get_users():
    name = request.args.get("name", "")
    db = get_db()
    cursor = db.cursor()

    query = f"SELECT * FROM users WHERE name = '{name}'"
    cursor.execute(query)

    results = [dict(row) for row in cursor.fetchall()]
    db.close()
    return jsonify(results)


# BAD: format() with user input in SQL query
@app.route("/users/<user_id>")
def get_user(user_id):
    db = get_db()
    cursor = db.cursor()

    query = "SELECT * FROM users WHERE id = {}".format(user_id)
    cursor.execute(query)

    row = cursor.fetchone()
    db.close()
    if row:
        return jsonify(dict(row))
    return jsonify({"error": "User not found"}), 404


# BAD: %-formatting with user input in SQL query
@app.route("/search")
def search_users():
    term = request.args.get("q", "")
    db = get_db()
    cursor = db.cursor()

    query = "SELECT * FROM users WHERE name LIKE '%%%s%%'" % term
    cursor.execute(query)

    results = [dict(row) for row in cursor.fetchall()]
    db.close()
    return jsonify(results)


if __name__ == "__main__":
    init_db()
    app.run(debug=True, port=5000)
