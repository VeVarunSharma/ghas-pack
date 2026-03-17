/**
 * ✅ SECURE — Fixed Path Traversal Example (C#)
 *
 * Security Principle : Input Validation + Canonical Path Verification
 * CWE Addressed      : CWE-22  (Improper Limitation of a Pathname to a Restricted Directory)
 * CodeQL query        : cs/path-injection
 *
 * What Changed:
 *   1. Strip path components with Path.GetFileName() — removes any
 *      directory traversal sequences (../, ..\ , absolute paths).
 *   2. Reject filenames containing ".." or path separators as an
 *      additional defence-in-depth check.
 *   3. Resolve the canonical (full) path and verify it starts with
 *      the allowed base directory, preventing symlink or encoding tricks.
 *
 * Compare with: examples/vulnerable/csharp/PathTraversal.cs
 */

using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace GhasPack.Secure.CSharp
{
    [ApiController]
    [Route("api/[controller]")]
    public class PathTraversalFixedController : ControllerBase
    {
        private static readonly string UploadsDirectory =
            Path.GetFullPath("/uploads");

        // GET /api/pathtraversalfixed?file=report.pdf
        [HttpGet]
        public IActionResult DownloadFile([FromQuery] string file)
        {
            // FIX #1: Reject null or empty filenames
            if (string.IsNullOrWhiteSpace(file))
            {
                return BadRequest("Missing required parameter: file");
            }

            // FIX #2: Strip directory components — Path.GetFileName returns
            // only the filename portion, discarding any leading path segments.
            // "../../etc/passwd" → "passwd", "C:\Windows\win.ini" → "win.ini"
            string sanitizedName = Path.GetFileName(file);

            // FIX #3: Defence-in-depth — reject if the original input contained
            // traversal characters.  This catches edge cases and makes the
            // security intent explicit in the code.
            if (file.Contains("..") ||
                file.Contains(Path.DirectorySeparatorChar) ||
                file.Contains(Path.AltDirectorySeparatorChar))
            {
                return BadRequest("Invalid filename.");
            }

            // FIX #4: Resolve the full canonical path and verify it is still
            // inside the allowed directory.  This protects against symlinks,
            // URL-encoded path separators, or other bypass techniques.
            string fullPath = Path.GetFullPath(
                Path.Combine(UploadsDirectory, sanitizedName));

            if (!fullPath.StartsWith(UploadsDirectory, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Access denied.");
            }

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound("File not found.");
            }

            string content = System.IO.File.ReadAllText(fullPath);
            return Ok(content);
        }

        // GET /api/pathtraversalfixed/download?file=report.pdf
        [HttpGet("download")]
        public IActionResult DownloadFileBytes([FromQuery] string file)
        {
            // Apply the same validation as above
            if (string.IsNullOrWhiteSpace(file))
            {
                return BadRequest("Missing required parameter: file");
            }

            string sanitizedName = Path.GetFileName(file);

            if (file.Contains("..") ||
                file.Contains(Path.DirectorySeparatorChar) ||
                file.Contains(Path.AltDirectorySeparatorChar))
            {
                return BadRequest("Invalid filename.");
            }

            string fullPath = Path.GetFullPath(
                Path.Combine(UploadsDirectory, sanitizedName));

            if (!fullPath.StartsWith(UploadsDirectory, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Access denied.");
            }

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound("File not found.");
            }

            byte[] data = System.IO.File.ReadAllBytes(fullPath);
            return File(data, "application/octet-stream", sanitizedName);
        }
    }
}
