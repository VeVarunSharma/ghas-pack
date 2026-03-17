/**
 * DELIBERATELY VULNERABLE CODE — FOR EDUCATIONAL / GHAS TESTING PURPOSES ONLY
 *
 * Vulnerability : Insecure Deserialization
 * CWE           : CWE-502  (Deserialization of Untrusted Data)
 * CodeQL query  : java/unsafe-deserialization
 *
 * Description:
 *   The servlet deserializes a Java object directly from the HTTP request
 *   body without any type filtering or validation.  An attacker can send
 *   a specially crafted serialized object (e.g., using ysoserial gadget
 *   chains) to achieve remote code execution on the server.
 *
 * Remediation:
 *   - Avoid Java native serialization for untrusted input entirely.
 *   - If deserialization is necessary, use an ObjectInputFilter (Java 9+)
 *     to restrict allowed classes.
 *   - Prefer safe data formats such as JSON with strict schema validation.
 */

import java.io.IOException;
import java.io.ObjectInputStream;
import java.io.PrintWriter;

import javax.servlet.ServletException;
import javax.servlet.annotation.WebServlet;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

@WebServlet("/deserialize")
public class InsecureDeserialization extends HttpServlet {

    @Override
    protected void doPost(HttpServletRequest request, HttpServletResponse response)
            throws ServletException, IOException {

        response.setContentType("text/plain");
        PrintWriter out = response.getWriter();

        try {
            // BAD: ObjectInputStream wraps the raw, untrusted HTTP request body
            ObjectInputStream ois = new ObjectInputStream(request.getInputStream());

            // BAD: readObject() with no ObjectInputFilter — any serializable
            //      class on the classpath can be instantiated
            Object obj = ois.readObject();

            out.println("Received object of type: " + obj.getClass().getName());
            out.println("Object value: " + obj.toString());

        } catch (ClassNotFoundException e) {
            out.println("Deserialization error: " + e.getMessage());
        }
    }
}
