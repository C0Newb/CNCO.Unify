using Avalonia.Controls;

namespace UnifyTestRunner.Views {
    public partial class MainWindowView : UserControl {

        public MainWindowView() {
            InitializeComponent();
            App.MainModel?.SetViews(viewMain, viewTestDetails);
        }
    }
}