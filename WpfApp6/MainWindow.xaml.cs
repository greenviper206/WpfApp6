using WpfApp6;
using LiveCharts;
using LiveCharts.Wpf;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection.Emit;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace WpfApp6
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        String aqiURL = "https://data.moenv.gov.tw/api/v2/aqx_p_432?api_key=b7df779e-71a6-4148-8379-5afbd441d803&limit=1000&sort=ImportDate%20desc&format=JSON";
        AQIData aqiData = new AQIData();
        List<Field> fields = new List<Field>();
        List<Record> records = new List<Record>();
        SeriesCollection seriesCollection = new SeriesCollection();
        List<Record> selectedRecords = new List<Record>();
        public MainWindow()
        {
            InitializeComponent();
            UrlTextBox.Text = aqiURL;
        }

        private async void GetAqiButton_Click(object sender, RoutedEventArgs e)
        {
            string url = UrlTextBox.Text;
            ContentTextBox.Text = "抓取資料中......";

            String data = await GetAQIAsync(url);
            ContentTextBox.Text = data;

            if (string.IsNullOrWhiteSpace(data))
            {
                MessageBox.Show("AQI 資料取得失敗");
                return;
            }

            ContentTextBox.Text = data;
            aqiData = JsonSerializer.Deserialize<AQIData>(data);

            fields = aqiData.fields.ToList();
            records = aqiData.records.ToList();
            selectedRecords = records;
            StatusBarText.Text = $"欄位數: {fields.Count}, 紀錄數: {records.Count}";

            DisplayAqiData();
        }

        private void DisplayAqiData()
        {
            RecordDataGrid.ItemsSource = records;

            if (records == null || records.Count == 0)
            {
                MessageBox.Show("沒有任何 AQI 紀錄");
                return;
            }
            FieldWrapPanel.Children.Clear();

            Record record = records[0];

            foreach (Field field in fields)
            {
                var propertyInfo = record.GetType().GetProperty(field.id);
                if (propertyInfo != null)
                {
                    var value = propertyInfo.GetValue(record) as String;
                    if (double.TryParse(value, out double v))
                    {
                        CheckBox cb = new CheckBox
                        {
                            Content = field.info.label,
                            Tag = field.id,
                            Margin = new Thickness(3),
                            FontSize = 14,
                            FontWeight = FontWeights.Bold,
                            Width = 150
                        };
                        cb.Checked += UpdateChart;
                        cb.Unchecked += UpdateChart;
                        FieldWrapPanel.Children.Add(cb);
                    }
                }
            }
        }

        private void UpdateChart(object sender, RoutedEventArgs e)
        {
            seriesCollection.Clear();

            foreach (CheckBox cb in FieldWrapPanel.Children)
            {
                if (cb.IsChecked == true)
                {
                    List<string> labels = new List<string>();
                    string tag = cb.Tag as string;
                    AqiChart.AxisX.Clear();
                    AqiChart.AxisX.Add(new Axis
                    {
                        Labels = labels
                    });
                    ColumnSeries columnSeries = new ColumnSeries();
                    ChartValues<double> values = new ChartValues<double>();

                    foreach (Record r in selectedRecords)
                    {
                        var propertyInfo = r.GetType().GetProperty(tag);
                        if (propertyInfo != null)
                        {
                            var value = propertyInfo.GetValue(r) as String;
                            if (double.TryParse(value, out double v))
                            {
                                values.Add(v);
                                labels.Add(r.sitename);
                            }
                        }
                    }
                    
                    columnSeries.Values = values;
                    columnSeries.Title = tag;
                    columnSeries.LabelPoint = point => $"{labels[(int)point.X]}: {point.Y.ToString()}";
                    seriesCollection.Add(columnSeries);
                }
            }
            AqiChart.Series = seriesCollection;
        }

        private async Task<string> GetAQIAsync(string url)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        return content;
                    }
                    else
                    {
                        ContentTextBox.Text = $"Error: {response.StatusCode}";
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                ContentTextBox.Text = $"Exception: {ex.Message}";
                return null;
            }
        }

        private void RecordDataGrid_LoadingRow(object sender, System.Windows.Controls.DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void RecordDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedRecords = RecordDataGrid.SelectedItems.OfType<Record>().ToList();
            StatusBarText.Text = $"欄位數: {fields.Count}, 紀錄數: {records.Count}, 已選取紀錄數: {selectedRecords.Count}";
        }
    }
}