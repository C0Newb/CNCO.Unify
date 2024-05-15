using UnifyTestRunner.NUnitResults;
using UnifyTestRunner.Views;

namespace UnifyTestRunner.ViewModels {
    public class MainWindowModel : ViewModelBase {
        public static MainWindowModel? Instance { get; private set; }

        private MainView? MainView;
        private TestDetailsView? TestDetailsView;

        public MainWindowModel() {
            Instance = this;
        }

        internal void SetViews(MainView mainView, TestDetailsView testDetailsView) {
            MainView = mainView;
            TestDetailsView = testDetailsView;
        }


        internal void ShowMainView() {
            MainView!.IsVisible = true;
            TestDetailsView!.IsVisible = false;
        }

        internal void ShowTestCaseDetails(TestCase testCase) {
            MainView!.IsVisible = false;
            TestDetailsView!.IsVisible = true;
            TestDetailsView.Instance?.ShowDetails(testCase);
        }
    }
}
