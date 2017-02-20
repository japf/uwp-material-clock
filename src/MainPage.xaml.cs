using System;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UwpMaterialClock
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.clock.Time = DateTime.Now;

            bool ignore = false;

            this.clock.TimeChanged += (s, e) =>
            {
                if (!ignore)
                {
                    ignore = true;
                    this.timePicker.Time = this.clock.Time.TimeOfDay;
                    ignore = false;
                }
            };

            this.timePicker.TimeChanged += (s, e) =>
            {
                if (!ignore)
                {
                    ignore = true;
                    this.clock.Time = DateTime.Now.Date.Add(this.timePicker.Time);
                    ignore = false;
                }
            };
        }
    }
}
