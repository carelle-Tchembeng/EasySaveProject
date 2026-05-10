using EasySave.ConsoleApp.Localization;
using EasySave.ConsoleApp.Views;
using EasySave.Core.Services;
using EasySave.Infrastructure.Configuration;

namespace EasySave.ConsoleApp.Controllers
{
    /// <summary>
    /// Handles user actions from the interactive console menu.
    ///
    /// This controller coordinates:
    /// - the console view
    /// - JobManager
    /// - BackupService
    /// - application settings
    ///
    /// It does not directly perform file copy operations.
    /// </summary>
    public class MenuController
    {
        private readonly JobManager _jobManager;
        private readonly BackupService _backupService;
        private readonly IConsoleView _view;
        private readonly ILocalizer _localizer;
        private readonly AppSettings _settings;

        public MenuController(
            JobManager jobManager,
            BackupService backupService,
            IConsoleView view,
            ILocalizer localizer,
            AppSettings settings)
        {
            _jobManager = jobManager;
            _backupService = backupService;
            _view = view;
            _localizer = localizer;
            _settings = settings;
        }

        /// <summary>
        /// Displays all configured jobs.
        /// </summary>
        public void HandleListJobs()
        {
            var jobs = _jobManager.GetAll();

            _view.ShowJobList(jobs);
            _view.WaitForEnter();
        }

        /// <summary>
        /// Creates a new backup job.
        /// </summary>
        public void HandleCreateJob()
        {
            if (_jobManager.MaxJobsReached())
            {
                _view.ShowError(_localizer.Get("error.max.jobs"));
                _view.WaitForEnter();
                return;
            }

            var form = _view.PromptJobForm();

            try
            {
                _jobManager.Add(
                    form.Name,
                    form.SourcePath,
                    form.TargetPath,
                    form.Type);

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
        /// Edits an existing backup job.
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
                _jobManager.Update(
                    index,
                    form.Name,
                    form.SourcePath,
                    form.TargetPath,
                    form.Type);

                _view.ShowSuccess(_localizer.Get("success.job.updated"));
            }
            catch (ArgumentException)
            {
                _view.ShowError(_localizer.Get("error.path.invalid"));
            }

            _view.WaitForEnter();
        }

        /// <summary>
        /// Deletes a selected backup job after confirmation.
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

            int index = _view.PromptJobIndex(_jobManager.Count);
            var job = _jobManager.GetByIndex(index);

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
        /// Executes a single selected backup job.
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
            var job = _jobManager.GetByIndex(index);

            Console.WriteLine();
            Console.WriteLine(string.Format(
                _localizer.Get("exec.starting"),
                job.Name));

            try
            {
                _backupService.ExecuteOne(index);

                _view.ShowSuccess(string.Format(
                    _localizer.Get("exec.done"),
                    job.Name));
            }
            catch (Exception ex)
            {
                _view.ShowError(string.Format(
                    _localizer.Get("exec.error"),
                    ex.Message));
            }

            _view.WaitForEnter();
        }

        /// <summary>
        /// Executes all configured jobs sequentially.
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
                _localizer.Get("exec.all.starting"),
                _jobManager.Count));

            try
            {
                _backupService.ExecuteAll();

                _view.ShowSuccess(_localizer.Get("exec.all.done"));
            }
            catch (Exception ex)
            {
                _view.ShowError(string.Format(
                    _localizer.Get("exec.error"),
                    ex.Message));
            }

            _view.WaitForEnter();
        }

        /// <summary>
        /// Handles the general settings menu.
        ///
        /// In v1.1, the key setting is the daily log format:
        /// JSON or XML.
        /// </summary>
        public void HandleSettings()
        {
            string selectedFormat = _view.PromptLogFormat(_settings.LogFormat);

            _settings.SetLogFormat(selectedFormat);

            _view.ShowSuccess(string.Format(
                _localizer.Get("settings.log.format.updated"),
                _settings.LogFormat));

            _view.WaitForEnter();
        }
    }
}
