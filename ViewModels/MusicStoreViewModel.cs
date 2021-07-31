using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Collections;
using MusicStoreTutorial.Models;
using MusicStoreTutorial.Views;
using ReactiveUI;

namespace MusicStoreTutorial.ViewModels
{
    public class MusicStoreViewModel : ViewModelBase
    {
        private string? _searchText;
        private bool _isBusy;
        private AlbumViewModel? _selectedAlbum;

        public MusicStoreViewModel()
        {
            BuyMusicCommand = ReactiveCommand.CreateFromTask(async () => SelectedAlbum);
            this.WhenAnyValue(x => x.SearchText).Where(x => !string.IsNullOrWhiteSpace(x))
                .Throttle(TimeSpan.FromMilliseconds(400))
                .ObserveOn(RxApp.MainThreadScheduler).Subscribe(DoSearch!);
        }
        public ReactiveCommand<Unit, AlbumViewModel?> BuyMusicCommand { get; }

        public AvaloniaList<AlbumViewModel> SearchResults { get; } = new();

        public AlbumViewModel? SelectedAlbum
        {
            get => _selectedAlbum;
            set => this.RaiseAndSetIfChanged(ref _selectedAlbum, value);
        }

        public string? SearchText
        {
            get => _searchText;
            set => this.RaiseAndSetIfChanged(ref _searchText, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => this.RaiseAndSetIfChanged(ref _isBusy, value);
        }

        private CancellationTokenSource? _cancellationTokenSource;

        private async void DoSearch(string s)
        {
            IsBusy = true;
            SearchResults.Clear();
            
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            var albums = await Album.SearchAsync(s);

            foreach (var album in albums)
            {
                var vm = new AlbumViewModel(album);
                SearchResults.Add(vm);
            }

            if (!_cancellationTokenSource.IsCancellationRequested)
            {
                LoadCovers(_cancellationTokenSource.Token);
            }

            IsBusy = false;
        }

        private async void LoadCovers(CancellationToken cancellationToken)
        {
            foreach (var album in SearchResults.ToList())
            {
                await album.LoadCover();

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
            }
        }
    }
}
