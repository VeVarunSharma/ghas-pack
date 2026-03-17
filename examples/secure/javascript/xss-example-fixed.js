/**
 * ✅ SECURE — Reflected XSS Fix (CWE-79)
 * https://cwe.mitre.org/data/definitions/79.html
 *
 * Principle: Encode all untrusted output before inserting it into HTML.
 *
 * This file is the fixed version of:
 *   ../../../vulnerable/javascript/xss-example.js
 *
 * What changed and why:
 *   1. Added an escapeHtml() helper that converts the five dangerous
 *      characters (&, <, >, ", ') into their HTML entity equivalents.
 *      This prevents a browser from interpreting user input as markup or
 *      script.
 *   2. Every place that injects user input into the HTML response now
 *      calls escapeHtml() first.
 *   3. The redirect endpoint validates that the URL starts with "/" or
 *      "https://" to block javascript: and data: URI attacks.
 *   4. Content-Type is set explicitly so the browser does not sniff.
 *
 * Detected by: CodeQL (javascript/reflected-xss)
 */

const express = require("express");

const app = express();

/**
 * Escape the five HTML-special characters so user input is rendered as
 * visible text, not as executable markup.
 *
 * Why these five characters?
 *   &  — starts an HTML entity
 *   <  — opens a tag
 *   >  — closes a tag
 *   "  — breaks out of a double-quoted attribute
 *   '  — breaks out of a single-quoted attribute
 */
function escapeHtml(str) {
  if (typeof str !== "string") return "";
  return str
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;")
    .replace(/'/g, "&#39;");
}

// GOOD: User input is HTML-encoded before being placed in the response.
app.get("/search", (req, res) => {
  const query = req.query.q;

  // Input validation: reject missing query.
  if (!query || typeof query !== "string") {
    return res.status(400).send("Missing search query");
  }

  const safeQuery = escapeHtml(query);

  res.setHeader("Content-Type", "text/html; charset=utf-8");
  res.send(`
    <html>
      <head><title>Search Results</title></head>
      <body>
        <h1>Search results for: ${safeQuery}</h1>
        <p>No results found.</p>
      </body>
    </html>
  `);
});

// GOOD: Username is HTML-encoded before rendering.
app.get("/profile", (req, res) => {
  const username = req.query.user;

  if (!username || typeof username !== "string") {
    return res.status(400).send("Missing user parameter");
  }

  const safeUsername = escapeHtml(username);

  res.setHeader("Content-Type", "text/html; charset=utf-8");
  res.send(`
    <html>
      <body>
        <h1>Profile</h1>
        <p>Welcome, ${safeUsername}!</p>
      </body>
    </html>
  `);
});

// GOOD: URL is validated against an allowlist of safe schemes before
// being placed in an href attribute, AND is HTML-encoded to prevent
// attribute breakout.
app.get("/redirect", (req, res) => {
  const url = req.query.url;

  if (!url || typeof url !== "string") {
    return res.status(400).send("Missing url parameter");
  }

  // Only allow relative paths and https:// URLs — block javascript:,
  // data:, vbscript:, and other dangerous schemes.
  const isRelative = url.startsWith("/") && !url.startsWith("//");
  const isHttps = url.startsWith("https://");
  if (!isRelative && !isHttps) {
    return res.status(400).send("Invalid redirect URL");
  }

  const safeUrl = escapeHtml(url);

  res.setHeader("Content-Type", "text/html; charset=utf-8");
  res.send(`
    <html>
      <body>
        <a href="${safeUrl}">Click here to continue</a>
      </body>
    </html>
  `);
});

app.listen(3000, () => {
  console.log("Server running on port 3000");
});
