using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using WpfApp6._2025_WpfApp6;

namespace WpfApp6
{
    public partial class MainWindow : Window
    {
        

     

        String aqiURL = "https://data.moenv.gov.tw/api/v2/aqx_p_432?api_key=b7df779e-71a6-4148-8379-5afbd441d803&limit=1000&sort=ImportDate%20desc&format=JSON";
        AQIData aqiData = new AQIData();
        List<Field> fields = new List<Field>();
        List<Record> records = new List<Record>();
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

            aqiData = JsonSerializer.Deserialize<AQIData>(data);
            fields = aqiData.fields.ToList();
            records = aqiData.records.ToList();
            StatusBarText.Text = $"欄位數: {fields.Count}, 紀錄數: {records.Count}";

            //DisplayAqiData();
        }
        private void DisplayAqiData()
        {
            throw new NotImplementedException();
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
    }
}