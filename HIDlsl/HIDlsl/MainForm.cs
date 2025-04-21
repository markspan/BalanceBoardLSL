using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using HidLibrary;
using LSL;

namespace HIDlsl
{
    /// <summary>
    /// The main form of the application, which handles HID device interaction and LSL streaming.
    /// </summary>
    public partial class MainForm : Form
    {
        private volatile static bool Linked = false;
        private Thread? LSLThread;
        private readonly List<HidDevice> deviceList = new();
        private HidDevice? device;
        private liblsl.StreamOutlet? outlet;
        private List<CheckBox> checkBoxes = new();
        private Dictionary<string, int> inputReportOffsets = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="MainForm"/> class.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            InitializeDeviceList();
            InitializeUI();
        }

        /// <summary>
        /// Initializes the user interface components.
        /// </summary>
        private void InitializeUI()
        {
            BoardSelector.SelectedIndexChanged += BoardSelector_SelectedIndexChanged;
            LinkButton.Click += LinkButton_Click;
        }

        /// <summary>
        /// Handles the event when the selected index of the board selector changes.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void BoardSelector_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // Clear existing checkboxes
            Controls.Clear();
            checkBoxes.Clear();
            inputReportOffsets.Clear();

            // Get the selected device
            device = deviceList.ElementAt(BoardSelector.SelectedIndex);

            // Enumerate input reports
            var capabilities = device.Capabilities;
            int top = 50;
            int offset = 0;

            // Assuming each input report is 4 bytes (int32)
            int reportLength = capabilities.InputReportByteLength;
            int reportCount = reportLength / 4;

            for (int i = 0; i < reportCount; i++)
            {
                var checkBox = new CheckBox
                {
                    Text = $"Input Report {i}",
                    Top = top,
                    Left = 10
                };
                checkBoxes.Add(checkBox);
                inputReportOffsets[checkBox.Text] = offset;
                Controls.Add(checkBox);
                top += 20;
                offset += 4;
            }
        }

        /// <summary>
        /// Extracts the device name from the device path.
        /// </summary>
        /// <param name="devicePath">The device path.</param>
        /// <returns>The extracted device name.</returns>
        private string? ExtractDeviceName(string devicePath)
        {
            int lastIndex = devicePath.LastIndexOf('\\');
            if (lastIndex >= 0 && lastIndex < devicePath.Length - 1)
            {
                return devicePath.Substring(lastIndex + 1);
            }
            return null;
        }

        /// <summary>
        /// Initializes the device list with available HID devices.
        /// </summary>
        private void InitializeDeviceList()
        {
            var devices = HidDevices.Enumerate();

            foreach (var device in devices)
            {
                // Ensure device is not null before accessing its properties
                if (device != null && !string.IsNullOrEmpty(device.DevicePath))
                {
                    var deviceName = ExtractDeviceName(device.DevicePath);
                    // Ensure deviceName is not null before calling Contains
                    if (deviceName != null && deviceName.Contains("Adapted BalanceBoard"))
                    {
                        AddDeviceToList(device);
                    }
                }
            }

            if (deviceList.Count < 1)
            {
                DisplayNoDeviceError();
            }
        }

        /// <summary>
        /// Adds a device instance to the device list and updates the UI.
        /// </summary>
        /// <param name="device">The device instance to add.</param>
        private void AddDeviceToList(HidDevice device)
        {
            var deviceName = ExtractDeviceName(device.DevicePath);
            if (!string.IsNullOrEmpty(deviceName)) // Ensure deviceName is not null or empty
            {
                BoardSelector.Items.Add(deviceName);
                deviceList.Add(device);
                BoardSelector.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Displays an error message when no device is found.
        /// </summary>
        private void DisplayNoDeviceError()
        {
            LinkButton.Text = "No Board Attached!";
            LinkButton.Enabled = false;
            BackColor = Color.Pink;
        }

        /// <summary>
        /// Starts or stops the LSL thread based on the current link status.
        /// </summary>
        /// <param name="sender">The button that triggered the event.</param>
        private void StartLSL(Button sender)
        {
            if (!Linked)
            {
                Linked = true;
                sender.Text = "Unlink";
                BoardSelector.Enabled = false;
                device = deviceList.ElementAt(BoardSelector.SelectedIndex);
                LSLThread = new Thread(() => MainForBB(device));
                LSLThread?.Start();
            }
            else
            {
                Linked = false;
                BoardSelector.Enabled = true;
                sender.Text = "Link";
                Thread.Sleep(1000); // Give the thread time to stop broadcasting.
                LSLThread = null;
            }
        }

        /// <summary>
        /// Main method for handling the BalanceBoard device.
        /// </summary>
        /// <param name="device">The HID device.</param>
        private void MainForBB(HidDevice device)
        {
            device.OpenDevice();
            device.MonitorDeviceEvents = true;
            device.ReadReport(OnReadReport);

            var selectedChannels = checkBoxes.Where(cb => cb.Checked).Select(cb => cb.Text).ToList();
            var info = new liblsl.StreamInfo(ExtractDeviceName(device.DevicePath) + "(USB)", "Mocap", selectedChannels.Count, 100, liblsl.channel_format_t.cf_int32, "sddsfsdf");
            var setup = info.desc().append_child("setup");
            setup.append_child_value("Author", "M.M.Span");
            setup.append_child_value("Manufacturer", "University of Groningen");
            setup.append_child_value("Manual", "markspan.github.io");
            setup.append_child_value("Model", "Adapted USB WiiBalanceBoard");

            var channels = info.desc().append_child("channels");
            foreach (var channel in selectedChannels)
            {
                AddChannel(channels, channel, "Force", "kilograms");
            }

            outlet = new liblsl.StreamOutlet(info);

            while (Linked)
            {
                device?.ReadReport(OnReadReport);
            }
            System.GC.Collect();
            device?.CloseDevice();
        }

        /// <summary>
        /// Adds a channel to the LSL stream description.
        /// </summary>
        /// <param name="channels">The channels element.</param>
        /// <param name="label">The label of the channel.</param>
        /// <param name="type">The type of the channel.</param>
        /// <param name="unit">The unit of the channel.</param>
        private static void AddChannel(liblsl.XMLElement channels, string label, string type, string unit)
        {
            channels.append_child("channel")
                .append_child_value("label", label)
                .append_child_value("type", type)
                .append_child_value("unit", unit);
        }

        /// <summary>
        /// Handles the read report event.
        /// </summary>
        /// <param name="report">The report data.</param>
        private void OnReadReport(HidReport report)
        {
            if (report == null) return;

            var data = report.Data;
            var lslout = new int[checkBoxes.Count(cb => cb.Checked)];
            int index = 0;

            foreach (var checkBox in checkBoxes)
            {
                if (checkBox.Checked)
                {
                    int offset = inputReportOffsets[checkBox.Text];
                    lslout[index++] = BitConverter.ToInt32(data, offset);
                }
            }

            outlet?.push_sample(lslout);
        }

        /// <summary>
        /// Handles the Click event of the LinkButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void LinkButton_Click(object? sender, EventArgs e)
        {
            if (sender is Button button)
            {
                StartLSL(button);
            }
        }
    }
}
