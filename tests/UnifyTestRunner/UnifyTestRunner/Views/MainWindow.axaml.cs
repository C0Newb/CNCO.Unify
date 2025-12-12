using Avalonia.Controls;

namespace UnifyTestRunner.Views;

public partial class MainWindow : Window
{
  public MainWindow()
  {
    InitializeComponent();
    App.MainModel?.SetViews(viewMain, viewTestDetails);
  }
}