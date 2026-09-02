using Android.App;
using Android.Runtime;
using Avalonia.Android;
using System;

namespace UnifyDemo.App.Android;

[Application]
public class MainApplication : AvaloniaAndroidApplication<App>
{
  public MainApplication(IntPtr handle, JniHandleOwnership transfer)
    : base(handle, transfer) { }
}
