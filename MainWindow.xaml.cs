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
using NetDiscordRpc.RPC;

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
        private static DiscordRPC rpc = new("757258842328399944");
        private static bool loop = true;
        private static string filePath;

        public MainWindow()
        {
            InitializeComponent();
            rpc.Initialize();
            wasapi.Stopped += Wasapi_Stopped;
        }


        /*
         * Notes
         * https://github.com/filoe/cscore/tree/master/Samples/AudioPlayerSample
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
            var file = new FileInfo(filePath);
            var src = new OpusSource(file.OpenRead(), 48000, 2);

            wasapi.Initialize(src);
            wasapi.Play();
            PlayPauseButton.Content = "Pause";

            var bit = src.WaveFormat.BitsPerSample * src.WaveFormat.Channels;
            var sampleRate = src.WaveFormat.SampleRate / 1000;
            var bps = (file.Length - src.WaveFormat.ExtraSize) / (src.Length / 200000) / 125;


            string fileName = filePath.Split('\\').Last().Split('.')[0];
            rpc.SetPresence(new RichPresence()
            {

                Details = fileName,
                State = $"{bit}bit {sampleRate}kHz {bps}kbps",
                Timestamps = Timestamps.Now,
                Assets = new Assets()
                {
                    LargeImageKey = "https://i.imgur.com/IvGkGfr.png"
                }
            });
            rpc.Invoke();
        }

        private void Wasapi_Stopped(object sender, PlaybackStoppedEventArgs e)
        {
            if (loop)
            {
                wasapi.Initialize(new OpusSource(File.OpenRead(filePath), 48000, 2));
                wasapi.Play();
                rpc.UpdateTimestamps(Timestamps.Now);
            }
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            wasapi.Stop();
        }
    }
}
