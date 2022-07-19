using System;
using System.ComponentModel;
using Android.Content;
using Android.Widget;
using Com.Serenegiant.Usb.Widget;
using UsbCameraSample;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using ARelativeLayout = Android.Widget.RelativeLayout;

[assembly: ExportRenderer(typeof(UvcCameraView),
                          typeof(UsbCameraSample.Droid.Renders.UvcCameraRender))]
namespace UsbCameraSample.Droid.Renders
{
    public class UvcCameraRender : ViewRenderer<UvcCameraView, ARelativeLayout>
    {
        internal UVCCameraTextureView UvcCamera;

        public UvcCameraRender(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<UvcCameraView> args)
        {
            base.OnElementChanged(args);

            if (args.NewElement != null)
            {
                if (Control == null)
                {
                    // Save the VideoView for future reference
                    UvcCamera = new UVCCameraTextureView(Context);

                    // Put the VideoView in a RelativeLayout
                    ARelativeLayout relativeLayout = new ARelativeLayout(Context);
                    relativeLayout.AddView(UvcCamera);

                    // Center the VideoView in the RelativeLayout
                    ARelativeLayout.LayoutParams layoutParams =
                        new ARelativeLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
                    layoutParams.AddRule(LayoutRules.CenterInParent);
                    UvcCamera.LayoutParameters = layoutParams;

                    SetNativeControl(relativeLayout);
                }

                args.NewElement.OpenRequested += NewElement_OpenRequested;
                args.NewElement.CloseRequested += NewElement_CloseRequested;
                args.NewElement.StartRecordingRequested += NewElement_StartRecordingRequested;
                args.NewElement.StopRecordingRequested += NewElement_StopRecordingRequested;
                args.NewElement.TakeSnapshotRequested += NewElement_TakeSnapshotRequested;

            }

            if (args.OldElement != null)
            {
                args.OldElement.OpenRequested -= NewElement_OpenRequested;
                args.OldElement.CloseRequested -= NewElement_CloseRequested;
                args.OldElement.StartRecordingRequested -= NewElement_StartRecordingRequested;
                args.OldElement.StopRecordingRequested -= NewElement_StopRecordingRequested;
                args.OldElement.TakeSnapshotRequested -= NewElement_TakeSnapshotRequested;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (Control != null && UvcCamera != null)
            {
            }
            if (Element != null)
            {
            }

            base.Dispose(disposing);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(sender, args);

            if (args.PropertyName == UvcCameraView.VideoRotationProperty.PropertyName)
            {
            }
            else if (args.PropertyName == UvcCameraView.FlipVerticallyProperty.PropertyName)
            {
            }
            else if (args.PropertyName == UvcCameraView.FlipHorizontallyProperty.PropertyName)
            {
            }
        }

        private void NewElement_OpenRequested(object sender, RequestOpenArgs e)
        {
            var cameraView = (UvcCameraView)sender;
            MyUvcCameraHelper.Setup(cameraView, new UvcCameraSetupOptions
            {
                PreviewHeight = e.PreviewHeight,
                PreviewWidth = e.PreviewWidth,
                VideoRotation = cameraView.VideoRotation,
                FlipVertically = cameraView.FlipVertically,
                FlipHorizontally = cameraView.FlipHorizontally
            });
            MyUvcCameraHelper.StartPreview(cameraView);
        }

        private void NewElement_CloseRequested(object sender, System.EventArgs e)
        {
            MyUvcCameraHelper.StopPreview();
        }

        private void NewElement_StartRecordingRequested(object sender, RequestStartRecordingArgs e)
        {
            MyUvcCameraHelper.StartRecording(e.VideoPath);
        }

        private void NewElement_StopRecordingRequested(object sender, System.EventArgs e)
        {
            MyUvcCameraHelper.StopRecording();
        }

        private void NewElement_TakeSnapshotRequested(object sender, RequestTakeSnapshotArgs e)
        {
            MyUvcCameraHelper.CapturePicture(e.ImagePath, (file) =>
            {
                Console.WriteLine("TakeSnapshot to:" + file);
            });
        }
    }
}