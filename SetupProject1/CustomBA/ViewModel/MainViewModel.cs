using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;

namespace CustomBA.ViewModel
{

    public class MainViewModel : ViewModelBase
    {
        public MainViewModel(BootstrapperApplication bootstrapper)
        {

            this.IsThinking = false;

            this.Bootstrapper = bootstrapper;
            this.Bootstrapper.ApplyComplete += this.OnApplyComplete;
            this.Bootstrapper.DetectPackageComplete += this.OnDetectPackageComplete;
            this.Bootstrapper.PlanComplete += this.OnPlanComplete;

            this.Bootstrapper.CacheAcquireProgress += (sender, args) =>
            {
                this.cacheProgress = args.OverallPercentage;
                this.Progress = (this.cacheProgress + this.executeProgress) / 2;
            };
            this.Bootstrapper.ExecuteProgress += (sender, args) =>
            {
                this.executeProgress = args.OverallPercentage;
                this.Progress = (this.cacheProgress + this.executeProgress) / 2;
            };
            this.CurrentSQLStatus = SQLStatus.exists;
        }

        #region Properties

        private bool installEnabled;
        public bool InstallEnabled
        {
            get { return installEnabled; }
            set
            {
                installEnabled = value;
                RaisePropertyChanged("InstallEnabled");
            }
        }

        private bool uninstallEnabled;
        public bool UninstallEnabled
        {
            get { return uninstallEnabled; }
            set
            {
                uninstallEnabled = value;
                RaisePropertyChanged("UninstallEnabled");
            }
        }

        private bool isThinking;
        public bool IsThinking
        {
            get { return isThinking; }
            set
            {
                isThinking = value;
                RaisePropertyChanged("IsThinking");
            }
        }

        private int progress;
        public int Progress
        {
            get { return progress; }
            set
            {
                this.progress = value;
                RaisePropertyChanged("Progress");
            }
        }

        private int cacheProgress;
        private int executeProgress;

        public BootstrapperApplication Bootstrapper { get; private set; }

        public enum SQLStatus{none =0 ,exists = 1}
        private SQLStatus currentSQLStatus;
        public SQLStatus CurrentSQLStatus {
            get { return currentSQLStatus; }
            set
            {
                this.currentSQLStatus = value;
                RaisePropertyChanged("CurrentSQLStatus");
                SetBurnVariable("CurrentSQLStatus",((int)value).ToString());
            }
        }

        #endregion //Properties

        #region Methods
        public void SetBurnVariable(string variableName, string value)
        {
            this.Bootstrapper.Engine.StringVariables[variableName] = value;
        }
        private void InstallExecute()
        {
            IsThinking = true;
            Bootstrapper.Engine.Plan(LaunchAction.Install);
        }

        private void UninstallExecute()
        {
            IsThinking = true;
            Bootstrapper.Engine.Plan(LaunchAction.Uninstall);
        }

        private void ExitExecute()
        {
            CustomBA.BootstrapperDispatcher.InvokeShutdown();
        }
        private void OnApplyComplete(object sender, ApplyCompleteEventArgs e)
        {
            IsThinking = false;
            InstallEnabled = false;
            UninstallEnabled = false;
            this.Progress = 100;
        }
        private void OnDetectPackageComplete(object sender, DetectPackageCompleteEventArgs e)
        {
            if (e.PackageId == "Main")
            {
                if (e.State == PackageState.Absent)
                    InstallEnabled = true;

                else if (e.State == PackageState.Present)
                    UninstallEnabled = true;
            }
        }

        private void OnPlanComplete(object sender, PlanCompleteEventArgs e)
        {
            if (e.Status >= 0)
                Bootstrapper.Engine.Apply(System.IntPtr.Zero);
        }

        #endregion //Methods

        #region RelayCommands

        private RelayCommand installCommand;
        public RelayCommand InstallCommand
        {
            get
            {
                if (installCommand == null)
                    installCommand = new RelayCommand(() => InstallExecute(), () => InstallEnabled == true);

                return installCommand;
            }
        }

        private RelayCommand uninstallCommand;
        public RelayCommand UninstallCommand
        {
            get
            {
                if (uninstallCommand == null)
                    uninstallCommand = new RelayCommand(() => UninstallExecute(), () => UninstallEnabled == true);

                return uninstallCommand;
            }
        }

        private RelayCommand exitCommand;
        public RelayCommand ExitCommand
        {
            get
            {
                if (exitCommand == null)
                    exitCommand = new RelayCommand(() => ExitExecute());

                return exitCommand;
            }
        }
        #endregion RelayCommands

    }
   
}
