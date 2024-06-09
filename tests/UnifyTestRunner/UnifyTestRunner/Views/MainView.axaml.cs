using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CNCO.Unify;
using CNCO.Unify.Communications;
using CNCO.Unify.Security;
using CNCO.Unify.Storage;
using NUnit.Engine;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using UnifyTestRunner.NUnitResults;
using UnifyTestRunner.ViewModels;

namespace UnifyTestRunner.Views {
    public partial class MainView : UserControl {
        private bool _running = false;
        private object _lock = new object();
        private bool _isDoingFileAction = false;

        private readonly ITestEngine engine = TestEngineActivator.CreateInstance();
        private TestPackage? testPackage;
        private XmlNode? TestResults;

        public ObservableCollection<TestCaseViewModel> Results { get; private set; } = new();

        public MainView() {
            DataContext = new MainViewModel();
            InitializeComponent();

            ResultsGrid.ItemsSource = Results;

            new Task(() => {
                UnifyRuntime.Create("UnifyTestRunner")
                    .UseSecurityRuntime(new SecurityRuntimeConfiguration {
                        // Here you can configure the runtime.
                    })
                    .UseCommunicationsRuntime(new CommunicationsRuntimeConfiguration {
                        // .. same for the communications runtime.
                    });
            }).Start();
        }

        private void CreateTestPackage() {
            if (testPackage == null) {
                if (Platform.IsMobile())
                    engine.InternalTraceLevel = InternalTraceLevel.Off;

                string root = Platform.GetApplicationRootDirectory();
                if (Platform.IsAndroid()) {
                    root = Path.GetFullPath(".__override__", root);
                }

                string[] testFiles = Directory.GetFiles(root, "UnifyTest*.dll");
                testPackage = new TestPackage(testFiles);
            }
        }


        private async Task SaveResults() {
            if (TestResults == null)
                return;
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) {
                MessageDialog.ShowMessage("Failed to show file save dialog, topLevel null.");
                return;
            }

            // Start async operation to open the dialog.
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions {
                Title = "Save Test Results XML",
                DefaultExtension = ".xml"
            });

