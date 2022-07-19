using System;
using System.Collections.Generic;
using System.Linq;
using Android.Hardware.Usb;
using Com.Jiangdg.Usbcamera;
using Com.Serenegiant.Usb;
using Com.Serenegiant.Usb.Common;
using Com.Serenegiant.Usb.Encoder;
using UsbCameraSample.Droid.Renders;
using Xamarin.Forms.Platform.Android;

namespace UsbCameraSample.Droid
{
    public class CameraHelperImpl : ICameraHelper
    {
        private readonly USBMonitor _usbMonitor;

        private readonly Dictionary<int, CameraSize[]> _cameraSizesCache = new Dictionary<int, CameraSize[]>();

        public CameraHelperImpl()
        {
            var listener = new DevConnectListener(this);
            _usbMonitor = new USBMonitor(MainActivity.Instance, listener);
            _usbMonitor.Register();
        }

        public string PhotoRootDir { get; set; }

        public string VideoRootDir { get; set; }

        public UsbDeviceInfo[] ListUsbDevices()
        {
            return _usbMonitor.DeviceList
                .Where(MyUvcCameraHelper.IsValidCameraDevice)
                .Select(MyUvcCameraHelper.FromAUsbDevice)
                .ToArray();
        }

        public CameraSize[] GetCameraSupportedSizes(int deviceId)
        {
            if (_cameraSizesCache.TryGetValue(deviceId, out var sizes))
                return sizes;

            var usbDevice = _usbMonitor.DeviceList.FirstOrDefault(x => x.DeviceId == deviceId);
            if (usbDevice == null) return null;

            try
            {
                var block = _usbMonitor.OpenDevice(usbDevice);
                var uvcCamera = new UVCCamera();
                uvcCamera.Open(block);

                sizes = uvcCamera.SupportedSizeList
                    .Select(x => new CameraSize
                    {
                        Width = x.Width,
                        Height = x.Height
                    }).ToArray();
                uvcCamera.Close();

                _cameraSizesCache[deviceId] = sizes;
                return sizes;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }

        public event EventHandler<UsbDeviceInfo> UsbDeviceAttached;

        public event EventHandler<UsbDeviceInfo> UsbDeviceDetached;

        class DevConnectListener : Java.Lang.Object, USBMonitor.IOnDeviceConnectListener
        {
            private readonly CameraHelperImpl _parent;

            public DevConnectListener(CameraHelperImpl parent)
            {
                _parent = parent;
            }

            public void OnAttach(UsbDevice device)
            {
                Console.WriteLine("OnAttach:" + device);
                if (!MyUvcCameraHelper.IsValidCameraDevice(device)) return;

                _parent._usbMonitor.RequestPermission(device);
            }

            public void OnCancel(UsbDevice device)
            {
                Console.WriteLine("OnCancel:" + device);
            }

            public void OnConnect(UsbDevice device, USBMonitor.UsbControlBlock block, bool createNew)
            {
                Console.WriteLine("OnConnect:" + device);
                if (!MyUvcCameraHelper.IsValidCameraDevice(device)) return;

                _parent.UsbDeviceAttached?.Invoke(_parent, MyUvcCameraHelper.FromAUsbDevice(device));
            }

            public void OnDetach(UsbDevice device)
            {
                Console.WriteLine("OnDetach:" + device);
                if (!MyUvcCameraHelper.IsValidCameraDevice(device)) return;
                _parent.UsbDeviceDetached?.Invoke(_parent, MyUvcCameraHelper.FromAUsbDevice(device));
            }

            public void OnDisconnect(UsbDevice device, USBMonitor.UsbControlBlock block)
            {
                Console.WriteLine("OnDisconnect:" + device);
            }
        }
    }

