using ActViz.Dialogs;
using ActViz.Models;
using ActViz.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ActViz
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EditView : Page
    {
        private Dataset _curDataset;
        private ObservableCollection<Sensor> dispSensorList;
        private ObservableCollection<SensorType> sensorTypeList;
        private List<Tuple<Sensor, Rectangle, TextBlock, TranslateTransform>> canvasSensorList;
        private TextBox sensorDetail;

        private static readonly CoreCursor sizeCursor = new CoreCursor(CoreCursorType.SizeWestEast, 1);
        private static readonly CoreCursor arrowCursor = new CoreCursor(CoreCursorType.Arrow, 1);

        public EditView()
        {
            this.InitializeComponent();
            dispSensorList = new ObservableCollection<Sensor>();
            sensorTypeList = SensorType.GetSensorTypes();
            canvasSensorList = new List<Tuple<Sensor, Rectangle, TextBlock, TranslateTransform>>();
            sensorDetail = new TextBox();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Get cur dataset from Navigation Parameter
            _curDataset = (e.Parameter as HomeViewModel).dataset;
            // Update Dataset Title
            txtDatasetTitle.Text = _curDataset.Name;
            base.OnNavigatedTo(e);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            StorageFile floorPlanFile = await _curDataset.Folder.GetFileAsync(_curDataset.pathToFloorPlan);
            using (Windows.Storage.Streams.IRandomAccessStream fileStream =
                    await floorPlanFile.OpenAsync(FileAccessMode.Read))
            {
                // Set the image source to the selected bitmap.
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.SetSource(fileStream);
                floorplanImage.Source = bitmapImage;
            }
            sensorListTypeSelect.SelectedIndex = 0;
            PopulateSensors();
            DrawSensors();
            sensorDetail.IsReadOnly = true;
            sensorDetail.Text = "";
            sensorDetail.TextWrapping = TextWrapping.Wrap;
            sensorDetail.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            sensorDetail.Opacity = 0.998;
            sensorDetail.Width = 200;
            sensorDetail.Visibility = Visibility.Collapsed;
            Canvas.SetZIndex(sensorDetail, 101);
            sensorCanvas.Children.Add(sensorDetail);
            VerticalSplitter.ManipulationMode = ManipulationModes.All;
            VerticalSplitter.ManipulationDelta += VerticalSplitter_ManipulationDelta;
        }

        public void TemporaryUpdateSensorDisplay(int tupleIndex, double width, double height)
        {
            Rectangle sensorRect = null;
            TextBlock sensorText = null;
            if (tupleIndex < canvasSensorList.Count && tupleIndex >= 0)
            {
                sensorRect = canvasSensorList[tupleIndex].Item2;
                sensorText = canvasSensorList[tupleIndex].Item3;
                sensorRect.Width = width;
                sensorRect.Height = height;
                sensorText.Width = width;
                sensorText.Height = height;
                if (sensorText.Height != 0 && sensorText.Width != 0)
                {
                    if (sensorText.ActualHeight / sensorText.Height > sensorText.ActualWidth / sensorText.Width)
                    {
                        double fontSizeMultiplier = Math.Sqrt(sensorText.Height / sensorText.ActualHeight);
                        try
                        {
                            sensorText.FontSize = Math.Floor(sensorText.FontSize * fontSizeMultiplier);
                        }
                        catch { }
                    }
                    else
                    {
                        double fontSizeMultiplier = Math.Sqrt(sensorText.Width / sensorText.ActualWidth);
                        try
                        {
                            sensorText.FontSize = Math.Floor(sensorText.FontSize * fontSizeMultiplier);
                        }
                        catch { }

                    }
                }
                sensorCanvas.InvalidateArrange();
                sensorCanvas.UpdateLayout();
            }
        }

        public async void SaveHome()
        {
            await _curDataset.SaveToFolder();
        }


        private void PopulateSensors()
        {
            foreach (Sensor sensor in _curDataset.GetSensors())
            {
                AddSeneorToCanvas(sensor);
            }
        }

        private void AddSeneorToCanvas(Sensor sensor)
        {
            double totalX = floorplanImage.ActualWidth;
            double totalY = floorplanImage.ActualHeight;
            TranslateTransform dragTransform = new TranslateTransform();
            // Draw Rectangle
            Rectangle sensorRect = new Rectangle();
            sensorRect.Width = sensor.sizeX * totalX;
            sensorRect.Height = sensor.sizeY * totalY;
            sensorRect.Fill = new SolidColorBrush(SensorType.GetColorFromSensorType(sensor.type));
            sensorRect.Stroke = new SolidColorBrush(Colors.Black);
            sensorRect.StrokeThickness = 1;
            sensorRect.StrokeDashCap = PenLineCap.Round;
            double startPosX = (sensorCanvas.ActualWidth - totalX) / 2 + sensor.locX * totalX;
            double startPosY = (sensorCanvas.ActualHeight - totalY) / 2 + sensor.locY * totalY;
            Canvas.SetTop(sensorRect, startPosY);
            Canvas.SetLeft(sensorRect, startPosX);
            Canvas.SetZIndex(sensorRect, 0);
            sensorRect.ManipulationMode = ManipulationModes.All;
            sensorRect.ManipulationDelta += Sensor_ManipulationDelta;
            sensorRect.ManipulationCompleted += Sensor_ManipulationCompleted;
            sensorRect.RenderTransform = dragTransform;
            sensorRect.RightTapped += Sensor_RightTapped;
            sensorRect.PointerEntered += Sensor_PointerEnter;
            sensorRect.PointerExited += Sensor_PointerExited;
            // Draw Text
            TextBlock sensorText = new TextBlock();
            sensorText.Text = sensor.name;
            sensorText.Height = sensorRect.Height;
            sensorText.Width = sensorRect.Width;
            Canvas.SetTop(sensorText, startPosY);
            Canvas.SetLeft(sensorText, startPosX);
            Canvas.SetZIndex(sensorText, 0);
            sensorText.ManipulationMode = ManipulationModes.All;
            sensorText.ManipulationDelta += Sensor_ManipulationDelta;
            sensorText.RenderTransform = dragTransform;
            sensorText.ManipulationCompleted += Sensor_ManipulationCompleted;
            sensorText.RightTapped += Sensor_RightTapped;
            sensorText.PointerEntered += Sensor_PointerEnter;
            sensorText.PointerExited += Sensor_PointerExited;
            // Populate Sensor List
            canvasSensorList.Add(new Tuple<Sensor, Rectangle, TextBlock, TranslateTransform>(sensor, sensorRect, sensorText, dragTransform));
            sensorCanvas.Children.Add(sensorRect);
            sensorCanvas.Children.Add(sensorText);
        }

        private void Sensor_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            txtCurSensorDescription.Text = "";
            txtCurSensorName.Text = "--";
            txtCurSensorType.Text = "--";
            sensorDetail.Visibility = Visibility.Collapsed;
            sensorCanvas.InvalidateArrange();
            sensorCanvas.UpdateLayout();
        }

        private void Sensor_PointerEnter(object sender, PointerRoutedEventArgs e)
        {
            Rectangle sensorRect = null;
            Sensor sensor = null;
            TextBlock sensorText = null;
            TranslateTransform dragTransform = null;
            int i = 0;
            if (sender is Rectangle)
            {
                sensorRect = (Rectangle)sender;
                for (i = 0; i < canvasSensorList.Count; i++)
                {
                    if (canvasSensorList[i].Item2 == sensorRect)
                    {
                        sensor = canvasSensorList[i].Item1;
                        sensorText = canvasSensorList[i].Item3;
                        dragTransform = canvasSensorList[i].Item4;
                        break;
                    }
                }

            }
            else if (sender is TextBlock)
            {
                sensorText = (TextBlock)sender;
                for (i = 0; i < canvasSensorList.Count; i++)
                {
                    if (canvasSensorList[i].Item3 == sensorText)
                    {
                        sensor = canvasSensorList[i].Item1;
                        sensorRect = canvasSensorList[i].Item2;
                        dragTransform = canvasSensorList[i].Item4;
                        break;
                    }
                }
            }
            txtCurSensorDescription.Text = sensor.description;
            txtCurSensorName.Text = sensor.name;
            txtCurSensorType.Text = sensor.type;
            sensorDetail.Text = "Name: " + sensor.name + "\n";
            sensorDetail.Text += "Type: " + sensor.type + "\n";
            sensorDetail.Text += "Description: \n" + sensor.description;
            sensorDetail.Visibility = Visibility.Visible;
            double sensorDetail_x = Canvas.GetLeft(sensorRect) + sensorRect.ActualWidth;
            double sensorDetail_y = Canvas.GetTop(sensorRect) + sensorRect.ActualHeight;
            if (sensorDetail_x + sensorDetail.ActualWidth > sensorCanvas.ActualWidth)
            {
                sensorDetail_x = Canvas.GetLeft(sensorRect) - sensorDetail.ActualWidth;
            }
            if (sensorDetail_y + sensorDetail.ActualHeight > sensorCanvas.ActualHeight)
            {
                sensorDetail_y = Canvas.GetTop(sensorRect) - sensorDetail.ActualHeight;
            }
            Canvas.SetLeft(sensorDetail, sensorDetail_x);
            Canvas.SetTop(sensorDetail, sensorDetail_y);
            sensorCanvas.InvalidateArrange();
            sensorCanvas.UpdateLayout();
        }

        private void Sensor_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            Rectangle sensorRect = null;
            Sensor sensor = null;
            TextBlock sensorText = null;
            TranslateTransform dragTransform = null;
            int i = 0;
            if (sender is Rectangle)
            {
                sensorRect = (Rectangle)sender;
                for (i = 0; i < canvasSensorList.Count; i++)
                {
                    if (canvasSensorList[i].Item2 == sensorRect)
                    {
                        sensor = canvasSensorList[i].Item1;
                        sensorText = canvasSensorList[i].Item3;
                        dragTransform = canvasSensorList[i].Item4;
                        break;
                    }
                }

            }
            else if (sender is TextBlock)
            {
                sensorText = (TextBlock)sender;
                for (i = 0; i < canvasSensorList.Count; i++)
                {
                    if (canvasSensorList[i].Item3 == sensorText)
                    {
                        sensor = canvasSensorList[i].Item1;
                        sensorRect = canvasSensorList[i].Item2;
                        dragTransform = canvasSensorList[i].Item4;
                        break;
                    }
                }
            }
            MenuFlyout sensorConfigMenu = new MenuFlyout();
            MenuFlyoutItem sensorConfigMenu_Config = new MenuFlyoutItem();
            sensorConfigMenu_Config.Text = "Config";
            sensorConfigMenu_Config.Click += SensorConfigMenu_Config_Click;
            sensorConfigMenu_Config.Tag = canvasSensorList[i];
            MenuFlyoutItem sensorConfigMenu_Delete = new MenuFlyoutItem();
            sensorConfigMenu_Delete.Text = "Delete";
            sensorConfigMenu_Delete.Click += SensorConfigMenu_Delete_Click;
            sensorConfigMenu_Delete.Tag = canvasSensorList[i];
            sensorConfigMenu.Items.Add(sensorConfigMenu_Config);
            sensorConfigMenu.Items.Add(sensorConfigMenu_Delete);
            sensorConfigMenu.ShowAt(sensorRect, new Point(sensorRect.ActualWidth, sensorRect.ActualHeight));
        }

        private async void SensorConfigMenu_Delete_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem configMenuItem = (MenuFlyoutItem)sender;
            Tuple<Sensor, Rectangle, TextBlock, TranslateTransform> sensorTuple =
                (Tuple<Sensor, Rectangle, TextBlock, TranslateTransform>)configMenuItem.Tag;
            Sensor sensor = sensorTuple.Item1;
            string message = "Are you sure that you want to perminantly remove sensor " + sensor.name + "?";
            var dialog = new Windows.UI.Popups.MessageDialog(message, "Remove Sensor");
            dialog.Commands.Add(new Windows.UI.Popups.UICommand("Yes") { Id = 0 });
            dialog.Commands.Add(new Windows.UI.Popups.UICommand("Cancel") { Id = 1 });
            var result = await dialog.ShowAsync();
            if (result.Label == "Yes")
            {
                // Remove Sensor
                sensorCanvas.Children.Remove(sensorTuple.Item2);
                sensorCanvas.Children.Remove(sensorTuple.Item3);
                _curDataset.RemoveSensor(sensor);
                dispSensorList.Remove(sensor);
                canvasSensorList.Remove(sensorTuple);
                SaveHome();
                sensorCanvas.InvalidateArrange();
                sensorCanvas.UpdateLayout();
            }
        }

        private async void SensorConfigMenu_Config_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem configMenuItem = (MenuFlyoutItem)sender;
            Tuple<Sensor, Rectangle, TextBlock, TranslateTransform> sensorTuple =
                (Tuple<Sensor, Rectangle, TextBlock, TranslateTransform>)configMenuItem.Tag;
            Sensor sensor = sensorTuple.Item1;
            Rectangle sensorRect = sensorTuple.Item2;
            TextBlock sensorText = sensorTuple.Item3;
            ConfigSensorDialog dialog = new ConfigSensorDialog(this, canvasSensorList.IndexOf(sensorTuple),
                sensor.name, sensor.type, sensorRect.ActualWidth, sensorRect.ActualHeight,
                0.3 * floorplanImage.ActualWidth, 0.3 * floorplanImage.ActualHeight, sensor.description);
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                double totalX = floorplanImage.ActualWidth;
                double totalY = floorplanImage.ActualHeight;
                sensor.name = dialog.sensorName;
                sensor.sizeX = dialog.sensorWidth / totalX;
                sensor.sizeY = dialog.sensorHeight / totalY;
                sensor.description = dialog.sensorDescription;
                sensorText.Text = sensor.name;
                SaveHome();
            }
            sensorListTypeSelect.SelectedIndex = SensorType.GetSensorTypeIndex(sensor.type);
            TemporaryUpdateSensorDisplay(dialog.sensorTupleIndex, dialog.sensorWidth, dialog.sensorHeight);
        }

        private void Sensor_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            Rectangle sensorRect = null;
            Sensor sensor = null;
            TextBlock sensorText = null;
            TranslateTransform dragTransform = null;
            if (sender is Rectangle)
            {
                sensorRect = (Rectangle)sender;
                for (int i = 0; i < canvasSensorList.Count; i++)
                {
                    if (canvasSensorList[i].Item2 == sensorRect)
                    {
                        sensor = canvasSensorList[i].Item1;
                        sensorText = canvasSensorList[i].Item3;
                        dragTransform = canvasSensorList[i].Item4;
                        break;
                    }
                }

            }
            else if (sender is TextBlock)
            {
                sensorText = (TextBlock)sender;
                for (int i = 0; i < canvasSensorList.Count; i++)
                {
                    if (canvasSensorList[i].Item3 == sensorText)
                    {
                        sensor = canvasSensorList[i].Item1;
                        sensorRect = canvasSensorList[i].Item2;
                        dragTransform = canvasSensorList[i].Item4;
                        break;
                    }
                }
            }
            if (dragTransform != null)
            {
                double totalX = floorplanImage.ActualWidth;
                double totalY = floorplanImage.ActualHeight;
                double startPosX = Canvas.GetLeft(sensorRect);
                double startPosY = Canvas.GetTop(sensorRect);
                startPosX += dragTransform.X;
                startPosY += dragTransform.Y;
                sensor.locX = (startPosX - (sensorCanvas.ActualWidth - totalX) / 2) / totalX;
                sensor.locY = (startPosY - (sensorCanvas.ActualHeight - totalY) / 2) / totalY;
                Canvas.SetLeft(sensorRect, startPosX);
                Canvas.SetTop(sensorRect, startPosY);
                Canvas.SetLeft(sensorText, startPosX);
                Canvas.SetTop(sensorText, startPosY);
                dragTransform.X = 0;
                dragTransform.Y = 0;
                sensorCanvas.InvalidateArrange();
                sensorCanvas.UpdateLayout();
                SaveHome();
                //string message = "dragTransform: " + dragTransform.X.ToString() + ", " + dragTransform.Y.ToString() + "\n";
                //message += "Rectangle: " + .ToString() + ", " + Canvas.GetTop(sensorRect).ToString() + "\n";
                //var dialog = new Windows.UI.Popups.MessageDialog(message);
                //dialog.Commands.Add(new Windows.UI.Popups.UICommand("Yes") { Id = 0 });
                //var result = dialog.ShowAsync();
            }
        }

        private void Sensor_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            Rectangle sensorRect = null;
            Sensor sensor = null;
            TextBlock sensorText = null;
            TranslateTransform dragTransform = null;
            if (sender is Rectangle)
            {
                sensorRect = (Rectangle)sender;
                for (int i = 0; i < canvasSensorList.Count; i++)
                {
                    if (canvasSensorList[i].Item2 == sensorRect)
                    {
                        sensor = canvasSensorList[i].Item1;
                        sensorText = canvasSensorList[i].Item3;
                        dragTransform = canvasSensorList[i].Item4;
                        break;
                    }
                }

            }
            else if (sender is TextBlock)
            {
                sensorText = (TextBlock)sender;
                for (int i = 0; i < canvasSensorList.Count; i++)
                {
                    if (canvasSensorList[i].Item3 == sensorText)
                    {
                        sensor = canvasSensorList[i].Item1;
                        sensorRect = canvasSensorList[i].Item2;
                        dragTransform = canvasSensorList[i].Item4;
                        break;
                    }
                }
            }
            if (dragTransform != null)
            {
                dragTransform.X += e.Delta.Translation.X;
                dragTransform.Y += e.Delta.Translation.Y;
            }
        }

        private void DrawSensors()
        {
            double totalX = floorplanImage.ActualWidth;
            double totalY = floorplanImage.ActualHeight;
            foreach (Tuple<Sensor, Rectangle, TextBlock, TranslateTransform> sensorTuple in canvasSensorList)
            {
                Sensor sensor = sensorTuple.Item1;
                Rectangle sensorRect = sensorTuple.Item2;
                TextBlock sensorText = sensorTuple.Item3;
                sensorRect.Width = sensor.sizeX * totalX;
                sensorRect.Height = sensor.sizeY * totalY;
                sensorText.Height = sensorRect.Height;
                sensorText.Width = sensorRect.Width;
                // Set Multipliers
                if (sensorText.Height != 0 && sensorText.Width != 0)
                {
                    if (sensorText.ActualHeight / sensorText.Height > sensorText.ActualWidth / sensorText.Width)
                    {
                        double fontSizeMultiplier = Math.Sqrt(sensorText.Height / sensorText.ActualHeight);
                        sensorText.FontSize = Math.Floor(sensorText.FontSize * fontSizeMultiplier);
                    }
                    else
                    {
                        double fontSizeMultiplier = Math.Sqrt(sensorText.Width / sensorText.ActualWidth);
                        sensorText.FontSize = Math.Floor(sensorText.FontSize * fontSizeMultiplier);
                    }
                }
                double startPosX = (sensorCanvas.ActualWidth - totalX) / 2 + sensor.locX * totalX;
                double startPosY = (sensorCanvas.ActualHeight - totalY) / 2 + sensor.locY * totalY;
                Canvas.SetTop(sensorRect, startPosY);
                Canvas.SetLeft(sensorRect, startPosX);
                Canvas.SetTop(sensorText, startPosY);
                Canvas.SetLeft(sensorText, startPosX);
            }
            sensorCanvas.InvalidateArrange();
            sensorCanvas.UpdateLayout();
        }

        private async void Button_NewSensorClick(object sender, RoutedEventArgs e)
        {
            AddSensorDialog dialog = new AddSensorDialog();
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                if (_curDataset != null)
                {
                    double totalX = floorplanImage.ActualWidth;
                    double totalY = floorplanImage.ActualHeight;
                    Sensor sensor = _curDataset.AddSensor(dialog.sensorName, dialog.sensorType, 0.0, 0.0, 23 / totalX, 15 / totalY);
                    int typeIndex = GetSensorTypeIndex(dialog.sensorType);
                    SaveHome();
                    AddSeneorToCanvas(sensor);
                    if (typeIndex >= 0)
                    {
                        sensorListTypeSelect.SelectedIndex = -1;
                        sensorListTypeSelect.SelectedIndex = typeIndex;
                    }
                }
            }
        }

        private void sensorListTypeSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dispSensorList.Clear();
            if (sensorListTypeSelect.SelectedIndex >= 0 && sensorListTypeSelect.SelectedIndex < sensorTypeList.Count)
            {
                List<Sensor> tmpSensorList = new List<Sensor>();
                foreach (Sensor sensor in _curDataset.GetSensors())
                {
                    if (sensor.type == sensorTypeList[sensorListTypeSelect.SelectedIndex].type)
                    {
                        tmpSensorList.Add(sensor);
                    }
                }
                tmpSensorList.Sort(Sensor.CompareByName);
                foreach (Sensor sensor in tmpSensorList)
                {
                    dispSensorList.Add(sensor);
                }
            }
        }

        private void sensorList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sensorList.SelectedIndex >= 0 && sensorList.SelectedIndex < dispSensorList.Count)
            {
                Sensor sensor = dispSensorList[sensorList.SelectedIndex];
                for (int i = 0; i < canvasSensorList.Count; i++)
                {
                    if (canvasSensorList[i].Item1.name == sensor.name)
                    {
                        Canvas.SetZIndex(canvasSensorList[i].Item2, 100);
                        Canvas.SetZIndex(canvasSensorList[i].Item3, 100);
                    }
                    else
                    {
                        Canvas.SetZIndex(canvasSensorList[i].Item2, 0);
                        Canvas.SetZIndex(canvasSensorList[i].Item3, 0);
                    }
                }
            }
            else
            {
                for (int i = 0; i < canvasSensorList.Count; i++)
                {
                    Canvas.SetZIndex(canvasSensorList[i].Item2, 0);
                    Canvas.SetZIndex(canvasSensorList[i].Item3, 0);
                }
            }
        }


        private int GetSensorTypeIndex(string type)
        {
            int i;
            for (i = 0; i < sensorTypeList.Count; i++)
            {
                if (sensorTypeList[i].type == type)
                {
                    return i;
                }
            }
            return -1;
        }

        private void floorplanImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawSensors();
        }

        private void sensorCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawSensors();
        }

        #region VerticalSplitter
        private void VerticalSplitter_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            try
            {
                infoPanel.Width = infoPanel.ActualWidth - e.Delta.Translation.X;
            }
            catch
            {

            }
        }

        private void VerticalSplitter_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = sizeCursor;
        }

        private void VerticalSplitter_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = arrowCursor;
        }
        #endregion
    }
}