            if (file is not null) {
                // Open writing stream from the file.
                await using var stream = await file.OpenWriteAsync();
                using var streamWriter = new StreamWriter(stream);
                var doc = XDocument.Parse(TestResults.OuterXml);
                await streamWriter.WriteAsync(doc.ToString());
            }
        }

        private async Task LoadResults() {
            // Get top level from the current control. Alternatively, you can use Window reference instead.
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) {
                MessageDialog.ShowMessage("Failed to show file save dialog, topLevel null.");
                return;
            }

            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
                Title = "Open Test Results XML",
                AllowMultiple = false,
            });

            if (files.Count >= 1) {
                try {
                    // Open reading stream from the first file.
                    await using var stream = await files[0].OpenReadAsync();
                    using (XmlReader reader = XmlReader.Create(stream)) {
                        TestResults = new XmlDocument().ReadNode(reader);
                    }
                } catch { }
            }
        }

        private void BtnSave_Click(object? sender, RoutedEventArgs e) {
            if (_isDoingFileAction)
                return;

            new Task(async () => {
                try {
                    _isDoingFileAction = true;
                    await SaveResults();
                } finally {
                    _isDoingFileAction = false;
                }
            }).Start();
        }
        private void BtnLoad_Click(object? sender, RoutedEventArgs e) {
            if (_isDoingFileAction)
                return;

            new Task(async () => {
                try {
                    _isDoingFileAction = true;
                    await LoadResults();
                    ShowResults();
                } finally {
                    _isDoingFileAction = false;
                }
            }).Start();
        }


        private void BtnSearch_Click(object? sender, RoutedEventArgs e) {
            try {
                testPackage = null;
                CreateTestPackage();

                using (ITestRunner runner = engine.GetRunner(testPackage)) {
                    lblStatus.Content = $"Found {runner.CountTestCases(TestFilter.Empty)} tests ...";
                }
            } catch (Exception ex) {
                lblStatus.Content = ex.Message;
                txtResults.Text = ex.StackTrace;
            }
        }

        private void BtnRun_Click(object? sender, RoutedEventArgs e) {
            if (_running)
                return;

            _running = true;
            ResultsGrid.IsVisible = false;
            Results.Clear();
            txtResults.IsVisible = true;
            txtResults.Text = "...running...";
            stackControlButtons.IsEnabled = false;

            new Task(() => {
                try {
                    string totalCount = "";
                    string passedCount = "";
                    string failedCount = "";
                    string warningCount = "";
                    string skippedCount = "";
                    Dispatcher.UIThread.Invoke(() => lblStatus.Content = string.Format("{0} tests loaded. {1} Passed, {2} Warnings, {3} Failed, {4} Skipped.", totalCount, passedCount, warningCount, failedCount, skippedCount));

                    CreateTestPackage();

                    using (ITestRunner runner = engine.GetRunner(testPackage)) {
                        Dispatcher.UIThread.Invoke(() => lblStatus.Content = $"Running {runner.CountTestCases(TestFilter.Empty)} tests ...");

                        TestResults = runner.Run(listener: null, TestFilter.Empty);
                        var doc = XDocument.Parse(TestResults.OuterXml);
                        var fileStorage = new LocalFileStorage();
                        fileStorage.Write($"UnifyTestRunner.tests_results_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xml", doc.ToString());

                        ShowResults();
                    }
                } catch (Exception ex) {
                    Dispatcher.UIThread.Invoke(() => {
                        lblStatus.Content = ex.Message;
                        txtResults.Text = ex.StackTrace;
                        txtResults.IsVisible = true;
                        ResultsGrid.IsVisible = false;
                        stackControlButtons.IsEnabled = true;
                    });
                } finally {
                    _running = false;
                }
            }).Start();
        }

        private void ShowResults() {
            if (TestResults == null) {
                lblStatus.Content = "No results...";
                Results.Clear();
                ResultsGrid.IsVisible = true;
                txtResults.IsVisible = false;
                return;
            }
            string totalCount = "";
            string passedCount = "";
            string failedCount = "";
            string warningCount = "";
            string skippedCount = "";
            totalCount = TestResults.SelectSingleNode("//test-run/@total")?.Value ?? TestResults.SelectSingleNode("/@total")?.Value ?? string.Empty;
            passedCount = TestResults.SelectSingleNode("//test-run/@passed")?.Value ?? TestResults.SelectSingleNode("/@passed")?.Value ?? string.Empty;
            failedCount = TestResults.SelectSingleNode("//test-run/@failed")?.Value ?? TestResults.SelectSingleNode("/@failed")?.Value ?? string.Empty;
            warningCount = TestResults.SelectSingleNode("//test-run/@warnings")?.Value ?? TestResults.SelectSingleNode("/@warnings")?.Value ?? string.Empty;
            skippedCount = TestResults.SelectSingleNode("//test-run/@skipped")?.Value ?? TestResults.SelectSingleNode("/@skipped")?.Value ?? string.Empty;

            TestRun testRun = TestRunDeserializer.DeserializeTestRun(TestResults) ?? throw new NullReferenceException("Failed to deserialize the test results!");
            TestCase[] testCases = testRun.GetTestCases();

            Dispatcher.UIThread.Invoke(() => {
                lblStatus.Content = string.Format("{0} Tests: {1} Passed, {2} Warnings, {3} Failed, {4} Skipped.", totalCount, passedCount, warningCount, failedCount, skippedCount);

                Results.Clear();
                foreach (TestCase testCase in testCases) {
                    TestCaseViewModel testCaseViewModel = new TestCaseViewModel(testCase);
                    Results.Add(testCaseViewModel);
                }
                ResultsGrid.ItemsSource = Results;

                ResultsGrid.IsVisible = true;
                txtResults.IsVisible = false;
                stackControlButtons.IsEnabled = true;
            });
        }
    }
}