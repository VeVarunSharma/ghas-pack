/**
 * ✅ SECURE — SQL Injection Fix (CWE-89)
 * https://cwe.mitre.org/data/definitions/89.html
 *
 * Principle: Use parameterized queries to separate code from data.
 *
 * This file is the fixed version of:
 *   ../../../vulnerable/javascript/sql-injection.js
 *
 * What changed and why:
 *   1. Replaced string concatenation / template literals with placeholder
 *      parameters ("?" for mysql).  The database driver sends the query
 *      structure and the user-supplied values separately, so the database
 *      engine can never interpret data as SQL syntax.
 *   2. Added input validation so obviously bad or missing values are
 *      rejected before they ever reach the database.
 *
 * Detected by: CodeQL (javascript/sql-injection)
 */

const express = require("express");
const mysql = require("mysql");

const app = express();

const db = mysql.createConnection({
  host: "localhost",
  user: "root",
  password: "password",
  database: "myapp",
});

db.connect();

// GOOD: Parameterized query — the "?" placeholder ensures user input is
// always treated as a value, never as part of the SQL syntax.
app.get("/users", (req, res) => {
  const name = req.query.name;

  // Input validation: reject empty or missing names early.
  if (!name || typeof name !== "string" || name.length > 100) {
    return res.status(400).json({ error: "Invalid 'name' parameter" });
  }

  // The second argument is an array of values that replace each "?"
  // placeholder.  The driver escapes them automatically.
  db.query(
    "SELECT * FROM users WHERE name = ?",
    [name],
    (err, results) => {
      if (err) {
        res.status(500).json({ error: "Database error" });
        return;
      }
      res.json(results);
    }
  );
});

// GOOD: Parameterized query for numeric id — even numeric values should use
// placeholders because the driver also validates the type.
app.get("/users/:id", (req, res) => {
  const id = req.params.id;

  // Input validation: ensure id looks like a positive integer.
  if (!/^\d+$/.test(id)) {
    return res.status(400).json({ error: "Invalid user ID" });
  }

  db.query(
    "SELECT * FROM users WHERE id = ?",
    [id],
    (err, results) => {
      if (err) {
        res.status(500).json({ error: "Database error" });
        return;
      }
      res.json(results);
    }
  );
});

app.listen(3000, () => {
  console.log("Server running on port 3000");
});
