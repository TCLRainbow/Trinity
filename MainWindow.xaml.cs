// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CSCore.CoreAudioAPI;
using CSCore.SoundOut;
using Microsoft.UI.Xaml;
using CSCore.Codecs.OPUS;
using System.IO;
using NetDiscordRpc;
using System.Linq;

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

        public MainWindow()
        {
            InitializeComponent();
        }


        /*
         * Notes
         * https://github.com/filoe/cscore/tree/master/Samples/AudioPlayerSample
         * https://www.nuget.org/packages/NetDiscordRpc
         */
        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            myButton.Content = "Clicked";
            string filePath = "test.opus";
            string fileName = filePath.Split('\\').Last().Split('.')[0];
            /*using (var mmdeviceEnumerator = new MMDeviceEnumerator())
            {
                using (
                    var mmdeviceCollection = mmdeviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active))
                {
                    //foreach (var device in mmdeviceCollection)
            */
                    var waveSource = new OpusSource(File.OpenRead(filePath), 48000, 2);
                    wasapi.Initialize(waveSource);
                    wasapi.Play();


                    var rpc = new DiscordRPC("757258842328399944");
                    rpc.Initialize();
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
