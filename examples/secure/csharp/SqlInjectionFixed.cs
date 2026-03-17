/**
 * ✅ SECURE — Fixed SQL Injection Example (C#)
 *
 * Security Principle : Parameterized Queries (SqlCommand.Parameters)
 * CWE Addressed      : CWE-89  (Improper Neutralization of Special Elements used in an SQL Command)
 * CodeQL query        : cs/sql-injection
 *
 * What Changed:
 *   1. Replaced string concatenation with a parameterized SqlCommand.
 *      Parameters.AddWithValue binds the user input as a typed value,
 *      so the database engine treats it as DATA, never as SQL syntax.
 *   2. Added input validation (null / whitespace check).
 *   3. Replaced raw exception message with a generic 500 response.
 *
 * Compare with: examples/vulnerable/csharp/SqlInjection.cs
 */

using System;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;

namespace GhasPack.Secure.CSharp
{
    [ApiController]
    [Route("api/[controller]")]
    public class SqlInjectionFixedController : ControllerBase
    {
        private const string ConnectionString =
            "Server=localhost;Database=AppDb;Trusted_Connection=True;";

        // GET /api/sqlinjectionfixed?name=alice
        [HttpGet]
        public IActionResult Search([FromQuery] string name)
        {
            // FIX #1: Input validation — reject missing or blank input
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Missing required parameter: name");
            }

            try
            {
                using var connection = new SqlConnection(ConnectionString);
                connection.Open();

                // FIX #2: Parameterized query — the @name placeholder tells
                // ADO.NET to send the query structure and the parameter value
                // to SQL Server SEPARATELY.  The server compiles the query
                // plan once, then binds the parameter as a literal string.
                // Even if 'name' contains ' OR '1'='1, it is treated as a
                // value to compare against, NOT as part of the SQL command.
                // This is WHY parameterized queries prevent SQL injection.
                using var cmd = new SqlCommand(
                    "SELECT * FROM Users WHERE Name = @name", connection);
                cmd.Parameters.AddWithValue("@name", name);

                using var reader = cmd.ExecuteReader();

                var results = new System.Collections.Generic.List<string>();
                while (reader.Read())
                {
                    results.Add($"{reader["Name"]} — {reader["Email"]}");
                }

                return Ok(results);
            }
            catch (SqlException)
            {
                // FIX #3: Generic error — never expose SQL details to callers
                return StatusCode(500, "An internal error occurred. Please try again later.");
            }
        }
    }
}
