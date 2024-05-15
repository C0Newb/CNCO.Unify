using System.Collections.ObjectModel;

namespace UnifyTestRunner.ViewModels {
    public class MainViewModel : ViewModelBase {
        public ObservableCollection<TestCaseViewModel> Results { get; }

        public MainViewModel() {
            Results = new ObservableCollection<TestCaseViewModel> {
                new TestCaseViewModel(new NUnitResults.TestCase {
                    Name = "SampleTest(123)",
                    Result = NUnitResults.TestResult.Inconclusive,
                    Duration = 0.14,
                    FullName = "MyLibrary.SampleTest(123)",
                    Failure = new NUnitResults.TestFailure {
                        Message = string.Empty,
                        StackTrace = string.Empty
                    }
                }),
                new TestCaseViewModel(new NUnitResults.TestCase {
                    Name = "CoolTest()",
                    Result = NUnitResults.TestResult.Failed,
                    Duration = 1100,
                    FullName = "MyLibrary.CoolTest()",
                    Failure = new NUnitResults.TestFailure {
                        Message = "Cannot assert coolness.",
                        StackTrace = "Line 3."
                    }
                })
            };
        }
    }
}
