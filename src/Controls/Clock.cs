using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using UwpMaterialClock.Extensions;

namespace UwpMaterialClock.Controls
{
    public class Clock : Control, IClock
    {
        private Point dragPosition;
        private Point canvasCenter;
        private ClockItemMember displayMode;
        private ClockButton lastTappedButton;
        private Canvas hoursCanvas;
        private Canvas minutesCanvas;
        private TextBlock textBlockHours;
        private TextBlock textBlockMinutes;

        public event EventHandler TimeChanged;

        public static readonly DependencyProperty TimeProperty = DependencyProperty.Register(
            nameof(Time),
            typeof(DateTime),
            typeof(Clock),
            new PropertyMetadata(default(DateTime), OnTimeChanged));

        public DateTime Time
        {
            get { return (DateTime) this.GetValue(TimeProperty); }
            set { this.SetValue(TimeProperty, value); }
        }

        public static readonly DependencyProperty Is24HoursEnabledProperty = DependencyProperty.Register(
            nameof(Is24HoursEnabled),
            typeof(bool),
            typeof(Clock),
            new PropertyMetadata(false, OnIs24HoursEnabledChanged));
        
        public bool Is24HoursEnabled
        {
            get { return (bool)this.GetValue(Is24HoursEnabledProperty); }
            set { this.SetValue(Is24HoursEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsPostMeridiemProperty = DependencyProperty.Register(
            nameof(IsPostMeridiem),
            typeof(bool),
            typeof(Clock),
            new PropertyMetadata(false, OnIsPostMeridiumChanged));
        
        public bool IsPostMeridiem
        {
            get { return (bool)this.GetValue(IsPostMeridiemProperty); }
            set { this.SetValue(IsPostMeridiemProperty, value); }
        }
        
        public Clock()
        {
            // default to the system settings at initialization
            // can be overriden later
            this.Is24HoursEnabled = false; // IsUsing24HoursTime(CultureInfo.CurrentCulture);
        }

        private static void OnTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var clock = (Clock) d;

            if (!clock.Is24HoursEnabled && clock.Time.Hour > 12)
                clock.IsPostMeridiem = true;

            if (clock.hoursCanvas == null)
                return; // template hasn't been loaded yet

            if (clock.lastTappedButton != null)
                clock.lastTappedButton.IsChecked = false;

            if (clock.displayMode == ClockItemMember.Hours)
            {
                clock.lastTappedButton = clock.hoursCanvas.Children.OfType<ClockButton>().First(b =>
                {
                    int hours = clock.Time.Hour;
                    if (!clock.Is24HoursEnabled && clock.IsPostMeridiem)
                        hours -= 12;

                    return b.Value % 24 == hours;
                });
            }
            else
            {
                clock.lastTappedButton = clock.minutesCanvas.Children.OfType<ClockButton>().First(b =>
                {
                    int minutes = clock.Time.Minute;
                    return b.Value % 60 == minutes;
                });
            }


            clock.UpdateHeaderDisplay();

            clock.lastTappedButton.IsChecked = true;

            clock.TimeChanged?.Invoke(clock, EventArgs.Empty);
        }

        private static void OnIs24HoursEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var clock = (Clock)d;

            clock.UpdateHeaderDisplay();

            var buttons = clock.hoursCanvas.Children.OfType<ClockButton>().ToList();
            foreach (var clockButton in buttons)
            {
                clock.hoursCanvas.Children.Remove(clockButton);
            }
            buttons = clock.minutesCanvas.Children.OfType<ClockButton>().ToList();
            foreach (var clockButton in buttons)
            {
                clock.minutesCanvas.Children.Remove(clockButton);
            }

            clock.GenerateButtons();

            if (clock.Time.Hour > 12)
                clock.IsPostMeridiem = true;
            else
                clock.IsPostMeridiem = false;
        }

        private static void OnIsPostMeridiumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var clock = (Clock)d;

            if (clock.IsPostMeridiem && clock.Time.Hour < 12)
                clock.Time = new DateTime(clock.Time.Year, clock.Time.Month, clock.Time.Day, clock.Time.Hour + 12, clock.Time.Minute, 0);
            else if (!clock.IsPostMeridiem && clock.Time.Hour >= 12)
                clock.Time = new DateTime(clock.Time.Year, clock.Time.Month, clock.Time.Day, clock.Time.Hour - 12, clock.Time.Minute, 0);
        }

