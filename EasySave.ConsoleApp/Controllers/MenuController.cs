// EasySave.ConsoleApp/Controllers/MenuController.cs

using EasySave.ConsoleApp.Localization;
using EasySave.ConsoleApp.Views;
using EasySave.Core.Services;

namespace EasySave.ConsoleApp.Controllers
{
    /// <summary>
    /// Handles the logic for each menu option in the interactive console mode.
    /// Acts as the mediator between the view (IConsoleView) and the domain
    /// services (JobManager, BackupService).
    /// Each Handle*() method corresponds to one menu option.
    /// Contains no display logic — delegates all output to IConsoleView.
    /// </summary>
    public class MenuController
    {
        // ─────────────────────────────────────────────────────────────
        // Dependencies
        // ─────────────────────────────────────────────────────────────

        private readonly JobManager    _jobManager;
        private readonly BackupService _backupService;
        private readonly IConsoleView  _view;
        private readonly ILocalizer    _localizer;

        // ─────────────────────────────────────────────────────────────
        // Constructor
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Initializes the controller with all required dependencies.
        /// </summary>
        public MenuController(
            JobManager    jobManager,
            BackupService backupService,
            IConsoleView  view,
            ILocalizer    localizer)
        {
            _jobManager    = jobManager;
            _backupService = backupService;
            _view          = view;
            _localizer     = localizer;
        }

        // ─────────────────────────────────────────────────────────────
        // Menu option handlers
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Menu option 1 — Displays all configured backup jobs.
        /// </summary>
        public void HandleListJobs()
        {
            var jobs = _jobManager.GetAll();
            _view.ShowJobList(jobs);
            _view.WaitForEnter();
        }

        /// <summary>
        /// Menu option 2 — Creates a new backup job from user input.
        /// Validates the 5-job limit and path accessibility before saving.
        /// </summary>
        public void HandleCreateJob()
        {
            // Check limit before prompting to avoid wasted user input
            if (_jobManager.MaxJobsReached())
            {
                _view.ShowError(_localizer.Get("error.max.jobs"));
                _view.WaitForEnter();
                return;
            }

            var form = _view.PromptJobForm();

            try
            {
                _jobManager.Add(form.Name, form.SourcePath, form.TargetPath, form.Type);
                _view.ShowSuccess(_localizer.Get("success.job.created"));
            }
            catch (ArgumentException)
            {
                _view.ShowError(_localizer.Get("error.path.invalid"));
            }
            catch (InvalidOperationException)
            {
                _view.ShowError(_localizer.Get("error.max.jobs"));
            }

            _view.WaitForEnter();
        }

        /// <summary>
        /// Menu option 3 — Edits an existing backup job.
        /// Prompts for a job index, then collects updated field values.
        /// </summary>
        public void HandleEditJob()
        {
            if (_jobManager.Count == 0)
            {
                _view.ShowError(_localizer.Get("error.no.jobs"));
                _view.WaitForEnter();
                return;
            }

            _view.ShowJobList(_jobManager.GetAll());
            int index = _view.PromptJobIndex(_jobManager.Count);

            var form = _view.PromptJobForm();

            try
            {
                _jobManager.Update(index, form.Name, form.SourcePath, form.TargetPath, form.Type);
                _view.ShowSuccess(_localizer.Get("success.job.updated"));
            }
            catch (ArgumentException)
            {
                _view.ShowError(_localizer.Get("error.path.invalid"));
            }

            _view.WaitForEnter();
        }

        /// <summary>
        /// Menu option 4 — Deletes a backup job after confirmation.
        /// Re-indexes remaining jobs automatically.
        /// </summary>
        public void HandleDeleteJob()
        {
            if (_jobManager.Count == 0)
            {
                _view.ShowError(_localizer.Get("error.no.jobs"));
                _view.WaitForEnter();
                return;
            }

            _view.ShowJobList(_jobManager.GetAll());
            int index  = _view.PromptJobIndex(_jobManager.Count);
            var job    = _jobManager.GetByIndex(index);
            bool confirmed = _view.PromptConfirmDelete(job.Name);

            if (!confirmed)
            {
                _view.ShowError(_localizer.Get("error.delete.cancelled"));
                _view.WaitForEnter();
                return;
            }

            _jobManager.Delete(index);
            _view.ShowSuccess(_localizer.Get("success.job.deleted"));
            _view.WaitForEnter();
        }

        /// <summary>
        /// Menu option 5 — Executes a single backup job selected by the user.
        /// Displays real-time progress during execution.
        /// </summary>
        public void HandleExecuteOne()
        {
            if (_jobManager.Count == 0)
            {
                _view.ShowError(_localizer.Get("error.no.jobs"));
                _view.WaitForEnter();
                return;
            }

            _view.ShowJobList(_jobManager.GetAll());
            int index = _view.PromptJobIndex(_jobManager.Count);
            var job   = _jobManager.GetByIndex(index);

            Console.WriteLine();
            Console.WriteLine(string.Format(_localizer.Get("exec.starting"), job.Name));

            try
            {
                _backupService.ExecuteOne(index);
                _view.ShowSuccess(string.Format(_localizer.Get("exec.done"), job.Name));
            }
            catch (Exception ex)
            {
                _view.ShowError(string.Format(_localizer.Get("exec.error"), ex.Message));
            }

            _view.WaitForEnter();
        }

        /// <summary>
        /// Menu option 6 — Executes all configured backup jobs sequentially.
        /// </summary>
        public void HandleExecuteAll()
        {
            if (_jobManager.Count == 0)
            {
                _view.ShowError(_localizer.Get("error.no.jobs"));
                _view.WaitForEnter();
                return;
            }

            Console.WriteLine();
            Console.WriteLine(string.Format(
                _localizer.Get("exec.all.starting"), _jobManager.Count));

            try
            {
                _backupService.ExecuteAll();
                _view.ShowSuccess(_localizer.Get("exec.all.done"));
            }
            catch (Exception ex)
            {
                _view.ShowError(string.Format(_localizer.Get("exec.error"), ex.Message));
            }

            _view.WaitForEnter();
        }
    }
}
