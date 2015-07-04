using Windows.UI.Notifications;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using InteractiveToastExtensions;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace InteractiveToastExtensionsSample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void UIElement_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var toast = new InteractiveToast();
            var visual = new Visual();
            visual.AddText(new Text("Spicy Heaven"));
            visual.AddText(new Text("When do you plan to come in tomorrow?"));
            visual.AddImage(new VisualImage("ms-appx:///Assets/Deadpool.png") { ImagePlacement = ImagePlacement.AppLogoOverride });

            toast.SetVisual(visual);

            var input = new ToastInput("time", ToastInputType.Selection) { DefaultInput = "2" };
            input.AddSelection("1", "Breakfast");
            input.AddSelection("2", "Lunch");
            input.AddSelection("3", "Dinner");

            toast.AddActionItem(input);

            toast.AddActionItem(new ToastAction("Reserve", "reserve") { ActivationType = ActivationType.Foreground });
            toast.AddActionItem(new ToastAction("Call Restaurant", "call") { ActivationType = ActivationType.Foreground });

            var notification = toast.GetNotification();

            ToastNotificationManager.CreateToastNotifier().Show(notification);
        }
    }
}
