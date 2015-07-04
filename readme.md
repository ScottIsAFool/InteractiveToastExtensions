# Interactive Toast Extensions

Interactive Toast Extensions is a project to more easily allow a developer to create the xml required for interactive toasts in Windows 10.

## Nuget
Coming soon...

## What are Interactive Toasts?
Information on what they are can be found in this awesome blog post [http://blogs.msdn.com/b/tiles_and_toasts/archive/2015/07/02/adaptive-and-interactive-toast-notifications-for-windows-10.aspx](http://blogs.msdn.com/b/tiles_and_toasts/archive/2015/07/02/adaptive-and-interactive-toast-notifications-for-windows-10.aspx)

## Your code
    var toast = new InteractiveToast();
    var visual = new Visual();
    visual.AddText(new Text("Spicy Heaven"));
    visual.AddText(new Text("When do you plan to come in tomorrow?"));
    visual.AddImage(new VisualImage("ms-appx:///Assets/Deadpool.png")
    {
    ImagePlacement = ImagePlacement.AppLogoOverride
    });
    
    toast.SetVisual(visual);
    
    var input = new ToastInput("time", ToastInputType.Selection)
    {
    DefaultInput = "2"
    };
    input.AddSelection("1", "Breakfast");
    input.AddSelection("2", "Lunch");
    input.AddSelection("3", "Dinner");
    
    toast.AddActionItem(input);
    
    toast.AddActionItem(new ToastAction("Reserve", "reserve")
    {
    ActivationType = ActivationType.Foreground
    });
    toast.AddActionItem(new ToastAction("Call Restaurant", "call")
    {
    ActivationType = ActivationType.Foreground
    });
    
    var notification = toast.GetNotification();
    
    ToastNotificationManager.CreateToastNotifier().Show(notification);

## The Generated XML
        <toast>
          <visual>
            <binding template="ToastGeneric">
              <text>Spicy Heaven</text>
              <text>When do you plan to come in tomorrow?</text>
              <image placement="appLogoOverride" src="A.png" />
            </binding>
          </visual>
          <actions>
            <input id="time" type="selection" defaultInput="2" >
              <selection id="1" content="Breakfast" />
              <selection id="2" content="Lunch" />
              <selection id="3" content="Dinner" />
            </input>
            <action activationType="background" content="Reserve" arguments="reserve" />
            <action activationType="background" content="Call Restaurant" arguments="call" />
          </actions>
        </toast>

## The Resulting Toast
Desktop:

![Desktop](http://wp7nugets.files.wordpress.com/2015/07/desktopnotification_thumb.png)

Phone:

![Phone](http://wp7nugets.files.wordpress.com/2015/07/phonenotification_thumb.png)

