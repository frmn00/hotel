using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace hotel
{
    /// <summary>
    /// Логика взаимодействия для HistoryReport.xaml
    /// </summary>
    public partial class HistoryReport : Window
    {
        Room[] rooms;
        Dictionary<int, Room> d;
        public HistoryReport(Room[] rooms)
        {
            this.rooms = rooms;
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            d = new Dictionary<int, Room>();
            d[203] = rooms[1];
            d[204] = rooms[2];
            d[205] = rooms[3];
            d[206] = rooms[4];
            d[207] = rooms[5];
            d[208] = rooms[6];
            d[209] = rooms[7];
            d[210] = rooms[8];
        }

        private void bOk_Click(object sender, RoutedEventArgs e)
        {
            DataBase.Connect();
            int n_begin = nStart.Text == "" ? 203 : System.Convert.ToInt32(nStart.Text), 
                n_end = nEnd.Text == "" ? 210 : System.Convert.ToInt32(nEnd.Text);
            DateTime t_begin = Start.SelectedDate == null ? new DateTime(1980, 1, 1) : (DateTime)Start.SelectedDate,
                t_end = End.SelectedDate == null ? new DateTime(2050, 1, 1) : (DateTime)End.SelectedDate;

            if (n_begin > n_end || n_end > 210 || n_begin < 203)
            {
                MessageBox.Show("Неправильно введён промежуток номеров.", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var lst = new List<List<RoomInformation>>();
            for (int i = n_begin; i <= n_end; i++)
            {
                lst.Add(DataBase.Information(d[i], new Itenso.TimePeriod.TimeRange(t_begin, t_end)));
            }

            Report.HistoryReport(lst);
            this.Close();
            return;
        }

        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }



        
    }
}
