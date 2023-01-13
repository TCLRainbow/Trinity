// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CSCore.CoreAudioAPI;
using CSCore.SoundOut;
using Microsoft.UI.Xaml;
using CSCore.Codecs.OPUS;
using System.IO;
using NetDiscordRpc;
using System.Linq;
using Windows.Storage.Pickers;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Trinity
{

    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private static WasapiOut wasapi = new() { Device = MMDeviceEnumerator.DefaultAudioEndpoint(DataFlow.Render, Role.Multimedia)};
        private static string filePath;
        private static DiscordRPC rpc = new("757258842328399944");

        public MainWindow()
        {
            InitializeComponent();
            rpc.Initialize();
        }


        /*
         * Notes
         * https://github.com/filoe/cscore/tree/master/Samples/AudioPlayerSample
         * https://www.nuget.org/packages/NetDiscordRpc
         * 
         * using (var mmdeviceEnumerator = new MMDeviceEnumerator())
         *   {
                using (
                    var mmdeviceCollection = mmdeviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active))
                {
                    //foreach (var device in mmdeviceCollection)
            
         */
        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlayPauseButton.Content.ToString() == "Pause")
            {
                wasapi.Pause();
                PlayPauseButton.Content = "Play";
            } else
            {
                wasapi.Resume();
                PlayPauseButton.Content = "Pause";
            }
        }

        private void SongPickButton_Click(object sender, RoutedEventArgs e)
        {
            var filePicker = new FileOpenPicker() { SuggestedStartLocation = PickerLocationId.MusicLibrary };
            filePicker.FileTypeFilter.Add("*");
            // Get the current window's HWND by passing in the Window object
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            // Associate the HWND with the file picker
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);
            var task = filePicker.PickSingleFileAsync().AsTask();
            task.Wait();
            filePath = task.Result.Path;

            PlayNewSong();
        }

        private void PlayNewSong()
        {
            var waveSource = new OpusSource(File.OpenRead(filePath), 48000, 2);
            wasapi.Initialize(waveSource);
            wasapi.Play();
            PlayPauseButton.Content = "Pause";

            string fileName = filePath.Split('\\').Last().Split('.')[0];
            rpc.SetPresence(new NetDiscordRpc.RPC.RichPresence()
            {
                Details = fileName,
                State = "32bit 48kHz",
                Assets = new NetDiscordRpc.RPC.Assets()
                {
                    LargeImageKey = "https://i.imgur.com/IvGkGfr.png"
                }
            });

            rpc.Invoke();
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            wasapi.Stop();
        }
    }
}
