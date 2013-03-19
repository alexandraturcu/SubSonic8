﻿using Microsoft.PlayerFramework;
using Windows.UI.Xaml;

namespace Subsonic8.Playback.Playback
{
    public sealed partial class Video
    {
        public Video()
        {
            InitializeComponent();
        }

        private void MediaPlayer_OnMediaEnded(object sender, MediaPlayerActionEventArgs e)
        {
            // TODO: Replace with something nicer | It may be bug in Windows.Interactivity
            ((PlaybackViewModel)DataContext).Next();
        }

        private void MediaPlayer_OnIsFullScreenChanged(object sender, RoutedPropertyChangedEventArgs<bool> e)
        {
            // TODO: Replace with something nicer | It may be bug in Windows.Interactivity
            ((PlaybackViewModel)DataContext).IsFullScreenChanged(MediaPlayer);
        }
    }
}
