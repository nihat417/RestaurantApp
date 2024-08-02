using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace RestaurantApp
{
    public partial class MainWindow : Window
    {
        private Button _selectedButton;
        private DispatcherTimer _timer;
        private const string ApiUrl = "http://185.202.223.38:16720";
        private List<TableGroup> _tableGroups;

        public MainWindow()
        {
            InitializeComponent();
            _tableGroups = new List<TableGroup>();
            LoadDataFromApi();
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += UpdateTime!;
            _timer.Start();

            UpdateTime(null, null);
        }

        private async void LoadDataFromApi()
        {
            var client = new RestClient(ApiUrl);
            var request = new RestRequest("service/dicts/departments?idSection=1&idDepartment=0", Method.Get);
            request.AddHeader("DEVICEID", "4");
            request.AddHeader("USERID", "1");
            request.AddHeader("PROGRAMTYPE", "DESKTOP");
            request.AddHeader("LOCALE", "en");
            request.AddHeader("IND", "1");
            request.AddHeader("MACADDR", "7C:8A:E1:BC:2D:67");
            request.AddHeader("WPID", "0");
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Access-Control-Allow-Origin", "*");

            var response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                Console.WriteLine("API Response: " + response.Content);
                var jsonResponse = JObject.Parse(response.Content!);
                var tableGroups = jsonResponse["data"]?.ToObject<List<TableGroup>>();

                if (tableGroups != null)
                {
                    _tableGroups = tableGroups;
                    UpdateTableGroupsDisplay();
                }
            }
            else
            {
                MessageBox.Show($"Error: {response.ErrorMessage}");
            }
        }

        private void UpdateTableGroupsDisplay()
        {
            scrollableStackPanel.Children.Clear();
            foreach (var group in _tableGroups)
            {
                var groupPanel = new StackPanel
                {
                    Margin = new Thickness(10),
                    Orientation = Orientation.Vertical
                };

                

                var groupHeader = new Button
                {
                    Content = group.Name, 
                    FontSize = 22,
                    Height = 50,
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(20, 10, 20, 10),
                    Foreground = Brushes.White,
                    Padding = new Thickness(10),
                    Background = Brushes.Transparent, 
                    BorderBrush = Brushes.Transparent, 
                    BorderThickness = new Thickness(0) 
                };

                groupHeader.Click += GroupHeader_Click;

                async void GroupHeader_Click(object sender, RoutedEventArgs e)
                {
                    var clickedButton = (Button)sender;
                    var tableGroupId = _tableGroups.FirstOrDefault(g => g.Name == clickedButton.Content.ToString())?.Id;

                    if (tableGroupId != null)
                    {
                        if (_selectedButton != null)
                        {
                            _selectedButton.Foreground = Brushes.White;
                        }
                        clickedButton.Foreground = Brushes.Aqua;
                        _selectedButton = clickedButton;

                        await LoadTableDataForGroup(tableGroupId.Value);
                    }
                }


                var tablePanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Visibility = Visibility.Collapsed
                };

                if (group.Tables != null)
                {
                    foreach (var table in group.Tables)
                    {
                        var tableBlock = new TextBlock
                        {
                            Text = table.Name,
                            FontSize = 18,
                            Margin = new Thickness(5),
                            Background = GetTableColor(table.TableStatusId),
                            Padding = new Thickness(10)
                        };

                        var textBox = new TextBox
                        {
                            Text = table.Name, 
                            FontSize = 20,
                            Margin = new Thickness(20, 0,0,0),
                            VerticalAlignment = VerticalAlignment.Center,
                            Padding = new Thickness(10),
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(1)
                        };

                        tablePanel.Children.Add(tableBlock);
                        tablePanel.Children.Add(textBox);
                    }
                }

                groupPanel.Children.Add(groupHeader);
                groupPanel.Children.Add(tablePanel);
                scrollableStackPanel.Children.Add(groupPanel);
            }
        }

        private async Task LoadTableDataForGroup(int groupId)
        {
            var client = new RestClient(ApiUrl);
            var request = new RestRequest($"service/dicts/departments?idSection=1&idDepartment={groupId}", Method.Get);
            request.AddHeader("DEVICEID", "4");
            request.AddHeader("USERID", "1");
            request.AddHeader("PROGRAMTYPE", "DESKTOP");
            request.AddHeader("LOCALE", "en");
            request.AddHeader("IND", "1");
            request.AddHeader("MACADDR", "7C:8A:E1:BC:2D:67");
            request.AddHeader("WPID", "0");
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("Access-Control-Allow-Origin", "*");

            var response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                var jsonResponse = JObject.Parse(response.Content!);
                var data = jsonResponse["data"]?.ToObject<List<JObject>>();

                if (data != null)
                {
                    var tables = new List<Table>();

                    foreach (var item in data)
                    {
                        // Извлекаем таблицы из данных
                        var tablesArray = item["tables"]?.ToObject<List<Table>>();
                        if (tablesArray != null)
                        {
                            tables.AddRange(tablesArray);
                        }
                    }

                    // Теперь tables содержит только необходимые данные
                    UpdateWrapPanelContent(tables);
                }
            }
            else
            {
                MessageBox.Show($"Error: {response.ErrorMessage}");
            }
        }


        private void UpdateWrapPanelContent(List<Table> tables)
        {
            wrapPanelContent.Children.Clear();
            foreach (var table in tables)
            {
                var tableItem = new TextBlock
                {
                    Text = table.Name,
                    FontSize = 18,
                    Margin = new Thickness(10),
                    Padding = new Thickness(10),
                    Background = GetTableColor(table.TableStatusId),
                    Foreground = Brushes.Black,
                    Width = 150 
                };

                wrapPanelContent.Children.Add(tableItem);
            }
        }



        private Brush GetTableColor(int statusId)
        {
            return statusId switch
            {
                1 => Brushes.White,
                2 => Brushes.Green,
                3 => Brushes.Yellow,
                4 => Brushes.Blue,
                _ => Brushes.Gray
            };
        }

        

        private void UpdateTime(object sender, EventArgs e) => datetimeDisplay.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm");

        private void ScrollRight(object sender, RoutedEventArgs e)
        {
            if (scrollableStackPanel.Children.Count > 0)    
            {
                var firstChild = scrollableStackPanel.Children[0];
                scrollableStackPanel.Children.RemoveAt(0);
                scrollableStackPanel.Children.Add(firstChild);

                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + firstChild.RenderSize.Width + 50);
            }
        }
    }
}
