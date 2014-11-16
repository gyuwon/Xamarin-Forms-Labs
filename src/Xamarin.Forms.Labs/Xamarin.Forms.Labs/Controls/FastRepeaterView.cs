using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Xamarin.Forms.Labs.Controls
{
    public class FastRepeaterView<T> : RepeaterView<T>
    {
        private BitArray _layoutKey;
        private int _layoutCompletion;

        public FastRepeaterView()
        {
            _layoutKey = null;
            _layoutCompletion = 0;

            ((INotifyCollectionChanged)Children).CollectionChanged += OnElementCollectionChanged;
        }

        private void OnElementCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (View item in e.OldItems)
                {
                    item.MeasureInvalidated -= OnChildMeasureInvalidated;
                }
                MakeLayoutDirtyFrom(e.OldStartingIndex);
            }

            if (e.NewItems != null)
            {
                foreach (View item in e.NewItems)
                {
                    item.MeasureInvalidated += OnChildMeasureInvalidated;
                }
                MakeLayoutDirtyFrom(e.NewStartingIndex);
            }
        }

        private void MakeLayoutDirtyFrom(int index)
        {
            _layoutCompletion = Math.Min(_layoutCompletion, index);
        }

        private void MakeLayoutDirtyFrom(View view)
        {
            MakeLayoutDirtyFrom(Children.IndexOf(view));
        }

        new private void OnChildMeasureInvalidated(object sender, EventArgs e)
        {
            MakeLayoutDirtyFrom((View)sender);
        }

        private void EnsureLayout(Rectangle bounds)
        {
            switch (Orientation)
            {
                case StackOrientation.Horizontal:
                    throw new NotImplementedException();

                case StackOrientation.Vertical:
                    EnsureLayoutVertical(bounds);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        private void EnsureLayoutVertical(Rectangle bounds)
        {
            var completion = _layoutCompletion;
            var key = CreateKey(bounds.Left, bounds.Top, bounds.Width);
            if (_layoutKey == null || new BitArray(_layoutKey).Xor(key).Cast<bool>().Any(e => e))
            {
                completion = 0;
            }
            for (; completion < Children.Count; completion++)
            {
                var view = Children[completion];
                var sizeRequest = view.GetSizeRequest(bounds.Width, bounds.Height);
                var offset = 0.0;
                if (completion != 0)
                {
                    offset = Children[completion - 1].Bounds.Bottom + Spacing;
                }
                view.Layout(new Rectangle
                            {
                                Location = new Point(0, offset),
                                Size = new Size
                                       {
                                           Width = bounds.Width,
                                           Height = sizeRequest.Request.Height
                                       }
                            });
            }
            _layoutKey = key;
            _layoutCompletion = completion;
        }

        protected override SizeRequest OnSizeRequest(double widthConstraint,
                                                     double heightConstraint)
        {
            EnsureLayout(new Rectangle
                         {
                             Location = Point.Zero,
                             Size = new Size
                                    {
                                        Width = widthConstraint,
                                        Height = heightConstraint
                                    }
                         });

            var last = Children.LastOrDefault();
            if (last == null)
            {
                return new SizeRequest(Size.Zero);
            }
            switch (Orientation)
            {
                case StackOrientation.Horizontal:
                    throw new NotImplementedException();

                case StackOrientation.Vertical:
                    return new SizeRequest(new Size
                    {
                        Width = widthConstraint,
                        Height = last.Bounds.Bottom
                    });

                default:
                    throw new InvalidOperationException();
            }
        }

        private BitArray CreateKey(params double[] values)
        {
            return new BitArray(BitConverter.GetBytes((int)Orientation)
                                            .Concat(values.SelectMany(BitConverter.GetBytes))
                                            .ToArray());
        }

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            EnsureLayout(new Rectangle
                         {
                             Location = new Point(x, y),
                             Size = new Size
                                    {
                                        Width = width,
                                        Height = height
                                    }
                         });
        }
    }
}