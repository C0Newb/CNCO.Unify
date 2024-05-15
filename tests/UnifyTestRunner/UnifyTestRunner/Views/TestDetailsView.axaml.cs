using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using UnifyTestRunner.NUnitResults;

namespace UnifyTestRunner.Views {
    public partial class TestDetailsView : UserControl {
        private static TestDetailsView? _instance;
        public static TestDetailsView Instance {
            get {
                _instance ??= new TestDetailsView();
                return _instance;
            }
        }
        public TestDetailsView() {
            InitializeComponent();
            _instance = this;
        }

        public void ShowDetails(TestCase testCase) {
            if (testCase == null) {
                ClearResults();
                MessageDialog.ShowMessage("Cannot open details page for a null test case!");
                return;
            }

            Dispatcher.UIThread.Invoke(() => {
                txtId.Text = testCase.Id;
                txtResult.Text = testCase.Result.ToString();
                txtClassName.Text = testCase.ClassName;
                txtMethodName.Text = testCase.MethodName;
                txtName.Text = testCase.Name;
                txtDuration.Text = testCase.Duration.ToString();
                txtRunState.Text = testCase.RunState.ToString();
                txtAsserts.Text = testCase.Asserts.ToString();
                txtErrorMessage.Text = testCase.Failure?.Message ?? string.Empty;
                txtErrorMessage.IsEnabled = !string.IsNullOrEmpty(testCase.Failure?.Message);
                txtStackTrace.Text = testCase.Failure?.StackTrace ?? string.Empty;
                txtStackTrace.IsEnabled = !string.IsNullOrEmpty(testCase.Failure?.StackTrace);
            });
        }

        public void ClearResults() {
            Dispatcher.UIThread.Invoke(() => {
                txtId.Text = "No results to display!!";
                txtResult.Text = txtClassName.Text = txtMethodName.Text = txtName.Text = txtDuration.Text = txtRunState.Text = txtAsserts.Text = txtErrorMessage.Text = txtStackTrace.Text = "";
            });
        }

        public void BackButton_Click(object sender, RoutedEventArgs e) {
            App.MainModel?.ShowMainView();
            ClearResults();
        }

    }
}