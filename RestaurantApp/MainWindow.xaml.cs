using MaterialDesignThemes.Wpf;
using Newtonsoft.Json.Linq;
using RestSharp;
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
                MessageBox.Show($"Error: {response.ErrorMessage}");
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

        private async void GroupHeader_Click(object sender, RoutedEventArgs e)
        {
            var clickedButton = (Button)sender;
            var tableGroupId = _tableGroups.FirstOrDefault(g => g.Name == clickedButton.Content.ToString())?.Id;

            if (tableGroupId != null)
            {
                if (_selectedButton != null)
                    _selectedButton.Foreground = Brushes.White;

                clickedButton.Foreground = Brushes.Aqua;
                _selectedButton = clickedButton;

                await LoadTableDataForGroup(tableGroupId.Value);
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
                        var tablesArray = item["tables"]?.ToObject<List<Table>>();
                        if (tablesArray != null)
                            tables.AddRange(tablesArray);
                    }

                    UpdateWrapPanelContent(tables);
                }
            }
            else
                MessageBox.Show($"Error: {response.ErrorMessage}");
        }


        private void UpdateWrapPanelContent(List<Table> tables)
        {
            wrapPanelContent.Children.Clear();
            foreach (var table in tables)
            {

                if(table.TableStatusId == 1)
                {
                    var border = new Border
                    {
                        Width = 300,
                        Height = 200,
                        CornerRadius = new CornerRadius(20),
                        Padding = new Thickness(10),
                        Margin = new Thickness(10),
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#c6cbcc"))
                    };

                    var grid = new Grid();
                    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                    var textBlockTitle = new TextBlock
                    {
                        Text = table.Name,
                        FontSize = 24,
                        FontWeight = FontWeights.Bold,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Foreground = Brushes.White
                    };
                    Grid.SetRow(textBlockTitle, 0);
                    Grid.SetColumn(textBlockTitle, 0);
                    grid.Children.Add(textBlockTitle);

                    var stackPanel = new StackPanel
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    var textBlockReserve = new TextBlock
                    {
                        Text = "Empty",
                        FontSize = 16,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Foreground = Brushes.White
                    };
                    stackPanel.Children.Add(textBlockReserve);

                    Grid.SetRow(stackPanel, 1);
                    Grid.SetColumn(stackPanel, 0);
                    grid.Children.Add(stackPanel);

                    border.Child = grid;
                    wrapPanelContent.Children.Add(border);
                }
                else if (table.TableStatusId == 2)
                {
                    var border2 = new Border
                    {
                        Width = 300,
                        Height = 200,
                        CornerRadius = new CornerRadius(20),
                        Padding = new Thickness(10),
                        Margin = new Thickness(10),
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A9D7E5"))
                    };

                    var grid2 = new Grid();
                    grid2.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    grid2.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                    grid2.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    grid2.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    grid2.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    grid2.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    var stackpanel_1 = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        VerticalAlignment = VerticalAlignment.Center,
                    };

                    var icon = new PackIcon
                    {
                        Kind = PackIconKind.Account,
                        Width = 20,
                        Height = 20,
                        Margin = new Thickness(0, 0, 5, 0),
                    };

                    stackpanel_1.Children.Add(icon);

                    var textBlockReserve_1 = new TextBlock
                    {
                        Text = table.PersonCount.ToString(),
                        FontSize = 16,
                        VerticalAlignment = VerticalAlignment.Center,
                    };
                    stackpanel_1.Children.Add(textBlockReserve_1);

                    Grid.SetRow(stackpanel_1, 0);
                    Grid.SetColumn(stackpanel_1, 0);
                    grid2.Children.Add(stackpanel_1);

                    var stackpanel_2 = new StackPanel
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                    };

                    var border_3 = new Border
                    {
                        CornerRadius = new CornerRadius(10),
                        Background = Brushes.White,
                        Width = 100,
                        Height = 20,
                    };

                    stackpanel_2.Children.Add(border_3);

                    Grid.SetRow(stackpanel_2, 0);
                    Grid.SetColumn(stackpanel_2, 1);
                    grid2.Children.Add(stackpanel_2);

                    var stackpanel_3 = new StackPanel
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Center,
                    };

                    var textBlockReserve_2 = new TextBlock
                    {
                        Text = table.InsertDate,
                        FontSize = 16,
                        HorizontalAlignment = HorizontalAlignment.Right,
                    };

                    stackpanel_3.Children.Add(textBlockReserve_2);

                    var textBlockReserve_3 = new TextBlock
                    {
                        Text = table.PastMins.ToString(),
                        FontSize = 12,
                        HorizontalAlignment = HorizontalAlignment.Right,
                    };

                    stackpanel_3.Children.Add(textBlockReserve_3);

                    Grid.SetRow(stackpanel_3, 0);
                    Grid.SetColumn(stackpanel_3, 2);
                    grid2.Children.Add(stackpanel_3);

                    var textBlockReserve_4 = new TextBlock
                    {
                        Text = table.Name,
                        FontSize = 24,
                        FontWeight = FontWeights.Bold,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                    };

                    Grid.SetRow(textBlockReserve_4, 1);
                    Grid.SetColumn(textBlockReserve_4, 0);
                    Grid.SetColumnSpan(textBlockReserve_4, 3);
                    grid2.Children.Add(textBlockReserve_4);

                    var textBlockReverse_5 = new TextBlock
                    {
                        Text = "₺"+table.SumTotal,
                        FontSize = 16,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    Grid.SetRow(textBlockReverse_5, 2);
                    Grid.SetColumn(textBlockReverse_5, 0);
                    Grid.SetColumnSpan(textBlockReverse_5, 3);
                    grid2.Children.Add(textBlockReverse_5);

                    border2.Child = grid2;
                    wrapPanelContent.Children.Add(border2);
                }
                else if (table.TableStatusId == 3)
                {
                    var border = new Border
                    {
                        Width = 300,
                        Height = 200,
                        CornerRadius = new CornerRadius(20),
                        Padding = new Thickness(10),
                        Margin = new Thickness(10),
                        Background = new LinearGradientBrush
                        {
                            StartPoint = new Point(0, 0),
                            EndPoint = new Point(1, 1),
                            GradientStops ={new GradientStop(Colors.Orange, 0.0),new GradientStop(Colors.DarkOrange, 1.0)}
                        }
                    };

                    var grid = new Grid();
                    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                    var textBlockTitle = new TextBlock
                    {
                        Text = table.Name,
                        FontSize = 24,
                        FontWeight = FontWeights.Bold,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Foreground = Brushes.White
                    };
                    Grid.SetRow(textBlockTitle, 0);
                    Grid.SetColumn(textBlockTitle, 0);
                    grid.Children.Add(textBlockTitle);

                    var stackPanel = new StackPanel
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    var icon = new PackIcon
                    {
                        Kind = PackIconKind.ClockOutline,
                        Width = 24,
                        Height = 24,
                        Margin = new Thickness(20, 0, 20, 5),
                        Foreground = Brushes.White
                    };
                    stackPanel.Children.Add(icon);

                    var textBlockReserve = new TextBlock
                    {
                        Text = "Rezerve",
                        FontSize = 16,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Foreground = Brushes.White
                    };
                    stackPanel.Children.Add(textBlockReserve);

                    Grid.SetRow(stackPanel, 1);
                    Grid.SetColumn(stackPanel, 0);
                    grid.Children.Add(stackPanel);

                    border.Child = grid;
                    wrapPanelContent.Children.Add(border);
                }
                else if (table.TableStatusId == 4)
                {
                    var border4 = new Border
                    {
                        Width = 300,
                        Height = 200,
                        CornerRadius = new CornerRadius(20),
                        Padding = new Thickness(10),
                        Margin = new Thickness(10),
                        Background = new LinearGradientBrush
                        {
                            StartPoint = new Point(0, 0),
                            EndPoint = new Point(1, 1),
                            GradientStops ={new GradientStop((Color)ColorConverter.ConvertFromString("#2986cc"), 0.0),new GradientStop((Color)ColorConverter.ConvertFromString("#206ba3"), 1.0)}
                        }
                    };

                    var grid4 = new Grid();
                    grid4.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    grid4.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                    grid4.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    grid4.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    grid4.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    grid4.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    var stackpanel_1 = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        VerticalAlignment = VerticalAlignment.Center,
                    };

                    var icon = new PackIcon
                    {
                        Kind = PackIconKind.Account,
                        Width = 20,
                        Height = 20,
                        Margin = new Thickness(0, 0, 5, 0),
                    };

                    stackpanel_1.Children.Add(icon);

                    var textBlockReserve_1 = new TextBlock
                    {
                        Text = table.PersonCount.ToString(),
                        FontSize = 16,
                        VerticalAlignment = VerticalAlignment.Center,
                    };
                    stackpanel_1.Children.Add(textBlockReserve_1);

                    Grid.SetRow(stackpanel_1, 0);
                    Grid.SetColumn(stackpanel_1, 0);
                    grid4.Children.Add(stackpanel_1);

                    var stackpanel_2 = new StackPanel
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                    };

                    var border_3 = new Border
                    {
                        CornerRadius = new CornerRadius(10),
                        Background = Brushes.White,
                        Width = 100,
                        Height = 20,
                    };

                    var textBlockInBorder = new TextBlock
                    {
                        FontWeight = FontWeights.SemiBold,
                        FontSize = 16,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Text = table.UserInsertName
                    };
                    border_3.Child = textBlockInBorder;
                    stackpanel_2.Children.Add(border_3);

                    Grid.SetRow(stackpanel_2, 0);
                    Grid.SetColumn(stackpanel_2, 1);
                    grid4.Children.Add(stackpanel_2);

                    var stackpanel_3 = new StackPanel
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Center,
                    };

                    var textBlockReserve_2 = new TextBlock
                    {
                        Text = table.InsertDate,
                        FontSize = 16,
                        HorizontalAlignment = HorizontalAlignment.Right,
                    };

                    stackpanel_3.Children.Add(textBlockReserve_2);

                    var textBlockReserve_3 = new TextBlock
                    {
                        Text = table.PastMins.ToString(),
                        FontSize = 12,
                        HorizontalAlignment = HorizontalAlignment.Right,
                    };

                    stackpanel_3.Children.Add(textBlockReserve_3);

                    Grid.SetRow(stackpanel_3, 0);
                    Grid.SetColumn(stackpanel_3, 2);
                    grid4.Children.Add(stackpanel_3);

                    var stackpanel_4 = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                    };

                    var lockIcon = new PackIcon
                    {
                        Kind = PackIconKind.Lock,
                        Width = 20,
                        Height = 20,
                        Margin = new Thickness(0, 7, 5, 0),
                    };

                    stackpanel_4.Children.Add(lockIcon);

                    var textBlockReserve_4 = new TextBlock
                    {
                        Text = table.Name,
                        FontSize = 24,
                        FontWeight = FontWeights.Bold,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                    };
                    stackpanel_4.Children.Add(textBlockReserve_4);

                    Grid.SetRow(stackpanel_4, 1);
                    Grid.SetColumn(stackpanel_4, 0);
                    Grid.SetColumnSpan(stackpanel_4, 3);
                    grid4.Children.Add(stackpanel_4);

                    var textBlockReverse_5 = new TextBlock
                    {
                        Text = "₺"+table.SumTotal,
                        FontSize = 16,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    Grid.SetRow(textBlockReverse_5, 2);
                    Grid.SetColumn(textBlockReverse_5, 0);
                    Grid.SetColumnSpan(textBlockReverse_5, 3);
                    grid4.Children.Add(textBlockReverse_5);

                    border4.Child = grid4;
                    wrapPanelContent.Children.Add(border4);
                }

                else
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
