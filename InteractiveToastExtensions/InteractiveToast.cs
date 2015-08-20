/*
*
* This is all based on the blog post on MSDN
* http://blogs.msdn.com/b/tiles_and_toasts/archive/2015/07/02/adaptive-and-interactive-toast-notifications-for-windows-10.aspx
*
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace InteractiveToastExtensions
{
    public class InteractiveToast
    {
        private Visual _visual;
        private readonly List<ActionItem> _actions = new List<ActionItem>();

        /// <summary>
        /// Gets or sets the launch arguments. These are passed to the launching app 
        /// when the toast is tapped
        /// </summary>
        public string LaunchArgs { get; set; }

        /// <summary>
        /// Gets or sets the type of the activation.
        /// </summary>
        public ActivationType? ActivationType { get; set; }

        /// <summary>
        /// Gets or sets the scenario.
        /// </summary>
        public Scenario? Scenario { get; set; }

        /// <summary>
        /// Gets or sets the people. This is there, but isn't actually used
        /// </summary>
        public string People { get; set; }

        /// <summary>
        /// Gets the audio element.
        /// </summary>
        public Audio Audio { get; private set; }

        public void SetVisual(Visual visual)
        {
            _visual = visual;
        }

        public void AddAudio(string source, bool? isLoop = null, bool? isSilent = null)
        {
            Audio = new Audio(source)
            {
                IsSilent = isSilent,
                Loop = isLoop
            };
        }

        public void RemoveAudio()
        {
            Audio = null;
        }

        public void AddActionItem(ActionItem action)
        {
            _actions.Add(action);
        }

        public void RemoveActionItem(ActionItem action)
        {
            _actions?.Remove(action);
        }

        /// <summary>
        /// Gets the XML.
        /// </summary>
        /// <returns>string representation of xml</returns>
        internal string GetXml()
        {
            var sb = new StringBuilder("<toast");

            if (!string.IsNullOrEmpty(LaunchArgs))
            {
                sb.Append($" launch=\"{LaunchArgs}\"");
            }

            if (ActivationType.HasValue)
            {
                sb.Append($" activationType=\"{ActivationType.Value}\"");
            }

            if (Scenario.HasValue)
            {
                sb.Append($" scenario=\"{Scenario.Value.ToString().ToLower()}\"");
            }

            if (!string.IsNullOrEmpty(People))
            {
                sb.Append($" hint-people=\"{People}\"");
            }

            sb.Append(">");

            if (_visual != null)
            {
                sb.AppendLine(_visual.GetXml());
            }

            if (Audio != null)
            {
                sb.AppendLine(Audio.GetXml());
            }

            if (_actions != null && _actions.Any())
            {
                sb.AppendLine("<actions>");
                foreach (var action in _actions)
                {
                    sb.AppendLine(action.GetXml());
                }

                sb.AppendLine("</actions>");
            }

            sb.AppendLine("</toast>");
            return sb.ToString();
        }

        public ToastNotification GetNotification()
        {
            var doc = new XmlDocument();
            var xml = GetXml();
            doc.LoadXml(xml);

            return new ToastNotification(doc);
        }

        public ScheduledToastNotification GetScheduledNotification(DateTimeOffset dateTime)
        {
            var doc = new XmlDocument();
            var xml = GetXml();
            doc.LoadXml(xml);

            return new ScheduledToastNotification(doc, dateTime);
        }
    }

    public abstract class VisualBase
    {
        public string Language { get; set; }

        public string BaseUri { get; set; }

        /// <summary>
        /// Gets or sets the add image query. 
        /// Set to "true" to allow Windows to append a query string 
        /// to the image URI supplied in the toast notification. 
        /// Use this attribute if your server hosts images and can handle query strings, 
        /// either by retrieving an image variant based on the query strings
        /// 
        /// eg
        /// "www.website.com/images/hello.png"
        /// becomes
        /// "www.website.com/images/hello.png?ms-scale=100&amp;ms-contrast=standard&amp;ms-lang=en-us"
        /// </summary>
        public bool? AddImageQuery { get; set; }

        internal string GetAttributes()
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(Language))
            {
                sb.Append($" lang=\"{Language}\"");
            }

            if (!string.IsNullOrEmpty(BaseUri))
            {
                sb.Append($" baseUri=\"{BaseUri}\"");
            }

            if (AddImageQuery.HasValue)
            {
                sb.Append($" addImageQuery=\"{AddImageQuery.Value}\"");
            }

            return sb.ToString();
        }
    }

    public class Visual : VisualBase
    {
        private readonly List<BindingBase> _items = new List<BindingBase>();

        public Branding? Branding { get; set; }

        public void AddText(Text text)
        {
            _items.Add(text);
        }

        public void RemoveText(Text text)
        {
            _items.Remove(text);
        }

        public void AddImage(VisualImage visualImage)
        {
            _items.Add(visualImage);
        }

        public void RemoveImage(VisualImage visualImage)
        {
            _items.Remove(visualImage);
        }

        internal string GetXml()
        {
            var sb = new StringBuilder("<visual");
            sb.Append(GetAttributes());

            if (Branding.HasValue)
            {
                sb.Append($" branding=\"{Branding.Value}\"");
            }

            sb.Append(">");

            if (_items.Any())
            {
                var binding = CreateBindingXml();
                sb.AppendLine(binding);
            }

            sb.AppendLine("</visual>");
            return sb.ToString();
        }

        private string CreateBindingXml()
        {
            var sb = new StringBuilder("<binding template=\"ToastGeneric\"");
            sb.Append(GetAttributes());
            sb.Append(">");

            foreach (var item in _items)
            {
                sb.AppendLine(item.GetXml());
            }

            sb.AppendLine("</binding>");
            return sb.ToString();
        }
    }

    public class Text : BindingBase
    {
        public string Content { get; set; }
        public string Language { get; set; }

        public Text(string content)
        {
            Content = content;
        }

        internal override string GetXml()
        {
            var sb = new StringBuilder("<text");
            if (!string.IsNullOrEmpty(Language))
            {
                sb.Append($" lang=\"{Language}\"");
            }

            var restOfText = $">{Content}</text>";
            sb.Append(restOfText);

            return sb.ToString();
        }
    }

    public class VisualImage : BindingBase
    {
        public string Source { get; set; }
        public string Alt { get; set; }
        public ImagePlacement? ImagePlacement { get; set; }
        public ImageCropping? ImageCropping { get; set; }
        /// <summary>
        /// Gets or sets the add image query. 
        /// Set to "true" to allow Windows to append a query string 
        /// to the image URI supplied in the toast notification. 
        /// Use this attribute if your server hosts images and can handle query strings, 
        /// either by retrieving an image variant based on the query strings
        /// 
        /// eg
        /// "www.website.com/images/hello.png"
        /// becomes
        /// "www.website.com/images/hello.png?ms-scale=100&amp;ms-contrast=standard&amp;ms-lang=en-us"
        /// </summary>
        public bool? AddImageQuery { get; set; }

        public VisualImage(string source)
        {
            Source = source;
        }

        internal override string GetXml()
        {
            var sb = new StringBuilder("<image");

            if (!string.IsNullOrEmpty(Source))
            {
                sb.Append($" src=\"{Source}\"");
            }

            if (!string.IsNullOrEmpty(Alt))
            {
                sb.Append($" alt=\"{Alt}\"");
            }

            if (ImagePlacement.HasValue)
            {
                sb.Append($" placement=\"{ImagePlacement.Value}\"");
            }

            if (ImageCropping.HasValue)
            {
                sb.Append($" hint-crop=\"{ImageCropping.Value}\"");
            }

            if (AddImageQuery.HasValue)
            {
                sb.Append($" addImageQuery=\"{AddImageQuery.Value}\"");
            }

            sb.Append(" />");
            return sb.ToString();
        }
    }

    public enum ImagePlacement
    {
        /// <summary>
        /// Means inside the toast body below texts
        /// </summary>
        Inline,

        /// <summary>
        /// Replace the application icon (that shows up on the top left corner of the toast)
        /// </summary>
        AppLogoOverride
    }

    public enum ImageCropping
    {
        None,
        Circle
    }

    public abstract class BindingBase
    {
        internal abstract string GetXml();
    }

    public enum Branding
    {
        None,
        Logo,
        Name
    }

    public class Audio
    {
        public string Source { get; internal set; }
        public bool? Loop { get; internal set; }
        public bool? IsSilent { get; internal set; }

        internal Audio(string source)
        {
            Source = source;
        }

        internal string GetXml()
        {
            var sb = new StringBuilder("<audio");

            if (!string.IsNullOrEmpty(Source))
            {
                sb.Append($" src=\"{Source}\"");
            }

            if (Loop.HasValue)
            {
                sb.Append($" loop=\"{Loop.Value}\"");
            }

            if (IsSilent.HasValue)
            {
                sb.Append($" silent=\"{IsSilent.Value}\"");
            }

            sb.Append(" />");
            return sb.ToString();
        }
    }

    public class ToastAction : ActionItem
    {
        public string Content { get; set; }
        public string Arguments { get; set; }
        public ActivationType? ActivationType { get; set; }
        public string ImageUri { get; set; }
        
        /// <summary>
        /// Gets or sets the input identifier. 
        /// This is specifically used for the quick reply scenario.
        /// The value needs to be the id of the input element desired to be associated with.
        /// In mobile and desktop, this will put the button right next to the input box.
        /// </summary>
        public string InputId { get; set; }

        public ToastAction(string content, string arguments)
        {
            Content = content;
            Arguments = arguments;
        }

        internal override string GetXml()
        {
            var sb = new StringBuilder("<action");

            sb.Append($" content=\"{Content}\" arguments=\"{Arguments}\"");

            if (ActivationType.HasValue)
            {
                sb.Append($" activationType=\"{ActivationType.Value}\"");
            }

            if (!string.IsNullOrEmpty(ImageUri))
            {
                sb.Append($" imageUri=\"{ImageUri}\"");
            }

            if (!string.IsNullOrEmpty(InputId))
            {
                sb.Append($" hint-inputId=\"{InputId}\"");
            }

            sb.Append(" />");
            return sb.ToString();
        }
    }

    public abstract class ActionItem
    {
        internal abstract string GetXml();
    }

    public class ToastInput : ActionItem
    {
        private readonly List<Selection> _items = new List<Selection>(); 
        public string Id { get; set; }
        public ToastInputType ToastInputType { get; set; }
        public string Title { get; set; }
        
        /// <summary>
        /// Gets or sets the content of the placeholder.
        /// This is the hint text for the textbox if InputType is Text
        /// </summary>
        public string PlaceholderContent { get; set; }

        /// <summary>
        /// Gets or sets the default input.
        /// If the input type is “text”, this will be treated as a string input;
        /// If the input type is “selection”, this is expected to be the id of the available selections inside this input elements.
        /// </summary>
        public string DefaultInput { get; set; }

        public ToastInput(string id, ToastInputType toastInputType)
        {
            Id = id;
            ToastInputType = toastInputType;
        }

        public void AddSelection(string id, string content)
        {
            _items.Add(new Selection(id, content));
        }

        internal override string GetXml()
        {
            var sb = new StringBuilder("<input");

            sb.Append($" id=\"{Id}\" type=\"{ToastInputType.ToString().ToLower()}\"");

            if (!string.IsNullOrEmpty(Title))
            {
                sb.Append($" title=\"{Title}\"");
            }

            if (!string.IsNullOrEmpty(PlaceholderContent) && ToastInputType == ToastInputType.Text)
            {
                sb.Append($" placeHolderContent=\"{PlaceholderContent}\"");
            }

            if (!string.IsNullOrEmpty(DefaultInput))
            {
                sb.Append($" defaultInput=\"{DefaultInput}\"");
            }

            switch (ToastInputType)
            {
                case ToastInputType.Text:
                    sb.Append(" />");
                    break;
                case ToastInputType.Selection:
                    sb.Append(">");
                    foreach (var item in _items)
                    {
                        sb.AppendLine(item.GetXml());
                    }

                    sb.AppendLine("</input>");
                    break;
            }

            return sb.ToString();
        }
    }

    public enum ToastInputType
    {
        /// <summary>
        /// This will display an input textbox
        /// </summary>
        Text,

        /// <summary>
        /// This will display an input listbox
        /// </summary>
        Selection
    }

    public class Selection
    {
        public string Id { get; set; }
        public string Content { get; set; }

        public Selection(string id, string content)
        {
            Id = id;
            Content = content;
        }

        internal string GetXml()
        {
            return $"<selection id=\"{Id}\" content=\"{Content}\" />";
        }
    }

    public enum ActivationType
    {
        Foreground,
        Background,
        Protocol,
        System
    }

    public enum Scenario
    {
        Default,
        Alarm,
        Reminder,
        IncomingCall
    }

    public enum ToastTemplate
    {
        ToastGeneric
    }
}
