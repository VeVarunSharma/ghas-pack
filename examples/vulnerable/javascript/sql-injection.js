/**
 * SQL Injection — CWE-89
 * https://cwe.mitre.org/data/definitions/89.html
 *
 * ⚠️  DELIBERATELY VULNERABLE — Do not use in production.
 *
 * This file demonstrates a classic SQL injection vulnerability where user
 * input from a query parameter is concatenated directly into a SQL string.
 * An attacker can supply a crafted value such as:
 *
 *   GET /users?name=' OR '1'='1
 *
 * to bypass authentication or dump the entire users table.
 *
 * Detected by: CodeQL (javascript/sql-injection)
 * Fixed version: ../../../secure/javascript/sql-injection.js
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

// BAD: User input directly concatenated into SQL query
app.get("/users", (req, res) => {
  const name = req.query.name;
  const query = "SELECT * FROM users WHERE name = '" + name + "'";

  db.query(query, (err, results) => {
    if (err) {
      res.status(500).json({ error: "Database error" });
      return;
    }
    res.json(results);
  });
});

// BAD: Template literal is equally vulnerable
app.get("/users/:id", (req, res) => {
  const id = req.params.id;
  const query = `SELECT * FROM users WHERE id = ${id}`;

  db.query(query, (err, results) => {
    if (err) {
      res.status(500).json({ error: "Database error" });
      return;
    }
    res.json(results);
  });
});

app.listen(3000, () => {
  console.log("Server running on port 3000");
});
