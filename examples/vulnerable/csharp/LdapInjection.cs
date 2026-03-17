/**
 * DELIBERATELY VULNERABLE CODE — FOR EDUCATIONAL / GHAS TESTING PURPOSES ONLY
 *
 * Vulnerability : LDAP Injection
 * CWE           : CWE-90  (Improper Neutralization of Special Elements used in an LDAP Query)
 * CodeQL query  : cs/ldap-injection
 *
 * Description:
 *   User-supplied input is interpolated directly into an LDAP search
 *   filter without sanitization.  An attacker can inject LDAP filter
 *   metacharacters (e.g., "*", "(", ")") to modify the query logic,
 *   potentially enumerating all users or bypassing access controls.
 *
 *   Example malicious input:  *)(&(objectClass=*)
 *
 * Remediation:
 *   - Escape LDAP special characters in user input before embedding
 *     them in a filter (RFC 4515 escaping: \28 \29 \2a \5c \00).
 *   - Use a well-tested LDAP library that provides parameterized
 *     filter construction.
 */

using System;
using System.DirectoryServices;
using Microsoft.AspNetCore.Mvc;

namespace GhasPack.Vulnerable.CSharp
{
    [ApiController]
    [Route("api/[controller]")]
    public class LdapInjectionController : ControllerBase
    {
        private const string LdapPath = "LDAP://dc=example,dc=com";

        // GET /api/ldapinjection?user=jsmith
        [HttpGet]
        public IActionResult LookupUser([FromQuery] string user)
        {
            // BAD: user input is directly interpolated into the LDAP filter
            string username = user;
            string filter = $"(&(objectClass=user)(sAMAccountName={username}))";

            try
            {
                using var entry = new DirectoryEntry(LdapPath);
                using var searcher = new DirectorySearcher(entry)
                {
                    // BAD: tainted filter string passed to the searcher
                    Filter = filter
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
            catch (Exception ex)
            {
                // BAD: internal error details returned to the caller
                return StatusCode(500, ex.Message);
            }
        }
    }
}
