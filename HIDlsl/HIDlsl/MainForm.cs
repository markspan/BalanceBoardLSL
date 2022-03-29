
//using System.Reactive.Linq;
using LSL;
using SharpDX.DirectInput;

/*
 * To read the XDF data correctly into MATLAB use load_xdf, and then do:
 * 
 * plot(double(bitshift(uint32(data'), -8)) / 2.7). 
 * 
 * The resulting data should be in kg then (approximately)
 */

namespace HIDlsl
{
    public partial class MainForm : Form
    {
        volatile static Boolean Linked = false;
        Thread? LSLThread;
        Joystick? joystick;
        readonly DirectInput? directInput;
        List<DeviceInstance>? deviceList;
        static DeviceInstance? device;

        public MainForm()
        {
            InitializeComponent();

            directInput = new();
            deviceList = new();

            // Find a Joystick Guid
            foreach (var deviceInstance in directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices))
                if (deviceInstance.ProductGuid.ToString().Contains("1b6f") && deviceInstance.ProductGuid.ToString().Contains("9206")) // Guid for the Adapted BalanceBoard
                {
                    this.BoardSelector.Items.Add(deviceInstance.ProductName);
                    deviceList.Add(deviceInstance);
                    this.BoardSelector.SelectedIndex++;
                }


            // If Joystick not found, throws an error
            if (deviceList.Count < 1)
            {
                this.LinkButton.Text = "No Board Attached!";
                this.LinkButton.Enabled = false;
                this.BackColor = Color.Pink;
            }
        }

        void StartLSL(Button sender)
        {
            if (Linked == false)
            {
                Linked = true;
                sender.Text = "Unlink";
                device = deviceList.ElementAt(BoardSelector.SelectedIndex);
                LSLThread = new Thread(() => MainForJoystick((Guid)device.InstanceGuid));
                LSLThread.Start();
            }
            else
            {
                Linked = false;
                sender.Text = "Link";
                // give the thread tiume to stop broadcasting.
                Thread.Sleep(1000);
                LSLThread = null;
            }
        }
        private void MainForJoystick(Guid Board)
        {

            joystick = new Joystick(directInput, Board);

            // Set BufferSize in order to use buffered data.
            joystick.Properties.BufferSize = 20;

            // Acquire the joystick
            joystick.Acquire();

            // Initialize LSL:
            liblsl.StreamInfo info = new(device.ProductName + "(USB)", "Mocap", 5, 100, liblsl.channel_format_t.cf_int32, "sddsfsdf");
            liblsl.XMLElement Setup = info.desc().append_child("Setup");

            Setup.append_child_value("Author", "M.M.Span");
            Setup.append_child_value("Manifacturer", "University of Groningen");
            Setup.append_child_value("Manual", "markspan.github.io");
            Setup.append_child_value("Model", "Adapted USB WiiBalanceBoard");

            liblsl.XMLElement Channels = info.desc().append_child("Channels");
            Channels.append_child("channel")
                .append_child_value("label", "Weight_BottomLeft")
                .append_child_value("type", "Force")
                .append_child_value("unit", "kilograms");
            Channels.append_child("channel")
                .append_child_value("label", "Weight_TopLeft")
                .append_child_value("type", "Force")
                .append_child_value("unit", "kilograms");
            Channels.append_child("channel")
                .append_child_value("label", "Weight_TopRight")
                .append_child_value("type", "Force")
                .append_child_value("unit", "kilograms");
            Channels.append_child("channel")
                .append_child_value("label", "Weight_BottomRight")
                .append_child_value("type", "Force")
                .append_child_value("unit", "kilograms");
            Channels.append_child("channel")
                .append_child_value("label", "SawTooth")
                .append_child_value("type", "Calibration")
                .append_child_value("unit", "Arbitrary units");

            liblsl.StreamOutlet outlet = new(info);

            int[] lslout = new int[5];
            bool newdata = false;
            int saw = 0;
            // Poll events from joystick
            while (Linked == true)
            {
                while (newdata == false)
                {
                    var datas = joystick.GetBufferedData();
                    foreach (var state in datas)
                    {
                        switch (state.Offset)
                        {
                            case JoystickOffset.X:
                                lslout[0] = state.Value;
                                break;
                            case JoystickOffset.Y:
                                lslout[1] = state.Value;
                                break;
                            case JoystickOffset.Z:
                                lslout[2] = state.Value;
                                break;
                            case JoystickOffset.RotationX:
                                lslout[3] = state.Value;
                                break;
                            case JoystickOffset.RotationY:
                                lslout[4] = ++saw;
                                if (saw >= 100)
                                    saw = 0;
                                newdata = true;
                                break;
                            default:
                                break;
                        }
                    }
                }
                outlet.push_sample(lslout);
                newdata = false;
            }
            System.GC.Collect();
        }

        private void LinkButton_Click(object sender, EventArgs e)
        {
            StartLSL((Button)sender);
        }
    }
}