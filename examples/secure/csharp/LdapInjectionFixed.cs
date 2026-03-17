/**
 * ✅ SECURE — Fixed LDAP Injection Example (C#)
 *
 * Security Principle : Input Sanitization (RFC 4515 LDAP Filter Escaping)
 * CWE Addressed      : CWE-90  (Improper Neutralization of Special Elements used in an LDAP Query)
 * CodeQL query        : cs/ldap-injection
 *
 * What Changed:
 *   1. All LDAP special characters in user input are escaped per RFC 4515
 *      before embedding in the search filter.  Characters ( ) * \ NUL
 *      are replaced with their \HH hex equivalents so they are treated
 *      as literal values, not filter operators.
 *   2. Added input validation (null / whitespace check, length limit).
 *   3. Replaced raw exception message with a generic 500 response.
 *
 * Compare with: examples/vulnerable/csharp/LdapInjection.cs
 */

using System;
using System.Text;
using System.DirectoryServices;
using Microsoft.AspNetCore.Mvc;

namespace GhasPack.Secure.CSharp
{
    [ApiController]
    [Route("api/[controller]")]
    public class LdapInjectionFixedController : ControllerBase
    {
        private const string LdapPath = "LDAP://dc=example,dc=com";
        private const int MaxUsernameLength = 256;

        // GET /api/ldapinjectionfixed?user=jsmith
        [HttpGet]
        public IActionResult LookupUser([FromQuery] string user)
        {
            // FIX #1: Input validation — reject null, empty, or excessively long input
            if (string.IsNullOrWhiteSpace(user))
            {
                return BadRequest("Missing required parameter: user");
            }

            if (user.Length > MaxUsernameLength)
            {
                return BadRequest("Username exceeds maximum length.");
            }

            // FIX #2: Escape LDAP special characters per RFC 4515.
            // This ensures metacharacters like *, (, ), \, NUL are treated
            // as literal values and cannot alter the filter logic.
            string safeUsername = EscapeLdapFilterValue(user);

            string filter = $"(&(objectClass=user)(sAMAccountName={safeUsername}))";

            try
            {
                using var entry = new DirectoryEntry(LdapPath);
                using var searcher = new DirectorySearcher(entry)
                {
                    Filter = filter,
                    // FIX #3: Limit result count to prevent enumeration
                    SizeLimit = 10
                };

                searcher.PropertiesToLoad.Add("cn");
                searcher.PropertiesToLoad.Add("mail");
                searcher.PropertiesToLoad.Add("memberOf");

                SearchResultCollection results = searcher.FindAll();

                var users = new System.Collections.Generic.List<object>();
                foreach (SearchResult result in results)
                {
                    users.Add(new
                    {
                        Name = result.Properties["cn"]?[0]?.ToString(),
                        Email = result.Properties["mail"]?[0]?.ToString()
                    });
                }

                return Ok(users);
            }
            catch (Exception)
            {
                // FIX #4: Generic error — never expose LDAP internals to callers
                return StatusCode(500, "An internal error occurred. Please try again later.");
            }
        }

        /// <summary>
        /// Escapes LDAP filter special characters per RFC 4515 §3.
        /// Each special character is replaced with a backslash followed by
        /// its two-digit hex code:
        ///   *  → \2a     (  → \28     )  → \29
        ///   \  → \5c     NUL → \00
        /// This prevents user input from being interpreted as filter operators.
        /// </summary>
        private static string EscapeLdapFilterValue(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var sb = new StringBuilder(input.Length + 10);
            foreach (char c in input)
            {
                switch (c)
                {
                    case '\\': sb.Append("\\5c"); break;
                    case '*':  sb.Append("\\2a"); break;
                    case '(':  sb.Append("\\28"); break;
                    case ')':  sb.Append("\\29"); break;
                    case '\0': sb.Append("\\00"); break;
                    default:   sb.Append(c);      break;
                }
            }
            return sb.ToString();
        }
    }
}
