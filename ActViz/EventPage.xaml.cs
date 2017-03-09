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
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
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
    public sealed partial class EventPage : Page
    {
        private HomeViewModel _home;
        private Dataset _curDataset;

        // Floorplan and Sensor Display
        private List<Tuple<Sensor, Rectangle, TextBlock>> canvasSensorList;
        private TextBox sensorDetail;

        private static readonly CoreCursor sizeCursor = new CoreCursor(CoreCursorType.SizeWestEast, 1);
        private static readonly CoreCursor arrowCursor = new CoreCursor(CoreCursorType.Arrow, 1);

        MenuFlyout annonateFlyout = new MenuFlyout();

        public EventPage()
        {
            this.InitializeComponent();
            canvasSensorList = new List<Tuple<Sensor, Rectangle, TextBlock>>();
            sensorDetail = new TextBox();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // Get cur dataset from Navigation Parameter
            _home = e.Parameter as HomeViewModel;
            _curDataset = _home.dataset;
            // Update Dataset Title
            txtDatasetTitle.Text = _curDataset.Name;
            // Load Data
            dataListView.IsEnabled = false;
            LoadingProgressRing.IsActive = true;
            await _home.LoadEvents();
            _home.InitSensorFireStatus();
            _home.SelectedEventIndex = -1;
            // Set Start/End of Datetime selection
            curEventDate.MinDate = _home.sensorEventCollection.FirstEventDate;
            curEventDate.MaxDate = _home.sensorEventCollection.LastEventDate;
            int numDays = Convert.ToInt32((curEventDate.MaxDate - curEventDate.MinDate).TotalDays);
            DateSlider.Maximum = numDays + 1;
            DateSlider.Minimum = 1;
            dataListView.IsEnabled = true;
            LoadingProgressRing.IsActive = false;
            curEventDate.Date = _home.sensorEventCollection.FirstEventDate;
            DateSlider.Value = 1;
            populateAnnotateFlyout();
            // Set Leftside Size
            dataListView.Width = dataListView.ActualWidth;
            base.OnNavigatedTo(e);
        }

        private void populateAnnotateFlyout()
        {
            MenuFlyoutSubItem ActivityMenu = new MenuFlyoutSubItem();
            ActivityMenu.Text = "Tag Activity";
            foreach(ActivityViewModel activity in _home.ActivityCollection)
            {
                MenuFlyoutItem curMenuItem = new MenuFlyoutItem();
                curMenuItem.Text = activity.name;
                curMenuItem.Click += TagActivity_Click;
                curMenuItem.Foreground = new SolidColorBrush(activity.color);
                curMenuItem.Tag = activity.name;
                ActivityMenu.Items.Add(curMenuItem);
            }
            annonateFlyout.Items.Add(ActivityMenu);
            MenuFlyoutSubItem ResidentMenu = new MenuFlyoutSubItem();
            ResidentMenu.Text = "Tag Resident";
            foreach(ResidentViewModel resident in _home.ResidentCollection)
            {
                MenuFlyoutItem curMenuItem = new MenuFlyoutItem();
                curMenuItem.Text = resident.name;
                curMenuItem.Click += TagResident_Click;
                curMenuItem.Foreground = new SolidColorBrush(resident.color);
                curMenuItem.Tag = resident.name;
                ResidentMenu.Items.Add(curMenuItem);
            }
            annonateFlyout.Items.Add(ResidentMenu);
        }

        private void TagResident_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem flyoutMenuItem = (MenuFlyoutItem)sender;
            List<SensorEventViewModel> selectedEvents = new List<SensorEventViewModel>();
            foreach (var item in dataListView.SelectedItems)
            {
                selectedEvents.Add(item as SensorEventViewModel);
            }
            _home.sensorEventCollection.TagResidents((string)flyoutMenuItem.Tag, selectedEvents);
        }

        private void TagActivity_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem flyoutMenuItem = (MenuFlyoutItem)sender;
            List<SensorEventViewModel> selectedEvents = new List<SensorEventViewModel>();
            foreach(var item in dataListView.SelectedItems)
            {
                selectedEvents.Add(item as SensorEventViewModel);
            }
            _home.sensorEventCollection.TagActivities((string)flyoutMenuItem.Tag, selectedEvents);
        }

        private void sensorCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawSensors();
        }

        private void floorplanImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawSensors();
        }

        #region SensorCanvas
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

            // VertialSplitter Move Control
            VerticalSplitter.ManipulationMode = ManipulationModes.All;
            VerticalSplitter.ManipulationDelta += VerticalSplitter_ManipulationDelta;
        }

        private void DrawSensors()
        {
            double totalX = floorplanImage.ActualWidth;
            double totalY = floorplanImage.ActualHeight;
            foreach (Tuple<Sensor, Rectangle, TextBlock> sensorTuple in canvasSensorList)
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
            // Draw Rectangle
            Rectangle sensorRect = new Rectangle();
            sensorRect.Width = sensor.sizeX * totalX;
            sensorRect.Height = sensor.sizeY * totalY;
            sensorRect.Stroke = new SolidColorBrush(SensorType.GetColorFromSensorType(sensor.type));
            sensorRect.StrokeThickness = 1;
            sensorRect.StrokeDashCap = PenLineCap.Round;
            double startPosX = (sensorCanvas.ActualWidth - totalX) / 2 + sensor.locX * totalX;
            double startPosY = (sensorCanvas.ActualHeight - totalY) / 2 + sensor.locY * totalY;
            Canvas.SetTop(sensorRect, startPosY);
            Canvas.SetLeft(sensorRect, startPosX);
            Canvas.SetZIndex(sensorRect, 0);
            sensorRect.ManipulationMode = ManipulationModes.All;
            //sensorRect.RightTapped += Sensor_RightTapped;
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
            //sensorText.RightTapped += Sensor_RightTapped;
            sensorText.PointerEntered += Sensor_PointerEnter;
            sensorText.PointerExited += Sensor_PointerExited;
            // Populate Sensor List
            canvasSensorList.Add(new Tuple<Sensor, Rectangle, TextBlock>(sensor, sensorRect, sensorText));
            sensorCanvas.Children.Add(sensorRect);
            sensorCanvas.Children.Add(sensorText);
        }

        private void Sensor_PointerEnter(object sender, PointerRoutedEventArgs e)
        {
            // No need to do anything if no event selected.
            if (_home.SelectedEventIndex < 0) return;
            // Update Layout
            Rectangle sensorRect = null;
            Sensor sensor = null;
            TextBlock sensorText = null;
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
                        break;
                    }
                }
            }
            sensorDetail.Text = "Name: " + sensor.name + "\n";
            sensorDetail.Text += "Type: " + sensor.type + "\n";
            sensorDetail.Text += "Description: \n" + sensor.description;
            int eventIndex = _home.SensorLastFireStat[sensor.name];
            if(eventIndex != -1)
            {
                sensorDetail.Text += (_home.SelectedEventIndex - eventIndex).ToString() + " events ago\n";
                sensorDetail.Text += "at " + _home.sensorEventCollection[eventIndex].TimeTag.ToString("H:mm:ss");
            }
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

        private void Sensor_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            sensorDetail.Visibility = Visibility.Collapsed;
            sensorCanvas.InvalidateArrange();
            sensorCanvas.UpdateLayout();
        }
        #endregion

        private void dataListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int maxTime = 3600 * 2; // 2 hours
            int maxSensor = 6;      // Maximum 6 sensor drawing
            int timeElapse = 0;
            int maxTimeElapse = 0;
            int sensorDrawn = 0;
            List<KeyValuePair<string, int>> sensorLastFiredEventList = _home.GetSensorFireStatusSorted();
            List<KeyValuePair<string, int>> sensorDrawList = new List<KeyValuePair<string, int>>();
            // Clear Drawing
            foreach (Tuple<Sensor, Rectangle, TextBlock> canvasSensorEntry in canvasSensorList)
            {
                canvasSensorEntry.Item2.Fill = new SolidColorBrush(Colors.White);
                canvasSensorEntry.Item2.Opacity = 0.998;
            }
            if(_home.sensorEventCollection.Count > 0)
            {
                // Find the total timeElapse
                foreach (KeyValuePair<string, int> entry in sensorLastFiredEventList)
                {
                    // Already drawn 6
                    if (sensorDrawn > maxSensor) break;
                    // No event
                    if (entry.Value == -1) break;
                    // Find time elapse of current entry
                    DateTime entryTime = _home.sensorEventCollection.UnfilteredStorage[entry.Value].TimeTag;
                    timeElapse = Convert.ToInt32((_home.SelectedEvent.TimeTag - entryTime).TotalSeconds);
                    // If Time elapsed greater than max time, stop here
                    if (timeElapse > maxTime) break;
                    // Otherwise, add it to sensorDrawList
                    sensorDrawList.Add(entry);
                    // Add Sensor Count
                    sensorDrawn++;
                }
                maxTimeElapse = timeElapse + 1;
                timeElapse = 0;
                foreach (KeyValuePair<string, int> entry in sensorDrawList)
                {
                    // Find time elapse of current entry
                    DateTime entryTime = _home.sensorEventCollection.UnfilteredStorage[entry.Value].TimeTag;
                    timeElapse = Convert.ToInt32((_home.SelectedEvent.TimeTag - entryTime).TotalSeconds);
                    // Update Sensor Drawn List
                    // Find Tuple in List
                    Tuple<Sensor, Rectangle, TextBlock> canvasSensorEntry = canvasSensorList.Find(x => x.Item1.name == entry.Key);
                    if (canvasSensorEntry != null)
                    {
                        Color sensorTypeColor = SensorType.GetColorFromSensorType(canvasSensorEntry.Item1.type);
                        canvasSensorEntry.Item2.Fill = new SolidColorBrush(sensorTypeColor);
                        canvasSensorEntry.Item2.Opacity = (1 - ((double)timeElapse) / maxTimeElapse) * 0.6 + 0.4;
                    }
                }
            }
            sensorCanvas.InvalidateArrange();
            sensorCanvas.UpdateLayout();
        }

        #region VerticalSplitter
        private void VerticalSplitter_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            try
            {
                dataListView.Width = dataListView.ActualWidth + e.Delta.Translation.X;
            }
            catch
            {

            }
        }

        //private void VerticalSplitter_PointerExited(object sender, PointerRoutedEventArgs e)
        //{
        //    Window.Current.CoreWindow.PointerCursor = sizeCursor;
        //}

        //private void VerticalSplitter_PointerEntered(object sender, PointerRoutedEventArgs e)
        //{
        //    Window.Current.CoreWindow.PointerCursor = arrowCursor;
        //}
        #endregion

        #region RenderEvent

        #endregion

        #region DateControl
        private void DisableDateChangeControllers()
        {
            curEventDate.IsEnabled = false;
            DateSlider.IsEnabled = false;
            btnPrevDay.IsEnabled = false;
            btnPrevWeek.IsEnabled = false;
            btnNextDay.IsEnabled = false;
            btnNextWeek.IsEnabled = false;
            btnPlay.IsEnabled = false;
        }

        private void EnableDateChangeControllers()
        {
            curEventDate.IsEnabled = true;
            DateSlider.IsEnabled = true;
            btnPrevDay.IsEnabled = true;
            btnPrevWeek.IsEnabled = true;
            btnNextDay.IsEnabled = true;
            btnNextWeek.IsEnabled = true;
            btnPlay.IsEnabled = true;
        }

        private async void curEventDate_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if(curEventDate.Date.Value != null && _home.sensorEventCollection.CurDate != curEventDate.Date.Value.DateTime)
            {
                // Loading at the moment, Disable other controls
                DisableDateChangeControllers();
                try
                {
                    await _home.LoadDateAsync(curEventDate.Date.Value.DateTime);
                } catch (InvalidOperationException e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
                EnableDateChangeControllers();
                curEventDate.SetDisplayDate(_home.sensorEventCollection.CurDate);
                DateSlider.Value = Convert.ToInt32((curEventDate.Date.Value.DateTime - curEventDate.MinDate).TotalDays + 1);
            }
        }

        private void DateSlider_ValueChangeCompleted(object sender, Controls.SliderValueChangeCompletedEventArgs args)
        {
            DateTime targetDateTime = curEventDate.MinDate.DateTime.AddDays(DateSlider.Value - 1);
            if (curEventDate.Date != null && targetDateTime != curEventDate.Date.Value.DateTime)
            {
                curEventDate.Date = targetDateTime;
            }
        }

        private void btnPrevWeek_Click(object sender, RoutedEventArgs e)
        {
            curEventDate.Date = curEventDate.Date.Value.AddDays(-7);
        }

        private void btnPrevDay_Click(object sender, RoutedEventArgs e)
        {
            curEventDate.Date = curEventDate.Date.Value.AddDays(-1);
        }

        private void btnNextDay_Click(object sender, RoutedEventArgs e)
        {
            curEventDate.Date = curEventDate.Date.Value.AddDays(1);
        }

        private void btnNextWeek_Click(object sender, RoutedEventArgs e)
        {
            curEventDate.Date = curEventDate.Date.Value.AddDays(7);
        }
        #endregion

        private void dataListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            annonateFlyout.ShowAt(sender as ListView, e.GetPosition(sender as ListView));
        }
    }
}
