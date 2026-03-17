/**
 * DELIBERATELY VULNERABLE CODE — FOR EDUCATIONAL / GHAS TESTING PURPOSES ONLY
 *
 * Vulnerability : SQL Injection
 * CWE           : CWE-89  (Improper Neutralization of Special Elements used in an SQL Command)
 * CodeQL query  : java/sql-injection
 *
 * Description:
 *   User-supplied input is concatenated directly into a SQL query string
 *   without parameterization or sanitization, allowing an attacker to
 *   alter the query logic (e.g., ' OR '1'='1' --).
 *
 * Remediation:
 *   Use PreparedStatement with parameter placeholders (?) instead of
 *   string concatenation.
 */

import java.io.IOException;
import java.io.PrintWriter;
import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Statement;

import javax.servlet.ServletException;
import javax.servlet.annotation.WebServlet;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

@WebServlet("/users")
public class SqlInjection extends HttpServlet {

    private static final String DB_URL = "jdbc:mysql://localhost:3306/appdb";
    private static final String DB_USER = "root";
    private static final String DB_PASS = "password";

    @Override
    protected void doGet(HttpServletRequest request, HttpServletResponse response)
            throws ServletException, IOException {

        // BAD: user input taken directly from the request parameter
        String name = request.getParameter("name");

        // BAD: string concatenation builds the SQL query — classic SQL injection
        String query = "SELECT * FROM users WHERE name = '" + name + "'";

        response.setContentType("text/html");
        PrintWriter out = response.getWriter();

        try (Connection connection = DriverManager.getConnection(DB_URL, DB_USER, DB_PASS);
             Statement stmt = connection.createStatement();
             ResultSet rs = stmt.executeQuery(query)) {  // BAD: executes tainted query

            out.println("<html><body><h2>Search Results</h2><ul>");
            while (rs.next()) {
                // BAD: also reflects database content back without encoding (XSS risk)
                out.println("<li>" + rs.getString("name") + " — " + rs.getString("email") + "</li>");
            }
            out.println("</ul></body></html>");

        } catch (SQLException e) {
            // BAD: leaks internal error details to the end user
            out.println("<p>Error: " + e.getMessage() + "</p>");
        }
    }
}
