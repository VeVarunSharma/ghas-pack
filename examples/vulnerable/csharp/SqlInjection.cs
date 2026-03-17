/**
 * DELIBERATELY VULNERABLE CODE — FOR EDUCATIONAL / GHAS TESTING PURPOSES ONLY
 *
 * Vulnerability : SQL Injection
 * CWE           : CWE-89  (Improper Neutralization of Special Elements used in an SQL Command)
 * CodeQL query  : cs/sql-injection
 *
 * Description:
 *   User-supplied query-string values are concatenated directly into a
 *   SQL command string.  An attacker can terminate the intended query and
 *   inject arbitrary SQL (e.g., ' OR '1'='1'; DROP TABLE Users; --).
 *
 * Remediation:
 *   Use parameterized queries (SqlCommand.Parameters.AddWithValue) or an
 *   ORM such as Entity Framework with LINQ queries.
 */

using System;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;

namespace GhasPack.Vulnerable.CSharp
{
    [ApiController]
    [Route("api/[controller]")]
    public class SqlInjectionController : ControllerBase
    {
        private const string ConnectionString =
            "Server=localhost;Database=AppDb;Trusted_Connection=True;";

        // GET /api/sqlinjection?name=alice
        [HttpGet]
        public IActionResult Search([FromQuery] string name)
        {
            // BAD: user input is taken directly from the query string
            string query = "SELECT * FROM Users WHERE Name = '" + name + "'";

            try
            {
                using var connection = new SqlConnection(ConnectionString);
                connection.Open();

                // BAD: tainted query string passed to SqlCommand
                using var cmd = new SqlCommand(query, connection);
                using var reader = cmd.ExecuteReader();

                var results = new System.Collections.Generic.List<string>();
                while (reader.Read())
                {
                    results.Add($"{reader["Name"]} — {reader["Email"]}");
                }

                return Ok(results);
            }
            catch (SqlException ex)
            {
                // BAD: internal error details returned to the caller
                return StatusCode(500, ex.Message);
            }
        }
    }
}
