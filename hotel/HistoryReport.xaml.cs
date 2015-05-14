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
            int st = Convert.ToInt32(nStart.Text), end = Convert.ToInt32(nEnd.Text);
            if (st < 203 || st > 210 || end < 203 || end > 210 || st > end)
            {
                MessageBox.Show("Неправильно введён промежуток номеров.", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            for (int i = st; i <= end; i++)
            {
                //TODO
            }
        }

        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }



        
    }
}
