﻿using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Model.Entities;
using System.IO;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.XbmcMetadata.Savers
{
    public class MovieXmlSaver : IMetadataSaver
    {
        private readonly ILibraryManager _libraryManager;
        private readonly IUserManager _userManager;
        private readonly IUserDataRepository _userDataRepo;

        public MovieXmlSaver(ILibraryManager libraryManager, IUserManager userManager, IUserDataRepository userDataRepo)
        {
            _libraryManager = libraryManager;
            _userManager = userManager;
            _userDataRepo = userDataRepo;
        }

        public string GetSavePath(BaseItem item)
        {
            if (item.ResolveArgs.IsDirectory)
            {
                var video = (Video)item;
                var path = video.VideoType == VideoType.VideoFile || video.VideoType == VideoType.Iso ? Path.GetDirectoryName(item.Path) : item.Path;

                return Path.Combine(path, Path.GetFileNameWithoutExtension(path) + ".nfo");
            }

            return Path.ChangeExtension(item.Path, ".nfo");
        }

        public void Save(BaseItem item, CancellationToken cancellationToken)
        {
            var builder = new StringBuilder();

            var tag = item is MusicVideo ? "musicvideo" : "movie";

            builder.Append("<" + tag + ">");

            XmlSaverHelpers.AddCommonNodes(item, builder, _libraryManager, _userManager, _userDataRepo);

            var imdb = item.GetProviderId(MetadataProviders.Imdb);

            if (!string.IsNullOrEmpty(imdb))
            {
                builder.Append("<id>" + SecurityElement.Escape(imdb) + "</id>");
            }

            var musicVideo = item as MusicVideo;

            if (musicVideo != null)
            {
                if (!string.IsNullOrEmpty(musicVideo.Artist))
                {
                    builder.Append("<artist>" + SecurityElement.Escape(musicVideo.Artist) + "</artist>");
                }
                if (!string.IsNullOrEmpty(musicVideo.Album))
                {
                    builder.Append("<album>" + SecurityElement.Escape(musicVideo.Album) + "</album>");
                }
            }
            
            XmlSaverHelpers.AddMediaInfo((Video)item, builder);

            builder.Append("</" + tag + ">");

            var xmlFilePath = GetSavePath(item);

            XmlSaverHelpers.Save(builder, xmlFilePath, new[]
                {
                    "id",
                    "album",
                    "artist"
                });
        }

        public bool IsEnabledFor(BaseItem item, ItemUpdateType updateType)
        {
            // If new metadata has been downloaded or metadata was manually edited, proceed
            if ((updateType & ItemUpdateType.MetadataDownload) == ItemUpdateType.MetadataDownload
                || (updateType & ItemUpdateType.MetadataEdit) == ItemUpdateType.MetadataEdit)
            {
                var trailer = item as Trailer;

                if (trailer != null)
                {
                    return !trailer.IsLocalTrailer;
                }

                return item is Movie || item is MusicVideo;
            }

            return false;
        }
    }
}
