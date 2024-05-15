using System;
using UnifyTestRunner.NUnitResults;


namespace UnifyTestRunner.ViewModels {
    public class TestCaseViewModel : ViewModelBase {
        private bool _isSelected;
        private string? _name;
        private string? _result;
        private string? _duration;
        private string? _errorMessage;

        private readonly TestCase _testCase;

        public bool IsSelected {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public string? Name {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string? Result {
            get => _result;
            set => SetProperty(ref _result, value);
        }

        public string? Duration {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }

        public string? ErrorMessage {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }


        public TestCaseViewModel(TestCase testCase) {
            _testCase = testCase;

            Name = testCase.Name;
            Result = testCase.Result.ToString();
            ErrorMessage = testCase.Failure?.Message ?? string.Empty;

            if (testCase.Duration < 0.1) {
                Duration = "<0.1ms";
            } else if (testCase.Duration >= 1000) {
                Duration = $"{Math.Round(testCase.Duration / 1000, 2)}s";
            } else {
                Duration = $"{Math.Round(testCase.Duration, 2)}ms";
            }
        }

        public void ShowMoreDetails() {
            App.MainModel?.ShowTestCaseDetails(_testCase);
        }
    }
}
