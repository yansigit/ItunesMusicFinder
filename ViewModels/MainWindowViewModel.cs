using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Collections;
using MusicStoreTutorial.Models;

namespace MusicStoreTutorial.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public bool _collectionEmpty;
        
        public MainWindowViewModel()
        {
            Task.Run(LoadAlbums);
            ShowDialog = new Interaction<MusicStoreViewModel, AlbumViewModel?>();
            BuyMusicCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var store = new MusicStoreViewModel();
                var result = await ShowDialog.Handle(store);
                if (result != null)
                {
                    Albums.Add(result);
                    await result.SaveToDiskAsync();
                }
            });
            this.WhenAnyValue(x => x.Albums.Count).Subscribe(x => CollectionEmpty = x == 0);
        }

        public Interaction<MusicStoreViewModel, AlbumViewModel?> ShowDialog { get; set; }

        public ReactiveCommand<Unit, Unit> BuyMusicCommand { get; }

        public bool CollectionEmpty
        {
            get => _collectionEmpty;
            set => this.RaiseAndSetIfChanged(ref _collectionEmpty, value);
        }

        public AvaloniaList<AlbumViewModel> Albums { get; } = new();

        private async Task LoadAlbums()
        {
            var albums = (await Album.LoadCachedAsync()).Select(x => new AlbumViewModel(x));

            foreach (var album in albums)
            {
                Albums.Add(album);
            }

            foreach (var album in Albums)
            {
                await album.LoadCover();
            }
        }
    }
}
