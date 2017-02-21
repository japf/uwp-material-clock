using System;
using System.Diagnostics;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace UwpMaterialClock.Controls
{
    [DebuggerDisplay("Value: {Value} Mode: {Mode}")]
    public class ClockButton : ToggleButton
    {
        private readonly IClock owner;
        private readonly double centerX;
        private readonly double centerY;

        public int Value { get; }

        public ClockItemMember Mode { get; }

        public ClockButton(ClockItemMember mode, int value, double centerX, double centerY, IClock owner)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));

            this.Mode = mode;
            this.centerX = centerX;
            this.centerY = centerY;
            this.Value = value;
            this.owner = owner;
        }

        protected override void OnTapped(TappedRoutedEventArgs e)
        {
            this.owner.OnButtonTapped(this);
        }

        protected override void OnApplyTemplate()
        {
            Thumb thumb = this.GetTemplateChild("PART_Thumb") as Thumb;
            if (thumb != null)
            {
                thumb.DragStarted += this.OnDragStarted;
                thumb.DragDelta += this.OnDragDelta;
                thumb.DragCompleted += this.OnDragCompleted;
            }
        }
        
        private void OnDragStarted(object sender, DragStartedEventArgs e)
        {
            this.owner.OnButtonDragStarted(this, new DragStartedEventArgs(
                this.centerX + e.HorizontalOffset - this.ActualWidth / 2.0, 
                this.centerY + e.VerticalOffset - this.ActualHeight / 2.0));
        }

        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            this.owner.OnButtonDragDelta(this, e);
        }

        private void OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            this.owner.OnButtonDragCompleted(this, e);
        }
    }
}