        private void UpdateHeaderDisplay()
        {
            if (this.Is24HoursEnabled || this.Time.Hour <= 12)
                this.textBlockHours.Text = this.Time.Hour.ToString("D2");
            else
                this.textBlockHours.Text = (this.Time.Hour - 12).ToString("D2");

            this.textBlockMinutes.Text = this.Time.Minute.ToString("D2");
        }

        protected override void OnApplyTemplate()
        {
            this.textBlockHours = this.GetTemplateChild("PART_TextBlockHours") as TextBlock;
            if (this.textBlockHours == null)
                throw new NotSupportedException("Could not find PART_TextBlockHours in the control template");

            this.textBlockMinutes = this.GetTemplateChild("PART_TextBlockMinutes") as TextBlock;
            if (this.textBlockMinutes == null)
                throw new NotSupportedException("Could not find PART_TextBlockMinutes in the control template");

            this.hoursCanvas = this.GetTemplateChild("PART_HoursCanvas") as Canvas;
            if (this.hoursCanvas == null)
                throw new NotSupportedException("Could not find PART_HoursCanvas in the control template");

            this.minutesCanvas = this.GetTemplateChild("PART_MinutesCanvas") as Canvas;
            if (this.minutesCanvas == null)
                throw new NotSupportedException("Could not find PART_MinutesCanvas in the control template");

            this.textBlockHours.Tapped += (s, e) => this.SetDisplayMode(ClockItemMember.Hours);
            this.textBlockMinutes.Tapped += (s, e) => this.SetDisplayMode(ClockItemMember.Minutes);

            this.GenerateButtons();

            this.SetDisplayMode(ClockItemMember.Hours);
            this.UpdateHeaderDisplay();


        }

        private void SetDisplayMode(ClockItemMember mode)
        {
            this.displayMode = mode;
            VisualStateManager.GoToState(this, mode == ClockItemMember.Hours ? "Normal" : "Minutes", true);
        }

        private void GenerateButtons()
        {
            if (this.Is24HoursEnabled)
            {
                this.GenerateButtons(this.hoursCanvas, Enumerable.Range(13, 12).ToList(), ClockItemMember.Hours, 1, "00");
                this.GenerateButtons(this.hoursCanvas, Enumerable.Range(1, 12).ToList(), ClockItemMember.Hours, 0.8, "#");
            }
            else
            {
                this.GenerateButtons(this.hoursCanvas, Enumerable.Range(1, 12).ToList(), ClockItemMember.Hours, 1, "0");
            }

            this.GenerateButtons(this.minutesCanvas, Enumerable.Range(1, 60).ToList(), ClockItemMember.Minutes, 1, "00");
        }

        private void GenerateButtons(Panel canvas, List<int> range, ClockItemMember mode, double innerRatio, string format)
        {
            double anglePerItem = 360.0 / range.Count;
            double radiansPerItem = anglePerItem * (Math.PI / 180);

            if (canvas.Width < 10.0 || Math.Abs(canvas.Height - canvas.Width) > 0.0)
                return;

            this.canvasCenter = new Point(canvas.Width / 2, canvas.Height / 2);
            double hypotenuseRadius = this.canvasCenter.X * innerRatio;

            foreach (int value in range)
            {
                double adjacent = Math.Cos(value * radiansPerItem) * hypotenuseRadius;
                double opposite = Math.Sin(value * radiansPerItem) * hypotenuseRadius;

                double centerX = this.canvasCenter.X + opposite;
                double centerY = this.canvasCenter.Y - adjacent;

                ClockButton button = new ClockButton(mode, value, centerX, centerY, this)
                {
                    Content = (value == 60 ? 0 : (value == 24 ? 0 : value)).ToString(format)                    
                };

                if (mode == ClockItemMember.Minutes && (value % 5 != 0))
                {
                    button.Width = 10;
                    button.Height = 10;
                    button.Style = this.FindResource<Style>("HintClockButtonStyle");
                }
                else
                {
                    button.Width = 40;
                    button.Height = 40;
                }
                
                Canvas.SetLeft(button, centerX - button.Width/2);
                Canvas.SetTop(button, centerY - button.Height/2);

                canvas.Children.Add(button);
            }
        }

