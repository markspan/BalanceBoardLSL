# ... (previous imports remain unchanged)

import hid
from pylsl import StreamInfo, StreamOutlet
from kivy.app import App
from kivy.core.window import Window
from kivy.uix.boxlayout import BoxLayout
from kivy.uix.button import Button
from kivy.uix.label import Label
from kivy.uix.scrollview import ScrollView
from kivy.clock import mainthread, Clock

import asyncio



class HidApp(App):
    """
    Kivy application for HID communication with USB devices.
    """

    def build(self):
        """
        Build the Kivy GUI.
        """
        Window.size = (400, 300)

        self.stop_event = asyncio.Event()

        self.devices_layout = BoxLayout(orientation='vertical')
        self.devices_scrollview = ScrollView()
        self.devices_scrollview.add_widget(self.devices_layout)

        self.scan_button = Button(text="Scan for Devices", size_hint=(1, 0.1))
        self.scan_button.bind(on_press=self.scan_for_hid_devices)

        self.cancel_button = Button(text="Cancel", size_hint=(1, 0.1))
        self.cancel_button.bind(on_press=self.cancel)

        self.root_layout = BoxLayout(orientation='vertical')
        self.root_layout.add_widget(self.scan_button)
        self.root_layout.add_widget(self.devices_scrollview)
        self.root_layout.add_widget(self.cancel_button)

        return self.root_layout

    def scan_for_hid_devices(self, instance):
        """
        Callback for the device scanner: cleans the interface and starts scanning for HID devices.
        """
        self.scan_button.disabled = True
        self.scan_hid_devices()

    def scan_hid_devices(self):
        """
        Scans for HID devices and adds them to the interface.
        """
        devices = hid.enumerate()
        for d in devices:
            self.add_hid_device_button(d)
        self.add_busy_label()

    @mainthread
    def add_hid_device_button(self, d):
        """
        Adds an HID device button to the interface.
        """
        device_button = Button(text=d['product_string'], size_hint=(1, 0.2))
        device_button.bind(on_press=lambda instance, device_info=d: self.connect_to_hid_device(device_info, instance))
        self.devices_layout.add_widget(device_button)

    @mainthread
    def add_busy_label(self):
        """
        Adds a busy label to the interface.
        """
        self.busyLabel = Label(text="", valign='middle')
        self.devices_layout.add_widget(self.busyLabel)
        self.busyvalue = 0;

    def connect_to_hid_device(self, device_info, instance):
        """
        Callback for the individual HID device buttons.
        Initiates the connection to the selected HID device.
        """
        self.stop_event.clear()
        instance.disabled = True
        self.loop = asyncio.new_event_loop()
        asyncio.set_event_loop(self.loop)
        self.loop.run_until_complete(self.read_hid_data(device_info))

    async def read_hid_data(self, device_info):
        """
        Reads data from the HID device and sends it as streams.
        """
        try:
            device = hid.device()
            device.open_path(device_info['path'])  # Open the selected HID device
            await self.send_hid_data_streams(device)
        except Exception as e:
            print(f"Error during HID data reading: {e}")

    async def send_hid_data_streams(self, device):
        """
        Sends HID device data as streams using asyncio tasks.
        """
        mocap_info = StreamInfo('MoCap', 'MoCap', 10, 100, 'float32', 'HID_MoCap')
        mocap_info.desc().append_child_value("manufacturer", "HID")
        mocap_channels = mocap_info.desc().append_child("channels")
        for i in range(10):
            mocap_channels.append_child("channel") \
                .append_child_value("name", f"Axis_{i}") \
                .append_child_value("unit", "unit") \
                .append_child_value("type", "MoCap")
    
        mocap_outlet = StreamOutlet(mocap_info)
        marker_info = StreamInfo('Markers', 'Markers', 1, 0, 'string', 'HID_Markers')
        marker_outlet = StreamOutlet(marker_info)
    
        # Adjust buffer size based on the expected maximum number of buttons and axes
        max_num_buttons = 12
        max_num_axes = 8
        buffer_size = max(20, max_num_buttons + max_num_axes + 1)
    
        def button_press_task(dt):
            """
            Task for detecting button presses and sending markers.
            """
            try:
                print("Button")
                report = device.get_feature_report(1, buffer_size)
                # if len(report) > 0:
                #     num_buttons = report[0]
                #     button_data = report[1:]
                #     pressed_buttons = [i + 1 for i, button in enumerate(button_data) if button != 0]
                #     if pressed_buttons:
                #         # Send markers for pressed buttons
                #         for button in pressed_buttons:
                #             marker_outlet.push_sample([str(button)])
            except Exception as e:
                print(f"Error during button press detection: {e}")
    
        def axis_record_task(dt):
            """
            Task for recording axis values and sending them in chunks.
            """
            try:
                print("Axis")
                # report = device.get_feature_report(0, buffer_size)
                # if len(report) > 0:
                #     num_axes = report[0]
                #     axis_data = report[1:]
                #     # Send axes data in chunks of 10 values per axis
                #     for i in range(0, len(axis_data), num_axes):
                #         chunk = axis_data[i:i + num_axes]
                #         mocap_outlet.push_sample([float(val) for val in chunk])
            except Exception as e:
                print(f"Error during axis recording: {e}")
    
        # Run the asyncio tasks
        Clock.schedule_interval(button_press_task, 0.55)  # Adjust the interval as needed
        Clock.schedule_interval(axis_record_task, 1)  # Adjust the interval as needed

    @mainthread
    def cancel(self, instance):
        """
        Stops the HID data acquisition and the application.
        """
        self.stop_event.set()
        App.get_running_app().stop()

if __name__ == "__main__":
    HidApp().run()
