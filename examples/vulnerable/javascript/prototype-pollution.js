/**
 * Prototype Pollution — CWE-1321
 * https://cwe.mitre.org/data/definitions/1321.html
 *
 * ⚠️  DELIBERATELY VULNERABLE — Do not use in production.
 *
 * This file demonstrates prototype pollution through an unsafe recursive
 * merge function that does not guard against special properties like
 * __proto__, constructor, or prototype.
 *
 * An attacker who controls the source object can inject properties into
 * Object.prototype, affecting every object in the application:
 *
 *   // Attacker-controlled payload
 *   merge({}, JSON.parse('{"__proto__": {"isAdmin": true}}'));
 *
 *   // Now every object inherits isAdmin
 *   const user = {};
 *   console.log(user.isAdmin); // true  ← privilege escalation!
 *
 * Detected by: CodeQL (javascript/prototype-polluting-assignment)
 * Fixed version: ../../../secure/javascript/prototype-pollution.js
 */

const express = require("express");

const app = express();
app.use(express.json());

// BAD: Recursively merging without checking __proto__, constructor, or prototype
function merge(target, source) {
  for (let key in source) {
    if (typeof source[key] === "object" && source[key] !== null) {
      if (!target[key]) {
        target[key] = {};
      }
      merge(target[key], source[key]);
    } else {
      target[key] = source[key];
    }
  }
  return target;
}

// Default application configuration
const defaultConfig = {
  theme: "light",
  language: "en",
  notifications: true,
};

// BAD: Merging untrusted user input into application config
app.post("/api/settings", (req, res) => {
  const userSettings = req.body;
  const config = merge({}, defaultConfig);
  merge(config, userSettings);

  res.json({ message: "Settings updated", config });
});

// Demonstration of the impact
app.get("/api/admin", (req, res) => {
  const user = {};
  // After prototype pollution, user.isAdmin is true even though
  // it was never explicitly set on this object.
  if (user.isAdmin) {
    res.json({ message: "Welcome, admin!" });
  } else {
    res.status(403).json({ message: "Access denied" });
  }
});

app.listen(3000, () => {
  console.log("Server running on port 3000");
});

/*
 * Attack reproduction:
 *
 * curl -X POST http://localhost:3000/api/settings \
 *   -H "Content-Type: application/json" \
 *   -d '{"__proto__": {"isAdmin": true}}'
 *
 * curl http://localhost:3000/api/admin
 * # => {"message": "Welcome, admin!"}
 */
