using Avalonia.Controls;
using DialogHostAvalonia;

namespace UnifyTestRunner;

public partial class MessageDialog : UserControl {
    public static MessageDialog Instance;
    public MessageDialog() {
        InitializeComponent();
    }

    public static void ShowMessage(string message) {
        Instance ??= new MessageDialog();
        Instance.mainLabel.Content = message;
        DialogHost.Show(Instance);
    }
}