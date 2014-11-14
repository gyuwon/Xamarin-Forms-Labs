using System;
using System.Collections.Specialized;
using System.Linq;

namespace Xamarin.Forms.Labs.Controls
{
    public class FastRepeaterView<T> : RepeaterView<T>
    {
        private class Measure
        {
            public Size? Size { get; set; }

            public Size? Offset { get; set; }

            public void MakeDirty(bool size = true, bool offset = true)
            {
                if (size)
                {
                    Size = null;
                }
                if (offset)
                {
                    Offset = null;
                }
            }
        }

        private static BindableProperty MeasureProperty = BindableProperty.CreateAttached("Measure", typeof(Measure), typeof(FastRepeaterView<T>), null);

        private Rectangle? _lastBounds;

        public FastRepeaterView()
        {
            ((INotifyCollectionChanged)Children).CollectionChanged += OnElementCollectionChanged;
        }

        private void OnElementCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (View item in e.OldItems)
                {
                    item.MeasureInvalidated -= OnChildMeasureInvalidated;
                    item.SetValue(MeasureProperty, null);
                }
            }

            if (e.NewItems != null)
            {
                foreach (View item in e.NewItems)
                {
                    item.MeasureInvalidated += OnChildMeasureInvalidated;
                }
            }
        }

        new private void OnChildMeasureInvalidated(object sender, EventArgs e)
        {
            foreach (var view in Children.Skip(Children.IndexOf((View)sender)))
            {
                view.GetValue<Measure>(MeasureProperty).MakeDirty(size: sender == view);
            }
        }

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            var bounds = new Rectangle(x, y, width, height);

            if (_lastBounds != bounds)
            {
                foreach (var child in Children)
                {
                    var measure = child.GetValue<Measure>(MeasureProperty);
                    if (measure != null)
                    {
                        measure.MakeDirty();
                    }
                }
            }

            EnsureChildrenMeasure(bounds);

            foreach (var view in Children)
            {
                var measure = view.GetValue<Measure>(MeasureProperty);
                LayoutChild(view, new Rectangle
                                  {
                                      Size = measure.Size.Value,
                                      Location = (Point)measure.Offset.Value
                                  });
            }

            _lastBounds = bounds;
        }

        private void LayoutChild(View view, Rectangle layoutBounds)
        {
            if (view.Bounds == layoutBounds)
            {
                return;
            }

            view.Layout(layoutBounds);
        }

        private void EnsureChildrenMeasure(Rectangle bounds)
        {
            if (Children.Any() == false)
            {
                return;
            }

            EnsureChildrenSize(bounds.Size);

            double offset = 0.0;
            foreach (var view in Children)
            {
                var measure = view.GetValue<Measure>(MeasureProperty);
                measure.Offset = new Size
                                 {
                                     Width = bounds.Left,
                                     Height = offset
                                 };
                offset += (measure.Size.Value.Height + Spacing);
            }
        }

        private void EnsureChildrenSize(Size size)
        {
            if (Children.Any() == false)
            {
                return;
            }

            foreach (var view in Children)
            {
                var measure = view.GetValue<Measure>(MeasureProperty);
                if (measure == null)
                {
                    measure = new Measure();
                    view.SetValue(MeasureProperty, measure);
                }
                if (measure.Size == null)
                {
                    if (view.IsVisible == false)
                    {
                        measure.Size = Size.Zero;
                    }
                    else
                    {
                        var sizeRequest = view.GetSizeRequest(size.Width, size.Height);
                        measure.Size = sizeRequest.Request;
                    }
                }
            }
        }

        protected override SizeRequest OnSizeRequest(double widthConstraint,
                                                     double heightConstraint)
        {
            if (Children.Any() == false)
            {
                return new SizeRequest
                       {
                           Request = Size.Zero,
                           Minimum = Size.Zero
                       };
            }

            switch (Orientation)
            {
                case StackOrientation.Horizontal:
                    throw new NotImplementedException();

                case StackOrientation.Vertical:
                    return OnSizeRequestVertical(widthConstraint, heightConstraint);

                default:
                    throw new InvalidOperationException();
            }
        }

        private SizeRequest OnSizeRequestVertical(double widthConstraint, double heightConstraint)
        {
            EnsureChildrenSize(new Size { Width = widthConstraint, Height = heightConstraint });

            var measures = Children.Select(v => v.GetValue<Measure>(MeasureProperty));
            var width = measures.Max(m => m.Size.Value.Width);
            var height = measures.Aggregate(seed: -Spacing,
                                            func: (a, m) => a + m.Size.Value.Height + Spacing);
            var request = new Size { Width = width, Height = height };
            return new SizeRequest { Request = request, Minimum = request };
        }
    }
}