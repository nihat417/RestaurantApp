using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RestaurantApp
{
    public partial class MainWindow : Window
    {
        private List<TableGroup> tableGroups = new List<TableGroup>();

        public MainWindow()
        {
            InitializeComponent();
            LoadTableGroups();
        }

        private async void LoadTableGroups()
        {
            var client = new RestClient("http://185.202.223.38:16720");
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
                var data = JsonConvert.DeserializeObject<ApiResponse>(response.Content);
                tableGroups = data.Data;
                DisplayTableGroups();
            }
            else
            {
                MessageBox.Show("Error fetching table groups.");
            }
        }

        private void DisplayTableGroups()
        {
            TableGroupPanel.Children.Clear();
            foreach (var group in tableGroups)
            {
                var groupButton = new Button
                {
                    Content = group.Name,
                    Tag = group.Id,
                    Margin = new Thickness(5),
                    Width = 150,
                    Height = 100,
                    Background = new SolidColorBrush(Color.FromRgb(0x5A, 0xA3, 0xFF)),
                    Foreground = Brushes.White,
                    FontSize = 16,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                groupButton.Click += GroupButton_Click;
                TableGroupPanel.Children.Add(groupButton);

                if (group.Tables != null)
                {
                    foreach (var table in group.Tables)
                    {
                        var tableButton = new Button
                        {
                            Content = $"{table.Name}\n{table.PersonCount} человек\n₺ {table.TotalDecreased}",
                            Margin = new Thickness(5),
                            Width = 150,
                            Height = 100,
                            Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xA5, 0x00)),
                            Foreground = Brushes.White,
                            FontSize = 14,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        tableButton.Visibility = Visibility.Collapsed;
                        TableGroupPanel.Children.Add(tableButton);
                    }
                }
            }
        }

        private async void GroupButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var groupId = (int)button.Tag;

            var group = tableGroups.First(g => g.Id == groupId);
            if (group.Tables == null)
            {
                var client = new RestClient("http://185.202.223.38:16720");
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
                    var data = JsonConvert.DeserializeObject<ApiResponse>(response.Content);
                    group.Tables = data.Data.First(g => g.Id == groupId).Tables;
                    DisplayTableGroups();
                }
                else
                {
                    MessageBox.Show("Error fetching tables.");
                }
            }
            else
            {
                foreach (var child in TableGroupPanel.Children)
                {
                    if (child is Button tableButton && tableButton.Tag is int id && id == groupId)
                    {
                        tableButton.Visibility = tableButton.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    }
                }
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchTerm = SearchBox.Text.ToLower();
            var filteredGroups = tableGroups.Where(group =>
                group.Name.ToLower().Contains(searchTerm) ||
                (group.Tables != null && group.Tables.Any(table => table.Name.ToLower().Contains(searchTerm)))
            ).ToList();
            DisplayFilteredTableGroups(filteredGroups);
        }

        private void DisplayFilteredTableGroups(List<TableGroup> filteredGroups)
        {
            TableGroupPanel.Children.Clear();
            foreach (var group in filteredGroups)
            {
                var groupButton = new Button
                {
                    Content = group.Name,
                    Tag = group.Id,
                    Margin = new Thickness(5),
                    Width = 150,
                    Height = 100,
                    Background = new SolidColorBrush(Color.FromRgb(0x5A, 0xA3, 0xFF)),
                    Foreground = Brushes.White,
                    FontSize = 16,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                groupButton.Click += GroupButton_Click;
                TableGroupPanel.Children.Add(groupButton);

                if (group.Tables != null)
                {
                    foreach (var table in group.Tables)
                    {
                        var tableButton = new Button
                        {
                            Content = $"{table.Name}\n{table.PersonCount} человек\n₺ {table.TotalDecreased}",
                            Margin = new Thickness(5),
                            Width = 150,
                            Height = 100,
                            Background = new SolidColorBrush(Color.FromRgb(0xFF, 0xA5, 0x00)),
                            Foreground = Brushes.White,
                            FontSize = 14,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        tableButton.Visibility = Visibility.Visible;
                        TableGroupPanel.Children.Add(tableButton);
                    }
                }
            }
        }
    }

    public class ApiResponse
    {
        public List<TableGroup> Data { get; set; }
    }
}