    public class MyUvcCameraHelper
    {
        internal static UVCCameraHelper ACameraHelper;
        public static void Setup(UvcCameraView cameraView, UvcCameraSetupOptions options)
        {
            var render = cameraView.GetRenderer() as UvcCameraRender;
            if (render == null) return;

            ACameraHelper = UVCCameraHelper.GetInstance(options.PreviewWidth, options.PreviewHeight);
            if (options.VideoRotation > 0)
            {
                ACameraHelper.CameraAngle = 360 - options.VideoRotation;
            }
            else
            {
                ACameraHelper.CameraAngle = 0;
            }

            ACameraHelper.FlipVertically = options.FlipVertically;
            ACameraHelper.FlipHorizontally = options.FlipHorizontally;

            ACameraHelper.SetDefaultFrameFormat(UVCCameraHelper.FrameFormatMjpeg);
            var listener = new OnMyDevConnectListener(cameraView);
            ACameraHelper.InitUSBMonitor(MainActivity.Instance, render.UvcCamera, listener);
            ACameraHelper.RegisterUSB();
        }

        public static void StartPreview(UvcCameraView view)
        {
            var render = view.GetRenderer() as UvcCameraRender;
            if (render == null) return;

            ACameraHelper.StartPreview(render.UvcCamera);
        }

        public static void StopPreview()
        {
            ACameraHelper.StopPreview();
        }

        public static void StartRecording(string videoPath)
        {
            var param = new RecordParams();
            param.RecordPath = videoPath;
            param.RecordDuration = 0;
            param.VoiceClose = true;
            ACameraHelper.StartPusher(param, null);
        }

        public static void StopRecording()
        {
            ACameraHelper.StopPusher();
        }

        public static void CapturePicture(string imageFile, Action<string> onResult)
        {
            ACameraHelper.CapturePicture(imageFile, new OnCaptureListener(onResult));
        }

        public static void RequestPermission(int deviceId)
        {
            ACameraHelper.RequestPermission(deviceId);
        }

        public static bool IsValidCameraDevice(UsbDevice device)
        {
            return device.DeviceClass == UsbClass.Misc &&
                   device.DeviceSubclass == UsbClass.Comm;
        }

        public static UsbDeviceInfo FromAUsbDevice(UsbDevice device)
        {
            var info = new UsbDeviceInfo
            {
                DeviceId = device.DeviceId,
                DeviceName = device.DeviceName,
                ProductId = device.ProductId,
                VendorId = device.VendorId,
                ManufacturerName = device.ManufacturerName,
                ProductName = device.ProductName
            };
            //try
            //{
            //    info.SerialNumber = device.SerialNumber;
            //}
            //catch
            //{
            //}

            return info;
        }
    }

    class OnCaptureListener : Java.Lang.Object, AbstractUVCCameraHandler.IOnCaptureListener
    {
        public OnCaptureListener(Action<string> onResult)
        {
            OnResult = onResult;
        }

        public Action<string> OnResult { get; }

        public void OnCaptureResult(string path)
        {
            OnResult?.Invoke(path);
        }
    }

    public class UvcCameraSetupOptions
    {
        public int PreviewWidth { get; set; }

        public int PreviewHeight { get; set; }

        public int VideoRotation { get; set; }

        public bool FlipHorizontally { get; set; }

        public bool FlipVertically { get; set; }
    }

    class OnMyDevConnectListener : Java.Lang.Object, UVCCameraHelper.IOnMyDevConnectListener
    {
        private readonly UvcCameraView _cameraView;
        public OnMyDevConnectListener(UvcCameraView cameraView)
        {
            _cameraView = cameraView;
        }

        public void OnAttachDev(UsbDevice device)
        {
            if (!MyUvcCameraHelper.IsValidCameraDevice(device)) return;
            MyUvcCameraHelper.RequestPermission(device.DeviceId);
        }

        public void OnConnectDev(UsbDevice device, bool isConnected)
        {
            if (!MyUvcCameraHelper.IsValidCameraDevice(device)) return;

            if (!isConnected) return;

            if (!MyUvcCameraHelper.ACameraHelper.WaitCameraCreated(3000))
                return;

            MyUvcCameraHelper.StartPreview(_cameraView);
        }

        public void OnDetachDev(UsbDevice device)
        {
            _cameraView.OnCameraDisconnected();
        }

        public void OnDisConnectDev(UsbDevice device)
        {
            Console.WriteLine("OnDisConnectDev");
        }
    }
}