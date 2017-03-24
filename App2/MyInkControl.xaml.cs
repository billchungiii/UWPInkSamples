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
using Windows.UI.Input.Inking.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace App2
{
    public sealed partial class MyInkControl : UserControl
    {
        /// <summary>
        /// 外部靠這個 Event 取得結果
        /// </summary>
        public event EventHandler<string> Recognized;

        private void OnRecognized(string result)
        {
            if (Recognized != null)
            {
                Recognized(this, result);
            }
        }



        public MyInkControl()
        {
            this.InitializeComponent();

            if (SetVisibility())
            {
                Candidates = new System.Collections.ObjectModel.ObservableCollection<string>();
                listview.ItemsSource = Candidates;
                InitialRecognizersCombobox();
                InitialInkRecognize();
                //  CoreInkIndependentInputSource core = CoreInkIndependentInputSource.Create(inkCanvas.InkPresenter);


            }

        }










        /// <summary>
        /// 設定自己的顯示狀態, 如果是 Desktop 則顯示並回傳 true
        /// </summary>
        /// <returns></returns>
        private bool SetVisibility()
        {
            var qualifiers = Windows.ApplicationModel.Resources.Core
                    .ResourceContext.GetForCurrentView().QualifierValues;
            if (qualifiers.ContainsKey("DeviceFamily"))
            {
                if (qualifiers["DeviceFamily"] == "Desktop")
                {
                    this.Visibility = Visibility.Visible;
                    return true;
                }
                else
                {

                    this.Visibility = Visibility.Collapsed;
                    return false;
                }
            }
            else
            {
                this.Visibility = Visibility.Collapsed;
                return false;
            }

        }

        private void InitialRecognizersCombobox()
        {
            recognizersCombobox.ItemsSource = InkRecognizerHelper.RecognizerNameList;
            string defaultName = InkRecognizerHelper.LanguageTagToRecognizerName(CultureInfo.CurrentCulture.Name);
            if (InkRecognizerHelper.RecognizerNameList.FirstOrDefault((x) => x == defaultName) != null)
            {
                recognizersCombobox.SelectedIndex = InkRecognizerHelper.RecognizerNameList.IndexOf(defaultName);
            }
        }

        private void InitialInkRecognize()
        {

            var drawAttirbutes = new InkDrawingAttributes() { Color = Colors.Black, };
            inkCanvas.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Mouse | CoreInputDeviceTypes.Pen | CoreInputDeviceTypes.Touch;
            inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(drawAttirbutes);
        }

        async private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var container = new InkRecognizerContainer();
                var recognizers = container.GetRecognizers();
                if (recognizers != null && recognizers.Count > 0)
                {
                    // 以下三行為取得Combobox選擇語系, 並設定語系的辨識引擎
                    string recognizerName = recognizersCombobox.SelectedItem.ToString();
                    var recognizer = recognizers.FirstOrDefault((x) => x.Name == recognizerName);
                    if (recognizer != null)
                    {
                        container.SetDefaultRecognizer(recognizer);
                    }

                }
                if (inkCanvas.InkPresenter.StrokeContainer != null)
                {
                    
                    var result = await container.RecognizeAsync(inkCanvas.InkPresenter.StrokeContainer, InkRecognitionTarget.All);
                    string returnText = string.Empty;
                    inkCanvas.InkPresenter.StrokeContainer.Clear();
                    Candidates.Clear();
                    foreach (var r in result)
                    {
                        foreach (var c in r.GetTextCandidates())
                        {
                            Candidates.Add(c);
                        }
                    }
                    listview.Visibility = Visibility.Visible;
                    button.Visibility = Visibility.Collapsed;
                }

            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            //if (result != null)
            //{
            //    returnText = result[0].GetTextCandidates()[0];
            //}

            //OnRecognized(returnText);

        }

        private System.Collections.ObjectModel.ObservableCollection<string> Candidates { get; set; }

        private void listview_ItemClick(object sender, ItemClickEventArgs e)
        {
            OnRecognized((string)e.ClickedItem);
            listview.Visibility = Visibility.Collapsed;
            button.Visibility = Visibility.Visible;
        }
    }
    public class InkRecognizerHelper
    {

        private static List<string> _recognizerNameList;
        public static List<string> RecognizerNameList
        {
            get
            {
                if (_recognizerNameList == null)
                {
                    CreateRecognizerNameList();
                }
                return _recognizerNameList;
            }
        }

        private static void CreateRecognizerNameList()
        {
            _recognizerNameList = new List<string>();
            var container = new InkRecognizerContainer();
            var recognizers = container.GetRecognizers();
            if (recognizers != null && recognizers.Count > 0)
            {
                foreach (var recognizer in recognizers)
                {
                    _recognizerNameList.Add(recognizer.Name);
                }
            }

        }

        private static Dictionary<string, string> Bcp47ToRecognizerNameDictionary = null;

        private static void EnsureDictionary()
        {
            if (Bcp47ToRecognizerNameDictionary == null)
            {
                Bcp47ToRecognizerNameDictionary = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);

                Bcp47ToRecognizerNameDictionary.Add("en-US", "Microsoft English (US) Handwriting Recognizer");
                Bcp47ToRecognizerNameDictionary.Add("en-GB", "Microsoft English (UK) Handwriting Recognizer");
                Bcp47ToRecognizerNameDictionary.Add("en-CA", "Microsoft English (Canada) Handwriting Recognizer");
                Bcp47ToRecognizerNameDictionary.Add("en-AU", "Microsoft English (Australia) Handwriting Recognizer");
                Bcp47ToRecognizerNameDictionary.Add("de-DE", "Microsoft-Handschrifterkennung - Deutsch");
                Bcp47ToRecognizerNameDictionary.Add("de-CH", "Microsoft-Handschrifterkennung - Deutsch (Schweiz)");
                Bcp47ToRecognizerNameDictionary.Add("es-ES", "Reconocimiento de escritura a mano en español de Microsoft");
                Bcp47ToRecognizerNameDictionary.Add("es-MX", "Reconocedor de escritura en Español (México) de Microsoft");
                Bcp47ToRecognizerNameDictionary.Add("es-AR", "Reconocedor de escritura en Español (Argentina) de Microsoft");
                Bcp47ToRecognizerNameDictionary.Add("fr", "Reconnaissance d'écriture Microsoft - Français");
                Bcp47ToRecognizerNameDictionary.Add("fr-FR", "Reconnaissance d'écriture Microsoft - Français");
                Bcp47ToRecognizerNameDictionary.Add("ja", "Microsoft 日本語手書き認識エンジン");
                Bcp47ToRecognizerNameDictionary.Add("ja-JP", "Microsoft 日本語手書き認識エンジン");
                Bcp47ToRecognizerNameDictionary.Add("it", "Riconoscimento grafia italiana Microsoft");
                Bcp47ToRecognizerNameDictionary.Add("nl-NL", "Microsoft Nederlandstalige handschriftherkenning");
                Bcp47ToRecognizerNameDictionary.Add("nl-BE", "Microsoft Nederlandstalige (België) handschriftherkenning");
                Bcp47ToRecognizerNameDictionary.Add("zh", "Microsoft 中文(简体)手写识别器");
                Bcp47ToRecognizerNameDictionary.Add("zh-CN", "Microsoft 中文(简体)手写识别器");
                Bcp47ToRecognizerNameDictionary.Add("zh-Hans-CN", "Microsoft 中文(简体)手写识别器");
                Bcp47ToRecognizerNameDictionary.Add("zh-Hant", "Microsoft 中文(繁體)手寫辨識器");
                Bcp47ToRecognizerNameDictionary.Add("zh-TW", "Microsoft 中文(繁體)手寫辨識器");
                Bcp47ToRecognizerNameDictionary.Add("ru", "Microsoft система распознавания русского рукописного ввода");
                Bcp47ToRecognizerNameDictionary.Add("pt-BR", "Reconhecedor de Manuscrito da Microsoft para Português (Brasil)");
                Bcp47ToRecognizerNameDictionary.Add("pt-PT", "Reconhecedor de escrita manual da Microsoft para português");
                Bcp47ToRecognizerNameDictionary.Add("ko", "Microsoft 한글 필기 인식기");
                Bcp47ToRecognizerNameDictionary.Add("pl", "System rozpoznawania polskiego pisma odręcznego firmy Microsoft");
                Bcp47ToRecognizerNameDictionary.Add("sv", "Microsoft Handskriftstolk för svenska");
                Bcp47ToRecognizerNameDictionary.Add("cs", "Microsoft rozpoznávač rukopisu pro český jazyk");
                Bcp47ToRecognizerNameDictionary.Add("da", "Microsoft Genkendelse af dansk håndskrift");
                Bcp47ToRecognizerNameDictionary.Add("nb", "Microsoft Håndskriftsgjenkjenner for norsk");
                Bcp47ToRecognizerNameDictionary.Add("nn", "Microsoft Håndskriftsgjenkjenner for nynorsk");
                Bcp47ToRecognizerNameDictionary.Add("fi", "Microsoftin suomenkielinen käsinkirjoituksen tunnistus");
                Bcp47ToRecognizerNameDictionary.Add("ro", "Microsoft recunoaştere grafie - Română");
                Bcp47ToRecognizerNameDictionary.Add("hr", "Microsoftov hrvatski rukopisni prepoznavač");
                Bcp47ToRecognizerNameDictionary.Add("sr-Latn", "Microsoft prepoznavač rukopisa za srpski (latinica)");
                Bcp47ToRecognizerNameDictionary.Add("sr", "Microsoft препознавач рукописа за српски (ћирилица)");
                Bcp47ToRecognizerNameDictionary.Add("ca", "Reconeixedor d'escriptura manual en català de Microsoft");
            }
        }

        public static string LanguageTagToRecognizerName(string bcp47tag)
        {
            EnsureDictionary();

            string recognizerName = string.Empty;
            try
            {
                recognizerName = Bcp47ToRecognizerNameDictionary[bcp47tag];
            }
            catch (KeyNotFoundException)
            {
                recognizerName = string.Empty;
            }

            return recognizerName;
        }
    }
}
