using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using UnifyTestRunner.ViewModels;
using UnifyTestRunner.Views;

namespace UnifyTestRunner {
    public partial class App : Application {
        public static MainWindowModel? MainModel { get; set; }

        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted() {
            MainModel ??= new MainWindowModel();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                desktop.MainWindow = new MainWindow {
                    DataContext = MainModel
                };
            } else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform) {
                singleViewPlatform.MainView = new MainWindowView {
                    DataContext = MainModel
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}