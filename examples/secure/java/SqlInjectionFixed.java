/**
 * ✅ SECURE — Fixed SQL Injection Example
 *
 * Security Principle : Parameterized Queries (Prepared Statements)
 * CWE Addressed      : CWE-89  (Improper Neutralization of Special Elements used in an SQL Command)
 * CodeQL query        : java/sql-injection
 *
 * What Changed:
 *   1. Replaced string concatenation with PreparedStatement and parameter
 *      placeholders (?).  The JDBC driver sends the query structure and
 *      parameter values to the database SEPARATELY, so user input can
 *      never alter the SQL syntax — it is always treated as data.
 *   2. Added input validation (null / empty check) before querying.
 *   3. HTML-encoded output to prevent reflected XSS.
 *   4. Replaced raw error messages with a generic error response.
 *
 * Compare with: examples/vulnerable/java/SqlInjection.java
 */

import java.io.IOException;
import java.io.PrintWriter;
import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;

import javax.servlet.ServletException;
import javax.servlet.annotation.WebServlet;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

@WebServlet("/users")
public class SqlInjectionFixed extends HttpServlet {

    private static final String DB_URL = "jdbc:mysql://localhost:3306/appdb";
    private static final String DB_USER = "root";
    private static final String DB_PASS = "password";

    @Override
    protected void doGet(HttpServletRequest request, HttpServletResponse response)
            throws ServletException, IOException {

        String name = request.getParameter("name");

        // FIX #1: Input validation — reject null or empty input early
        if (name == null || name.trim().isEmpty()) {
            response.setStatus(HttpServletResponse.SC_BAD_REQUEST);
            response.getWriter().println("Missing required parameter: name");
            return;
        }

        response.setContentType("text/html");
        PrintWriter out = response.getWriter();

        // FIX #2: Use PreparedStatement with a parameter placeholder (?).
        // The database engine compiles the query structure first, then binds the
        // user-supplied value as a LITERAL string parameter.  Even if the input
        // contains SQL metacharacters like ' OR '1'='1, it is treated as data,
        // NOT as part of the SQL command — this is WHY prepared statements
        // prevent SQL injection.
        String sql = "SELECT * FROM users WHERE name = ?";

        try (Connection connection = DriverManager.getConnection(DB_URL, DB_USER, DB_PASS);
             PreparedStatement ps = connection.prepareStatement(sql)) {

            // FIX #3: Bind the user input as a typed parameter
            ps.setString(1, name);

            try (ResultSet rs = ps.executeQuery()) {
                out.println("<html><body><h2>Search Results</h2><ul>");
                while (rs.next()) {
                    // FIX #4: HTML-encode output to prevent reflected XSS
                    String safeName = htmlEncode(rs.getString("name"));
                    String safeEmail = htmlEncode(rs.getString("email"));
                    out.println("<li>" + safeName + " &mdash; " + safeEmail + "</li>");
                }
                out.println("</ul></body></html>");
            }

        } catch (SQLException e) {
            // FIX #5: Return a generic error message — never leak SQL details
            response.setStatus(HttpServletResponse.SC_INTERNAL_SERVER_ERROR);
            out.println("<p>An internal error occurred. Please try again later.</p>");
            // Log the real error server-side (not shown to user)
            getServletContext().log("Database error in /users", e);
        }
    }

    /** Minimal HTML entity encoding to prevent XSS in reflected output. */
    private static String htmlEncode(String input) {
        if (input == null) return "";
        return input.replace("&", "&amp;")
                     .replace("<", "&lt;")
                     .replace(">", "&gt;")
                     .replace("\"", "&quot;")
                     .replace("'", "&#x27;");
    }
}
