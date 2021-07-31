using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using MusicStoreTutorial.Models;
using ReactiveUI;

namespace MusicStoreTutorial.ViewModels
{
    public class AlbumViewModel : ViewModelBase
    {
        private readonly Album _album;
        private Bitmap? _cover;

        public AlbumViewModel(Album album)
        {
            _album = album;
        }

        public string Artist => _album.Artist;
        public string Title => _album.Title;

        public Bitmap? Cover
        {
            get => _cover;
            private set => this.RaiseAndSetIfChanged(ref _cover, value);
        }

        public async Task SaveToDiskAsync()
        {
            await _album.SaveAsync();

            if (Cover != null)
            {
                var bitmap = Cover;

                await Task.Run(() =>
                {
                    using (var fs = _album.SaveCoverBitmapSteam())
                    {
                        bitmap.Save(fs);
                    }
                });
            }
        }

        public async Task LoadCover()
        {
            await using (var imageSteam = await _album.LoadCoverBitmapAsync())
            {
                Cover = await Task.Run(() => Bitmap.DecodeToWidth(imageSteam, 400));
            }
        }
    }
}
