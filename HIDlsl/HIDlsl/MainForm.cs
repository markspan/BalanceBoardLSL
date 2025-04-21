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
    public partial class MainForm : Form
    {
        private volatile static bool Linked = false;
        private Thread? LSLThread;
        private HidDevice? hidDevice;
        private readonly List<HidDevice> deviceList = new();
        private HidDevice? device;
        private liblsl.StreamOutlet? outlet;
        private List<CheckBox> checkBoxes = new();
        private Dictionary<string, int> inputReportOffsets = new();

        public MainForm()
        {
            InitializeComponent();
            InitializeDeviceList();
            InitializeUI();
        }

        private void InitializeUI()
        {
            BoardSelector.SelectedIndexChanged += BoardSelector_SelectedIndexChanged;
            LinkButton.Click += LinkButton_Click;
        }

        private void BoardSelector_SelectedIndexChanged(object sender, EventArgs e)
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

        private string? ExtractDeviceName(string devicePath)
        {
            int lastIndex = devicePath.LastIndexOf('\\');
            if (lastIndex >= 0 && lastIndex < devicePath.Length - 1)
            {
                return devicePath.Substring(lastIndex + 1);
            }
            return null;
        }

        private void InitializeDeviceList()
        {
            var devices = HidDevices.Enumerate();

            foreach (var device in devices)
            {
                if (ExtractDeviceName(device.DevicePath).Contains("Adapted BalanceBoard"))
                {
                    AddDeviceToList(device);
                }
            }

            if (deviceList.Count < 1)
            {
                DisplayNoDeviceError();
            }
        }

        private void AddDeviceToList(HidDevice device)
        {
            BoardSelector.Items.Add(ExtractDeviceName(device.DevicePath));
            deviceList.Add(device);
            BoardSelector.SelectedIndex = 0;
        }

        private void DisplayNoDeviceError()
        {
            LinkButton.Text = "No Board Attached!";
            LinkButton.Enabled = false;
            BackColor = Color.Pink;
        }

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
                Thread.Sleep(1000);
                LSLThread = null;
            }
        }

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

        private static void AddChannel(liblsl.XMLElement channels, string label, string type, string unit)
        {
            channels.append_child("channel")
                .append_child_value("label", label)
                .append_child_value("type", type)
                .append_child_value("unit", unit);
        }

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

        private void LinkButton_Click(object sender, EventArgs e)
        {
            StartLSL((Button)sender);
        }
    }
}
