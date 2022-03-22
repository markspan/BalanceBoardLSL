using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System;
using System.Diagnostics;

using SharpDX.DirectInput;
using LSL;

namespace HIDlsl
{
    public partial class MainForm : Form
    {
        static Boolean Linked = false;
        static Thread? LSLThread;

        public MainForm()
        {
            InitializeComponent();
        }
        
        static void StartLSL(Button sender)
        {
            if (Linked == false)
            {
                Linked = true;
                sender.Text = "Unlink";
                LSLThread = new Thread(MainForJoystick);
                LSLThread.Start();
            }
            else
            {
                Linked = false;
                sender.Text = "Link";
            }
        }
        private static void  MainForJoystick()
        {
            // Initialize LSL:
            liblsl.StreamInfo info = new("BalanceBoard (USB)", "Mocap", 5, 100, liblsl.channel_format_t.cf_float32, "sddsfsdf");
            liblsl.XMLElement Setup =  info.desc().append_child("Setup");

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

            // Initialize DirectInput
            var directInput = new DirectInput();

            // Find a Joystick Guid
            var joystickGuid = Guid.Empty;

            foreach (var deviceInstance in directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices))
                joystickGuid = deviceInstance.InstanceGuid;

            // If Gamepad not found, look for a Joystick
            if (joystickGuid == Guid.Empty)
                foreach (var deviceInstance in directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices))
                    joystickGuid = deviceInstance.InstanceGuid;

            // If Joystick not found, throws an error
            if (joystickGuid == Guid.Empty)
            {
                return;
            }

            // Instantiate the joystick
            var joystick = new Joystick(directInput, joystickGuid);

            // Set BufferSize in order to use buffered data.
            joystick.Properties.BufferSize = 128;

            // Acquire the joystick
            joystick.Acquire();
            float[] lslout = new float[5];
            
            // Poll events from joystick
            while (Linked == true)
            {
                //joystick.Poll() ;
                Stopwatch stopwatch = Stopwatch.StartNew();
                var datas = joystick.GetBufferedData();
                foreach (var state in datas)
                {
                    switch (state.Offset)
                    {
                        case JoystickOffset.X:
                            lslout[0] = (float)(Convert.ToSingle(state.Value) / 575.0);
                            break;
                        case JoystickOffset.Y:
                            lslout[1] =  (float)(Convert.ToSingle(state.Value) / 575.0);
                            break;
                        case JoystickOffset.Z:
                            lslout[2] = (float)(Convert.ToSingle(state.Value) / 575.0);
                            break;
                        case JoystickOffset.RotationX:
                            lslout[3] = (float)(Convert.ToSingle(state.Value) / 575.0);
                            break;
                        case JoystickOffset.RotationY:
                            lslout[4] = state.Value;
                            break;
                        default:
                            break;
                    }
                }
                
                while (stopwatch.ElapsedMilliseconds < 9)
                    Thread.Sleep(1);

                outlet.push_sample(lslout);
            }
        }

        private void LinkButton_Click(object sender, EventArgs e)
        {
            StartLSL((Button)sender);
        }
    }
 }