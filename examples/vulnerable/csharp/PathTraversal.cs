/**
 * DELIBERATELY VULNERABLE CODE — FOR EDUCATIONAL / GHAS TESTING PURPOSES ONLY
 *
 * Vulnerability : Path Traversal (Directory Traversal)
 * CWE           : CWE-22  (Improper Limitation of a Pathname to a Restricted Directory)
 * CodeQL query  : cs/path-injection
 *
 * Description:
 *   A user-supplied filename is combined with a base directory using
 *   Path.Combine and then read without any validation.  An attacker can
 *   supply values such as "../../etc/passwd" or "..\..\windows\win.ini"
 *   to escape the intended directory and read arbitrary files on the
 *   server's filesystem.
 *
 * Remediation:
 *   - Validate that the resolved canonical path starts with the intended
 *     base directory (Path.GetFullPath + StartsWith check).
 *   - Reject filenames that contain ".." or path separator characters.
 *   - Use a whitelist of allowed filenames where possible.
 */

using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace GhasPack.Vulnerable.CSharp
{
    [ApiController]
    [Route("api/[controller]")]
    public class PathTraversalController : ControllerBase
    {
        private const string UploadsDirectory = "/uploads";

        // GET /api/pathtraversal?file=report.pdf
        [HttpGet]
        public IActionResult DownloadFile([FromQuery] string file)
        {
            // BAD: user-controlled filename used without validation
            string filename = file;

            // BAD: Path.Combine does NOT prevent traversal sequences
            string filePath = Path.Combine(UploadsDirectory, filename);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found.");
            }

            // BAD: arbitrary file read — attacker can traverse outside /uploads
            string content = System.IO.File.ReadAllText(filePath);

            return Ok(content);
        }

        // GET /api/pathtraversal/download?file=report.pdf
        [HttpGet("download")]
        public IActionResult DownloadFileBytes([FromQuery] string file)
        {
            // BAD: same pattern, returning raw bytes
            string filePath = Path.Combine(UploadsDirectory, file);

            // BAD: no canonical-path check before reading
            byte[] data = System.IO.File.ReadAllBytes(filePath);

            return File(data, "application/octet-stream", file);
        }
    }
}