        public void OnButtonTapped(ClockButton sender)
        {
            if (this.lastTappedButton != null)
                this.lastTappedButton.IsChecked = false;

            sender.IsChecked = !sender.IsChecked;

            if (sender.Mode == ClockItemMember.Hours)
            {
                int hour = sender.Value % 24;
                if (!this.Is24HoursEnabled && this.IsPostMeridiem)
                    hour += 12;

                this.Time = new DateTime(this.Time.Year, this.Time.Month, this.Time.Day, hour, this.Time.Minute, 0);
                this.SetDisplayMode(ClockItemMember.Minutes);
            }
            else
            {
                int minute = sender.Value % 60;
                this.Time = new DateTime(this.Time.Year, this.Time.Month, this.Time.Day, this.Time.Hour, minute, 0);
            }

            this.lastTappedButton = sender;
        }

        public void OnButtonDragStarted(ClockButton sender, DragStartedEventArgs e)
        {
            this.dragPosition = new Point(e.HorizontalOffset, e.VerticalOffset);
        }
        
        public void OnButtonDragDelta(ClockButton sender, DragDeltaEventArgs e)
        {
            this.dragPosition = new Point(
                this.dragPosition.X + e.HorizontalChange, 
                this.dragPosition.Y + e.VerticalChange);

            Point delta = new Point(
                this.dragPosition.X - this.canvasCenter.X,
                this.dragPosition.Y - this.canvasCenter.Y);

            var angle = Math.Atan2(delta.X, -delta.Y);
            if (angle < 0)
                angle += 2 * Math.PI;
            
            DateTime time;
            if (this.displayMode == ClockItemMember.Hours)
            {
                if (this.Is24HoursEnabled)
                {
                    double outerBoundary = this.canvasCenter.X * 0.75 + (this.canvasCenter.X * 1 - this.canvasCenter.X * 0.75) / 2;
                    double sqrt = Math.Sqrt(
                        (this.canvasCenter.X - this.dragPosition.X) * (this.canvasCenter.X - this.dragPosition.X) +
                        (this.canvasCenter.Y - this.dragPosition.Y) * (this.canvasCenter.Y - this.dragPosition.Y));

                    bool localIsPostMeridiem = sqrt > outerBoundary;

                    int hour = (int) Math.Round(6 * angle / Math.PI, MidpointRounding.AwayFromZero) % 12 + (localIsPostMeridiem ? 12 : 0);

                    if (hour == 12)
                        hour = 0;
                    else if (hour == 0)
                        hour = 12;

                    time = new DateTime(this.Time.Year, this.Time.Month, this.Time.Day, hour, this.Time.Minute, this.Time.Second);
                }
                else
                {
                    time = new DateTime(this.Time.Year, this.Time.Month, this.Time.Day,
                        (int) Math.Round(6 * angle / Math.PI, MidpointRounding.AwayFromZero) % 12 +
                        (this.IsPostMeridiem ? 12 : 0), this.Time.Minute, this.Time.Second);
                }
            }
            else
            {
                time = new DateTime(this.Time.Year, this.Time.Month, this.Time.Day, this.Time.Hour, (int)Math.Round(30 * angle / Math.PI, MidpointRounding.AwayFromZero) % 60, this.Time.Second);
            }

            this.Time = time;
        }

        public void OnButtonDragCompleted(ClockButton sender, DragCompletedEventArgs e)
        {
            this.SetDisplayMode(ClockItemMember.Minutes);
        }

        private static bool IsUsing24HoursTime(CultureInfo culture)
        {
            return culture.DateTimeFormat.ShortTimePattern.Contains("h");
        }
    }
}
