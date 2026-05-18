// EasySave.LogServer/Controllers/LogController.cs
// POST /api/log  — receives a log entry from any EasySave client and appends it to the daily file

using EasySave.LogServer.DTOs;
using EasySave.LogServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace EasySave.LogServer.Controllers
{
    /// <summary>
    /// REST controller for the centralised log service.
    ///
    /// POST /api/log   — Accepts a LogEntryRemoteDto and appends it to today's daily JSON log file.
    /// GET  /api/log/status — Health check / current log file info.
    /// </summary>
    [ApiController]
    [Route("api/log")]
    public class LogController : ControllerBase
    {
        private readonly ILogStorageService       _storageService;
        private readonly ILogger<LogController>   _logger;

        public LogController(ILogStorageService storageService, ILogger<LogController> logger)
        {
            _storageService = storageService;
            _logger         = logger;
        }

        // ── POST /api/log ─────────────────────────────────────────────────

        /// <summary>
        /// Receives a log entry from an EasySave client and persists it.
        /// Returns 204 No Content on success, 400 Bad Request if the payload is invalid.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PostLog([FromBody] LogEntryRemoteDto entry)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.JobName))
                return BadRequest("Invalid log entry: JobName is required.");

            try
            {
                await _storageService.AppendLogAsync(entry);
                _logger.LogDebug("Log entry received from {Machine} — job: {Job}", entry.MachineName, entry.JobName);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist log entry from {Machine}", entry.MachineName);
                return StatusCode(500, "Failed to persist log entry.");
            }
        }

        // ── GET /api/log/status ───────────────────────────────────────────

        /// <summary>Returns the path of today's log file and its current size.</summary>
        [HttpGet("status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetStatus()
        {
            string todayFile = _storageService.GetLogFilePath(DateTime.UtcNow);
            long   sizeBytes = System.IO.File.Exists(todayFile)
                ? new System.IO.FileInfo(todayFile).Length
                : 0;

            return Ok(new
            {
                status      = "running",
                logFile     = todayFile,
                sizeBytes   = sizeBytes,
                serverTime  = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
            });
        }
    }
}
