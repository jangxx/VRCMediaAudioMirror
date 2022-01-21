using System;
using UIExpansionKit.API;
using MelonLoader;
using System.Collections.Generic;
using NAudio.Wave;

namespace VRCMediaAudioMirror
{
    public class OutputDeviceEntry : Tuple<int, string>
    {
        public OutputDeviceEntry(int item1, string item2) : base(item1, item2) { }
    }

    public class AudioDeviceChangedEventArgs : EventArgs
    {
        public int DeviceNumber { get; set; }
        public bool Disable { get; set; }
    }

    public class QuickMenuSettings
    {
        public event EventHandler AudioDeviceChangedEvent;

        private MelonLogger.Instance LoggerInstance = new MelonLogger.Instance("Player_Audio_Mirror");
        private List<OutputDeviceEntry> outputDevices = new List<OutputDeviceEntry>();
        private int? selectedDevice = null;

        public void Init()
        {
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Audio Mirror", OpenMenu);

            UpdateOutputDevices();
        }

        public void UpdateOutputDevices()
        {
            this.outputDevices = new List<OutputDeviceEntry>();

            this.outputDevices.Add(new OutputDeviceEntry(-2, "Disable"));
            this.outputDevices.Add(new OutputDeviceEntry(-1, "Default Device"));

            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                var capabilities = WaveOut.GetCapabilities(i);
                this.outputDevices.Add(new OutputDeviceEntry(i, capabilities.ProductName));
            }
        }

        private void OpenMenu()
        {
            UpdateOutputDevices();

            var controlMenu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu2Column);

            foreach(var entry in outputDevices)
            {
                if (entry.Item1 == -2)
                {
                    controlMenu.AddSimpleButton("Disable", () =>
                    {
                        LoggerInstance.Msg("Disabling audio mirroring");
                        this.selectedDevice = null;
                        var args = new AudioDeviceChangedEventArgs();
                        args.Disable = true;

                        EventHandler handler = AudioDeviceChangedEvent;
                        handler?.Invoke(this, args);

                        // update menu
                        controlMenu.Hide();
                        OpenMenu();
                    });
                }
                else
                {
                    controlMenu.AddSimpleButton(entry.Item2, () =>
                    {
                        LoggerInstance.Msg("Trying to change audio device to " + entry.Item2 + " (" + entry.Item1 + ")");
                        this.selectedDevice = entry.Item1;
                        var args = new AudioDeviceChangedEventArgs();
                        args.DeviceNumber = entry.Item1;
                        args.Disable = false;

                        EventHandler handler = AudioDeviceChangedEvent;
                        handler?.Invoke(this, args);

                        // update menu
                        controlMenu.Hide();
                        OpenMenu();
                    });
                }
            }

            if (outputDevices.Count % 2 == 1)
            {
                controlMenu.AddSpacer(); // add one additional one to balance everything out
            }

            controlMenu.AddLabel("Current Device:");
            if (selectedDevice == null)
            {
                controlMenu.AddLabel("None");
            }
            else
            {
                foreach (var entry in outputDevices)
                {
                    if (entry.Item1 == selectedDevice)
                    {
                        controlMenu.AddLabel(entry.Item2);
                        break;
                    }
                }
            }

            controlMenu.AddSimpleButton("Refresh", () =>
            {
                controlMenu.Hide();
                UpdateOutputDevices();
                OpenMenu();
            });

            controlMenu.AddSimpleButton("Close", () =>
            {
                controlMenu.Hide();
            });

            controlMenu.Show();
        }
    }
}

namespace UIExpansionKit.API
{
    public struct LayoutDescriptionCustom
    {
        public static LayoutDescription QuickMenu2Column = new LayoutDescription { NumColumns = 2, RowHeight = 380 / 8, NumRows = 8 };
    }
}