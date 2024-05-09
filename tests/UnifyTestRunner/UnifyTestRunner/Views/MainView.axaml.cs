using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CNCO.Unify;
using CNCO.Unify.Storage;
using NUnit.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace UnifyTestRunner.Views {
    public partial class MainView : UserControl {
        private bool _running = false;
        private object _lock = new object();

        private readonly ITestEngine engine = TestEngineActivator.CreateInstance();
        private TestPackage? testPackage;

        public MainView() {
            InitializeComponent();

            var _ = Runtime.Current.ApplicationId = "UnifyTestRunner";
            Runtime.Current.Initialize();
        }

        private void CreateTestPackage() {
            if (testPackage == null) {
                if (Platform.IsMobile())
                    engine.InternalTraceLevel = InternalTraceLevel.Off;

                string root = Platform.GetApplicationRootDirectory();
                if (Platform.IsAndroid()) {
                    root = Path.GetFullPath(".__override__", root);
                }

                string[] testFiles = Directory.GetFiles(root, "UnifyTests*.dll");
                testPackage = new TestPackage(testFiles);
            }
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
                txtResutls.Text = ex.StackTrace;
            }
        }

        private void BtnRun_Click(object? sender, RoutedEventArgs e) {
            if (_running)
                return;

            _running = true;
            txtResutls.Text = "...running...";

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

                        XmlNode testResults = runner.Run(listener: null, TestFilter.Empty);
                        var doc = XDocument.Parse(testResults.OuterXml);
                        var fileStorage = new LocalFileStorage();
                        fileStorage.Write($"UnifyTestRunner.tests_results_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xml", doc.ToString());

                        totalCount = testResults.SelectSingleNode("/test-run/@total")?.Value ?? string.Empty;
                        passedCount = testResults.SelectSingleNode("/test-run/@passed")?.Value ?? string.Empty;
                        failedCount = testResults.SelectSingleNode("/test-run/@failed")?.Value ?? string.Empty;
                        warningCount = testResults.SelectSingleNode("/test-run/@warnings")?.Value ?? string.Empty;
                        skippedCount = testResults.SelectSingleNode("/test-run/@skipped")?.Value ?? string.Empty;
                        Dispatcher.UIThread.Invoke(() => {
                            lblStatus.Content = string.Format("{0} Tests: {1} Passed, {2} Warnings, {3} Failed, {4} Skipped.", totalCount, passedCount, warningCount, failedCount, skippedCount);

                            // pretty print :)
                            txtResutls.Text = doc.ToString();
                        });
                    }
                } catch (Exception ex) {
                    Dispatcher.UIThread.Invoke(() => {
                        lblStatus.Content = ex.Message;
                        txtResutls.Text = ex.StackTrace;
                    });
                } finally {
                    _running = false;
                }
            }).Start();
        }
    }
}