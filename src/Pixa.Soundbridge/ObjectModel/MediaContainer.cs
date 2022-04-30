using System;

namespace Pixa.Soundbridge.Library
{
    public class MediaContainer : MediaObject
    {
        private MediaContainer _parent;
        private bool _isCurrentContainer;

        internal MediaContainer(MediaServer server, int index, string name) : base(server, index, name)
        {
            _isCurrentContainer = true;
            Soundbridge.Cache.RegisterCache(new MediaContainerMediaContainerCacheProvider(Soundbridge, this));
            Soundbridge.Cache.RegisterCache(new MediaContainerSongCacheProvider(Soundbridge, this));
        }

        internal MediaContainer(MediaContainer parent, int index, string name) : base(parent.Server, index, name)
        {
            _parent = parent;
            Soundbridge.Cache.RegisterCache(new MediaContainerMediaContainerCacheProvider(Soundbridge, this));
            Soundbridge.Cache.RegisterCache(new MediaContainerSongCacheProvider(Soundbridge, this));
        }

        public bool IsCurrentContainer
        {
            get
            {
                return ReferenceEquals(Soundbridge.ConnectedServer, Server) & _isCurrentContainer;
            }
        }

        public MediaContainer Parent
        {
            get
            {
                return _parent;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                Server.Soundbridge.Cache.DeregisterCache(this, typeof(MediaContainer));
            }
        }

        public void Enter()
        {
            if (IsActive)
            {
                string r = Client.ContainerEnter(Index);
                if (r != "OK")
                {
                    ExceptionHelper.ThrowCommandReturnError("ContainerEnter", r);
                }

                Server.Soundbridge.ActiveList = null;
                _isCurrentContainer = true;
                Parent._isCurrentContainer = false;
            }
            else
            {
                throw new InvalidOperationException("Can't enter a container that's not in the active list");
            }
        }

        public void Exit()
        {
            if (IsCurrentContainer)
            {
                string r = Client.ContainerExit();
                if (r != "OK")
                {
                    ExceptionHelper.ThrowCommandReturnError("ContainerExit", r);
                }

                Server.Soundbridge.ActiveList = null;
                _isCurrentContainer = false;
                Parent._isCurrentContainer = true;
            }
            else
            {
                throw new InvalidOperationException("Can't exit a container that's not the current container");
            }
        }

        public SoundbridgeObjectCollection<MediaContainer> GetChildContainers()
        {
            if (IsCurrentContainer)
            {
                var containers = Client.ListContainerContents();
                Soundbridge.ActiveList = Soundbridge.Cache.BuildList<MediaContainer>(this, containers);
                return (SoundbridgeObjectCollection<MediaContainer>)Soundbridge.ActiveList;
            }
            else
            {
                throw new InvalidOperationException("Can't get child items for a container that's not the current container");
            }
        }

        public SongCollection GetSongs()
        {
            if (IsCurrentContainer)
            {
                var songs = Client.ListContainerContents();
                Soundbridge.ActiveList = Soundbridge.Cache.BuildList<Song>(this, songs);
                return (SongCollection)Soundbridge.ActiveList;
            }
            else
            {
                throw new InvalidOperationException("Can't get child items for a container that's not the current container");
            }
        }
    }
}