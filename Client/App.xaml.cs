﻿using System.Threading.Tasks;
using Caliburn.Micro;
using Client.Common;
using Client.Common.Services;
using Subsonic8.BottomBar;
using Subsonic8.Framework;
using Subsonic8.Framework.Services;
using Subsonic8.Main;
using Subsonic8.Playback;
using Subsonic8.Settings;
using Subsonic8.Shell;
using Subsonic8.VideoPlayback;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MugenInjection;

namespace Subsonic8
{
    public sealed partial class App
    {
        private IShellViewModel _shellViewModel;
        private ApplicationExecutionState _previousExecutionState;
        private ISubsonicService _subsonicService;
        private CustomFrameAdapter _navigationService;
        private IToastNotificationService _toastNotificationService;
        private IStorageService _storageService;

        public App()
        {
            InitializeComponent();
        }

        protected override void Configure()
        {
            Kernel.Bind<IEventAggregator>().To<EventAggregator>().InSingletonScope();
            Kernel.Load<CommonModule>();
            Kernel.Load<ClientModule>();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _previousExecutionState = ApplicationExecutionState.Terminated;
            StartApplication();
        }

        protected async override void OnSearchActivated(SearchActivatedEventArgs args)
        {
            var frame = Window.Current.Content as Frame;
            if (frame == null)
            {
                StartApplication();
            }

            await _shellViewModel.PerformSubsonicSearch(args.QueryText);
        }

        protected override async void OnSuspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();
            deferral.Complete();
        }

        private async void StartApplication()
        {
            DisplayRootView<ShellView>();

            var shellView = GetShellView();

            RegisterNavigationService(shellView.ShellFrame);

            InstantiateRequiredSingletons();

            LoadServices();

            BindShellViewModelToView(shellView);

            await LoadSettings();

            await RestoreLastViewOrGoToMain(shellView);
        }

        private ShellView GetShellView()
        {
            return (ShellView)RootFrame.Content;
        }

        private void RegisterNavigationService(Frame shellFrame, bool treatViewAsLoaded = false)
        {
            _navigationService = new CustomFrameAdapter(shellFrame, treatViewAsLoaded);
            Kernel.Bind<INavigationService>().ToConstant(_navigationService);
        }

        private void LoadServices()
        {
            _subsonicService = Kernel.Get<ISubsonicService>();
            _toastNotificationService = Kernel.Get<IToastNotificationService>();
            _storageService = Kernel.Get<IStorageService>();
        }

        private void InstantiateRequiredSingletons()
        {
            //resolved so that they can start listening for events
            Kernel.Get<IPlaybackViewModel>();
            Kernel.Get<IFullScreenVideoPlaybackViewModel>();
            Kernel.Get<IDefaultBottomBarViewModel>();
        }

        private void BindShellViewModelToView(ShellView shellView)
        {
            _shellViewModel = Kernel.Get<IShellViewModel>();

            ViewModelBinder.Bind(_shellViewModel, shellView, null);
        }

        private async Task RestoreLastViewOrGoToMain(ShellView shellView)
        {
            if (_previousExecutionState == ApplicationExecutionState.Terminated)
            {
                await SuspensionManager.RestoreAsync();
            }

            SuspensionManager.RegisterFrame(shellView.ShellFrame, "MainFrame");

            if (shellView.ShellFrame.Content == null)
            {
                _navigationService.NavigateToViewModel<MainViewModel>();
            }
        }

        private async Task LoadSettings()
        {
            var subsonic8Configuration = await GetSubsonic8Configuration();

            _subsonicService.Configuration = subsonic8Configuration.SubsonicServiceConfiguration;

            _toastNotificationService.UseSound = subsonic8Configuration.ToastsUseSound;
        }

        private async Task<Subsonic8Configuration> GetSubsonic8Configuration()
        {
            var subsonic8Configuration = await _storageService.Load<Subsonic8Configuration>() ?? new Subsonic8Configuration();

            return subsonic8Configuration;
        }
    }
}