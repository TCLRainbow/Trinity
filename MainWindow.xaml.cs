// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using CSCore.Codecs;
using CSCore;
using CSCore.CoreAudioAPI;
using CSCore.SoundOut;
using Microsoft.UI.Xaml;
using System;
using CSCore.Codecs.OPUS;
using System.IO;
using NetDiscordRpc;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Trinity
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            myButton.Content = "Clicked";
            string file = "test.opus";
            using (var mmdeviceEnumerator = new MMDeviceEnumerator())
            {
                using (
                    var mmdeviceCollection = mmdeviceEnumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active))
                {
                    //foreach (var device in mmdeviceCollection)
                    var device = mmdeviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                    var soundOut = new WasapiOut() { Device = device };
                    var waveSource = new OpusSource(File.OpenRead(file), 48000, 2);
                    soundOut.Initialize(waveSource);

                    soundOut.Play();


                    var rpc = new DiscordRPC("757258842328399944");
                    rpc.Initialize();
                    rpc.SetPresence(new NetDiscordRpc.RPC.RichPresence()
                    {
                        Details = "Details",
                        State = "State"
                    });

                    rpc.Invoke();
                }
            }
        }
    }
}
