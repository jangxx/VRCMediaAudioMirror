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

    public class StatusMenuStatus
    {
        public int CurrentlyHooked = 0;
    }

    public class AudioDeviceChangedEventArgs : EventArgs
    {
        public int DeviceNumber { get; set; }
        public bool Disable { get; set; }
    }

    public class MenuInteractionEventArgs : EventArgs
    {
        public enum MenuInteractionType
        {
            TRIGGER_UPDATE,
            UNHOOK_ALL,
        };

        public MenuInteractionType Type { get; set; }

        public int[] IntParams { get; set; }
        public bool[] BoolParams { get; set; }
    }

    public class QuickMenuSettings
    {
        public event EventHandler AudioDeviceChangedEvent;
        public event EventHandler MenuInteractionEvent;

        private MelonLogger.Instance LoggerInstance = new MelonLogger.Instance("Media_Audio_Mirror");
        private List<OutputDeviceEntry> outputDevices = new List<OutputDeviceEntry>();
        private StatusMenuStatus statusMenuStatus = new StatusMenuStatus();
        private int? selectedDevice = null;
        private ICustomShowableLayoutedMenu statusMenu;

        private bool statusMenu_filterDisabled = false;

        public void Init()
        {
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Audio Mirror Setup", OpenSetupMenu);
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Audio Mirror Status", OpenStatusMenu);

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

        public void UpdateStatusMenuStatus(StatusMenuStatus status)
        {
            this.statusMenuStatus = status;

            if (statusMenu != null)
            {
                statusMenu.Hide();
                OpenStatusMenu();
            }
        }

        private void OpenStatusMenu()
        {
            statusMenu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu2ColumnSmaller);

            statusMenu.AddLabel("Hooked AudioSources:");
            statusMenu.AddLabel(statusMenuStatus.CurrentlyHooked.ToString());

            statusMenu.AddToggleButton("(Disable Filter)", (setting) => { statusMenu_filterDisabled = setting; }, () => statusMenu_filterDisabled);
            statusMenu.AddSimpleButton("Retry hooking", () =>
            {
                LoggerInstance.Msg("Manually retrying hooking");
                var args = new MenuInteractionEventArgs();
                args.Type = MenuInteractionEventArgs.MenuInteractionType.TRIGGER_UPDATE;

                args.BoolParams = new bool[] { statusMenu_filterDisabled };

                EventHandler handler = MenuInteractionEvent;
                handler?.Invoke(this, args);
            });

            statusMenu.AddSpacer();

            statusMenu.AddSimpleButton("Unhook All", () =>
            {
                LoggerInstance.Msg("Removing all filters");
                var args = new MenuInteractionEventArgs();
                args.Type = MenuInteractionEventArgs.MenuInteractionType.UNHOOK_ALL;

                EventHandler handler = MenuInteractionEvent;
                handler?.Invoke(this, args);
            });

            statusMenu.AddSpacer();
            statusMenu.AddSimpleButton("Close", () =>
            {
                statusMenu.Hide();
                statusMenu = null;
            });

            statusMenu.Show();
        }

        private void OpenSetupMenu()
        {
            UpdateOutputDevices();

            var setupMenu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu2Column);

            foreach(var entry in outputDevices)
            {
                if (entry.Item1 == -2)
                {
                    setupMenu.AddSimpleButton("Disable", () =>
                    {
                        LoggerInstance.Msg("Disabling audio mirroring");
                        this.selectedDevice = null;
                        var args = new AudioDeviceChangedEventArgs();
                        args.Disable = true;

                        EventHandler handler = AudioDeviceChangedEvent;
                        handler?.Invoke(this, args);

                        // update menu
                        setupMenu.Hide();
                        OpenSetupMenu();
                    });
                }
                else
                {
                    setupMenu.AddSimpleButton(entry.Item2, () =>
                    {
                        LoggerInstance.Msg("Trying to change audio device to " + entry.Item2 + " (" + entry.Item1 + ")");
                        this.selectedDevice = entry.Item1;
                        var args = new AudioDeviceChangedEventArgs();
                        args.DeviceNumber = entry.Item1;
                        args.Disable = false;

                        EventHandler handler = AudioDeviceChangedEvent;
                        handler?.Invoke(this, args);

                        // update menu
                        setupMenu.Hide();
                        OpenSetupMenu();
                    });
                }
            }

            if (outputDevices.Count % 2 == 1)
            {
                setupMenu.AddSpacer(); // add one additional one to balance everything out
            }

            setupMenu.AddLabel("Current Device:");
            if (selectedDevice == null)
            {
                setupMenu.AddLabel("None");
            }
            else
            {
                foreach (var entry in outputDevices)
                {
                    if (entry.Item1 == selectedDevice)
                    {
                        setupMenu.AddLabel(entry.Item2);
                        break;
                    }
                }
            }

            setupMenu.AddSimpleButton("Refresh", () =>
            {
                setupMenu.Hide();
                UpdateOutputDevices();
                OpenSetupMenu();
            });

            setupMenu.AddSimpleButton("Close", () =>
            {
                setupMenu.Hide();
            });

            setupMenu.Show();
        }
    }
}

namespace UIExpansionKit.API
{
    public struct LayoutDescriptionCustom
    {
        public static LayoutDescription QuickMenu2Column = new LayoutDescription { NumColumns = 2, RowHeight = 380 / 8, NumRows = 8 };
        public static LayoutDescription QuickMenu2ColumnSmaller = new LayoutDescription { NumColumns = 2, RowHeight = 380 / 8, NumRows = 5 };
    }
}