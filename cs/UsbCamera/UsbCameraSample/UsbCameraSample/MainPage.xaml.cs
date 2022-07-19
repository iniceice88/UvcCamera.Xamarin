using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace UsbCameraSample
{
    public partial class MainPage
    {
        private readonly ICameraHelper _cameraHelper = CameraHelper.Instance;

        public MainPage()
        {
            InitializeComponent();

            _cameraHelper.UsbDeviceAttached += Instance_UsbDeviceAttached;
            _cameraHelper.UsbDeviceDetached += Instance_UsbDeviceDetached;

            UvcCameraView.CameraOpen += UvcCameraView_CameraOpen;
            UvcCameraView.CameraClose += UvcCameraView_CameraClose;

            UvcCameraView.RecordingStarted += UvcCameraView_RecordingStarted;
            UvcCameraView.RecordingStopped += UvcCameraView_RecordingStopped;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!await CheckPermissions())
                return;

            RefreshUsbDevices();
        }

        private void Instance_UsbDeviceAttached(object sender, UsbDeviceInfo e)
        {
            MainThread.BeginInvokeOnMainThread(RefreshUsbDevices);
        }

        private void Instance_UsbDeviceDetached(object sender, UsbDeviceInfo e)
        {
            MainThread.BeginInvokeOnMainThread(RefreshUsbDevices);
        }

        private void UvcCameraView_CameraOpen(object sender, EventArgs e)
        {
            BtnStartStop.Text = "Close";
            BtnSnapshot.IsVisible = BtnRecord.IsVisible = true;
            PanelSetting.IsEnabled = false;
        }

        private void UvcCameraView_CameraClose(object sender, EventArgs e)
        {
            BtnStartStop.Text = "Open";
            BtnSnapshot.IsVisible = BtnRecord.IsVisible = false;
            BtnRecord.Text = "Start Recording";
            PanelSetting.IsEnabled = true;
        }

        private void UvcCameraView_RecordingStarted(object sender, string e)
        {
            BtnRecord.Text = "Stop Recording";
        }

        private void UvcCameraView_RecordingStopped(object sender, EventArgs e)
        {
            BtnRecord.Text = "Start Recording";
        }

        private void RefreshUsbDevices()
        {
            var usbDevices = _cameraHelper.ListUsbDevices();
            var selectedDevice = PickerCamera.SelectedItem as UsbDeviceInfo;
            PickerCamera.ItemsSource = usbDevices;
            if (usbDevices.Length == 0) return;

            if (selectedDevice != null)
            {
                var prevIdx = usbDevices.IndexOf(x => x.DeviceId == selectedDevice.DeviceId);
                if (prevIdx >= 0)
                    PickerCamera.SelectedIndex = prevIdx;
            }

            if (PickerCamera.SelectedIndex < 0)
                PickerCamera.SelectedIndex = 0;
        }

        private async void BtnStartStop_OnClicked(object sender, EventArgs e)
        {
            if (!UvcCameraView.IsOpen)
            {
                var selectedDevice = PickerCamera.SelectedItem as UsbDeviceInfo;
                var selectedSize = PickerResolution.SelectedItem as CameraSize;
                if (selectedDevice == null || selectedSize == null) return;

                if (await CheckPermissions() != true)
                    return;

                UvcCameraView.FlipHorizontally = ChkFlipHorizontally.IsChecked;
                UvcCameraView.FlipVertically = ChkFlipVertically.IsChecked;
                UvcCameraView.VideoRotation = (int)PickerRotation.SelectedItem;

                UvcCameraView.Open(selectedDevice.DeviceId, selectedSize.Width, selectedSize.Height);
            }
            else
            {
                UvcCameraView.Close();
            }
        }

        private void BtnRecord_OnClicked(object sender, EventArgs e)
        {
            if (!UvcCameraView.IsRecording)
            {
                var videoFile = Path.Combine(_cameraHelper.VideoRootDir, GetUid() + ".mp4");
                Console.WriteLine("StartRecording:" + videoFile);
                UvcCameraView.StartRecording(videoFile);
            }
            else
            {
                UvcCameraView.StopRecording();
            }
        }

        private void BtnSnapshot_OnClicked(object sender, EventArgs e)
        {
            var imageFile = Path.Combine(_cameraHelper.PhotoRootDir, GetUid() + ".jpg");
            UvcCameraView.TakeSnapshot(imageFile);
        }

        private async Task<bool> CheckPermissions()
        {
            var status = await Permissions.RequestAsync<Permissions.StorageWrite>();
            if (status != PermissionStatus.Granted)
                return false;

            status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
                return false;

            return true;
        }

        private void PickerCamera_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedDevice = PickerCamera.SelectedItem as UsbDeviceInfo;
            if (selectedDevice == null)
            {
                PickerResolution.ItemsSource = null;
                return;
            }

            var sizes = _cameraHelper.GetCameraSupportedSizes(selectedDevice.DeviceId);
            if (sizes == null) return;

            var oldSize = PickerResolution.SelectedItem as CameraSize;
            PickerResolution.ItemsSource = sizes;
            if (sizes.Length == 0)
                return;
            if (oldSize != null)
            {
                var prevIdx = sizes.IndexOf(x => x.ToString() == oldSize.ToString());
                if (prevIdx >= 0)
                    PickerResolution.SelectedIndex = prevIdx;
            }

            if (PickerResolution.SelectedIndex < 0)
                PickerResolution.SelectedIndex = 0;
        }

        private string GetUid()
        {
            return DateTime.Now.ToString("yyyyMMdd_HHmmss");
        }
    }
}
