using System;
using Xamarin.Forms;

namespace UsbCameraSample
{
    public class UvcCameraView : Xamarin.Forms.View
    {
        public int PreviewHeight { get; private set; }

        public int PreviewWidth { get; private set; }

        public int DeviceId { get; private set; }

        public event EventHandler<RequestOpenArgs> OpenRequested;
        public event EventHandler CloseRequested;
        public event EventHandler<RequestStartRecordingArgs> StartRecordingRequested;
        public event EventHandler StopRecordingRequested;
        public event EventHandler<RequestTakeSnapshotArgs> TakeSnapshotRequested;
        public static readonly BindableProperty VideoRotationProperty =
            BindableProperty.Create(nameof(VideoRotation), typeof(int), typeof(UvcCameraView), 0);


        public event EventHandler<string> RecordingStarted;
        public event EventHandler RecordingStopped;
        public event EventHandler CameraOpen;
        public event EventHandler CameraClose;

        public int VideoRotation
        {
            set => SetValue(VideoRotationProperty, value);
            get => (int)GetValue(VideoRotationProperty);
        }


        public static readonly BindableProperty FlipHorizontallyProperty =
            BindableProperty.Create(nameof(FlipHorizontally), typeof(bool), typeof(UvcCameraView), false);

        public bool FlipHorizontally
        {
            set => SetValue(FlipHorizontallyProperty, value);
            get => (bool)GetValue(FlipHorizontallyProperty);
        }

        public static readonly BindableProperty FlipVerticallyProperty =
            BindableProperty.Create(nameof(FlipVertically), typeof(bool), typeof(UvcCameraView), false);

        public bool FlipVertically
        {
            set => SetValue(FlipVerticallyProperty, value);
            get => (bool)GetValue(FlipVerticallyProperty);
        }

        public bool IsOpen { get; private set; }

        public bool IsRecording { get; private set; }

        public void Open(int deviceId, int width, int height)
        {
            this.DeviceId = deviceId;
            this.PreviewHeight = height;
            this.PreviewWidth = width;
            OpenRequested?.Invoke(this, new RequestOpenArgs(deviceId, width, height));
            IsOpen = true;
            CameraOpen?.Invoke(this, EventArgs.Empty);
        }

        public void Close()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
            IsOpen = false;
            CameraClose?.Invoke(this, EventArgs.Empty);
        }

        public void StartRecording(string videoPath)
        {
            StartRecordingRequested?.Invoke(this, new RequestStartRecordingArgs(videoPath));
            IsRecording = true;
            RecordingStarted?.Invoke(this, videoPath);
        }

        public void StopRecording()
        {
            StopRecordingRequested?.Invoke(this, EventArgs.Empty);
            IsRecording = false;
            RecordingStopped?.Invoke(this, EventArgs.Empty);
        }

        public void TakeSnapshot(string imgFilePath)
        {
            TakeSnapshotRequested?.Invoke(this, new RequestTakeSnapshotArgs(imgFilePath));
        }

        public void OnCameraDisconnected()
        {
            if (IsRecording)
            {
                IsRecording = false;
                RecordingStopped?.Invoke(this, EventArgs.Empty);
            }

            if (IsOpen)
            {
                IsOpen = false;
                CameraClose?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public class RequestOpenArgs
    {
        public RequestOpenArgs(int deviceId, int previewWidth, int previewHeight)
        {
            DeviceId = deviceId;
            PreviewWidth = previewWidth;
            PreviewHeight = previewHeight;
        }

        public int PreviewHeight { get; }

        public int PreviewWidth { get; }

        public int DeviceId { get; }
    }

    public class RequestStartRecordingArgs
    {
        public RequestStartRecordingArgs(string videoPath)
        {
            VideoPath = videoPath;
        }

        public string VideoPath { get; }
    }

    public class RequestTakeSnapshotArgs
    {
        public RequestTakeSnapshotArgs(string imagePath)
        {
            ImagePath = imagePath;
        }

        public string ImagePath { get; }
    }

    public interface ICameraHelper
    {
        string PhotoRootDir { get; }

        string VideoRootDir { get; }

        UsbDeviceInfo[] ListUsbDevices();

        CameraSize[] GetCameraSupportedSizes(int deviceId);

        event EventHandler<UsbDeviceInfo> UsbDeviceAttached;

        event EventHandler<UsbDeviceInfo> UsbDeviceDetached;
    }

    public static class CameraHelper
    {
        public static ICameraHelper Instance { get; set; }
    }

    public class UsbDeviceInfo
    {
        public int VendorId { get; set; }

        // public string SerialNumber { get; set; }

        public string ProductName { get; set; }

        public int ProductId { get; set; }

        public string ManufacturerName { get; set; }

        public string DeviceName { get; set; }

        public int DeviceId { get; set; }

        public string DisplayName => !string.IsNullOrEmpty(ProductName) ? ProductName : DeviceName;
    }

    public class CameraSize
    {
        public int Width { get; set; }

        public int Height { get; set; }

        public override string ToString()
        {
            return $"{Width} X {Height}";
        }
    }
}