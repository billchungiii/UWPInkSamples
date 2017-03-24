using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//空白頁項目範本收錄在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace App1
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            InitialInkRecognize();
        }

        private void InitialInkRecognize()
        {

            var drawAttirbutes = new InkDrawingAttributes() { Color = Colors.Black, };
            inkCanvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Mouse | CoreInputDeviceTypes.Pen | CoreInputDeviceTypes.Touch;
            inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(drawAttirbutes);
        }

        async private void Button_Click(object sender, RoutedEventArgs e)
        {
            var container = new InkRecognizerContainer();
            var recognizers = container.GetRecognizers();
            if (recognizers != null && recognizers.Count > 0)
            {
                // 以下三行為取得目前的語系, 並設定使用目前語系的辨識引擎
                string recognizerName = InkRecognizerHelper.LanguageTagToRecognizerName(CultureInfo.CurrentCulture.Name);
                var recognizer = recognizers.FirstOrDefault((x) => x.Name == recognizerName);
                if (recognizer != null)
                {
                    container.SetDefaultRecognizer(recognizer);
                }

            }

            var result = await container.RecognizeAsync(inkCanvas.InkPresenter.StrokeContainer, InkRecognitionTarget.All);
          


        }

        private void EraseButton_Click(object sender, RoutedEventArgs e)
        {
            inkCanvas.InkPresenter.StrokeContainer.Clear();
        }
    }
}
