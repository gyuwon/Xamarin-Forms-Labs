using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Xamarin.Forms.Labs.Sample.ViewModel
{
    public class InfiniteScrollViewModel : Mvvm.ViewModel
    {
        public const string LoremIpsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

        private static string[] LoremIpsumWords = LoremIpsum.Split(' ');

        private Point _position;
        private Size _size;
        private Stack<InfiniteScrollItem> _dataSource;
        private ObservableCollection<InfiniteScrollItem> _items;
        private long _elapsedToAdd;
        private double _threshold;

        public InfiniteScrollViewModel()
        {
            _dataSource = new Stack<InfiniteScrollItem>();
            _items = new ObservableCollection<InfiniteScrollItem>();
            _threshold = 500;

            Initialize = new Command(async () =>
            {
                _items.Clear();
                CreateDataSource();
                await LoadItemsAsync(count: 20);
            });

            PropertyChanged += async (s, e) =>
            {
                if (e.PropertyName == "Position" || e.PropertyName == "Size")
                {
                    if (IsBusy || _dataSource.Any() == false)
                    {
                        return;
                    }
                    if (_size.Height < _position.Y + _threshold)
                    {
                        IsBusy = true;
                        try
                        {
                            await LoadItemsAsync(count: 20);
                        }
                        finally
                        {
                            IsBusy = false;
                        }
                    }
                }
            };
        }

        private void CreateDataSource(int size = 1000)
        {
            var r = new Random();
            var query = from i in Enumerable.Range(1, 1000)
                        let words = LoremIpsumWords.Take(r.Next(LoremIpsumWords.Length) + 1)
                        select new InfiniteScrollItem
                        {
                            Id = i,
                            Message = string.Join(" ", words)
                        };
            _dataSource = new Stack<InfiniteScrollItem>(query);
        }

        private async Task LoadItemsAsync(int count)
        {
            await Task.Delay(100);
            for (int i = 0; i < count && _dataSource.Any(); i++)
            {
                var stopwatch = Stopwatch.StartNew();
                _items.Add(_dataSource.Pop());
                stopwatch.Stop();
                ElapsedToAdd = stopwatch.ElapsedMilliseconds;
            }
        }

        public Point Position
        {
            get { return _position; }
            set { SetProperty(ref _position, value); }
        }

        public Size Size
        {
            get { return _size; }
            set { SetProperty(ref _size, value); }
        }

        public long ElapsedToAdd
        {
            get { return _elapsedToAdd; }
            private set { SetProperty(ref _elapsedToAdd, value); }
        }

        public Command Initialize { get; set; }

        public ObservableCollection<InfiniteScrollItem> Items { get { return _items; } }
    }
}