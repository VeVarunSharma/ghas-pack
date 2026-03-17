/**
 * ✅ SECURE — Fixed Insecure Deserialization Example
 *
 * Security Principle : Avoid Native Serialization; Use Safe Data Formats
 * CWE Addressed      : CWE-502  (Deserialization of Untrusted Data)
 * CodeQL query        : java/unsafe-deserialization
 *
 * What Changed:
 *   1. Replaced Java native ObjectInputStream deserialization with JSON
 *      parsing via Jackson, which does NOT instantiate arbitrary classes.
 *   2. Incoming data is mapped to a strictly typed DTO (UserPayload),
 *      so only known, safe fields are accepted.
 *   3. Added input size limit to prevent denial-of-service via large
 *      payloads.
 *   4. If native deserialization were truly required (it rarely is),
 *      an ObjectInputFilter (Java 9+) should whitelist allowed classes.
 *
 * Compare with: examples/vulnerable/java/InsecureDeserialization.java
 */

import java.io.IOException;
import java.io.PrintWriter;

import javax.servlet.ServletException;
import javax.servlet.annotation.WebServlet;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.DeserializationFeature;

@WebServlet("/deserialize")
public class InsecureDeserializationFixed extends HttpServlet {

    // FIX: Use Jackson ObjectMapper configured for safe deserialization
    private static final ObjectMapper MAPPER = new ObjectMapper()
            // Reject unknown JSON properties to enforce a strict schema
            .configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, true);

    // Maximum allowed payload size (1 MB) to prevent DoS
    private static final int MAX_PAYLOAD_BYTES = 1_048_576;

    /**
     * A strictly typed DTO that defines exactly which fields are accepted.
     * Unlike ObjectInputStream.readObject(), Jackson will ONLY populate
     * these declared fields — no arbitrary class instantiation is possible.
     */
    public static class UserPayload {
        public String name;
        public String email;

        @Override
        public String toString() {
            return "UserPayload{name='" + name + "', email='" + email + "'}";
        }
    }

    @Override
    protected void doPost(HttpServletRequest request, HttpServletResponse response)
            throws ServletException, IOException {

        response.setContentType("text/plain");
        PrintWriter out = response.getWriter();

        // FIX #1: Enforce a content-length limit to mitigate DoS
        if (request.getContentLength() > MAX_PAYLOAD_BYTES) {
            response.setStatus(HttpServletResponse.SC_REQUEST_ENTITY_TOO_LARGE);
            out.println("Payload too large.");
            return;
        }

        try {
            // FIX #2: Deserialize the JSON request body into a known, safe DTO.
            // Jackson maps JSON keys to Java fields by name — it does NOT
            // execute arbitrary class constructors or gadget chains the way
            // ObjectInputStream.readObject() does.
            UserPayload payload = MAPPER.readValue(
                    request.getInputStream(), UserPayload.class);

            out.println("Received object of type: " + payload.getClass().getName());
            out.println("Object value: " + payload);

        } catch (Exception e) {
            // FIX #3: Generic error — do not leak internals
            response.setStatus(HttpServletResponse.SC_BAD_REQUEST);
            out.println("Invalid request payload.");
            getServletContext().log("Deserialization error", e);
        }
    }
}
