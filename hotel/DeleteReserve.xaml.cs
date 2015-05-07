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
using Itenso.TimePeriod;

namespace hotel
{
    /// <summary>
    /// Логика взаимодействия для DeleteReserve.xaml
    /// </summary>
    public partial class DeleteReserve : Window
    {
        private Room room;
        public DeleteReserve(Room room)
        {
            this.room = room;
            InitializeComponent();
        }




        private void Delete(object sender, RoutedEventArgs e)
        {
            try
            {
                room.DeleteRes((DateTime)Reserve.SelectedItem);
                this.Close();
            }
            catch
            {
                MessageBox.Show("Выберите начало промежутка.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            foreach (TimeRange s in room.ReservedList)
            {
                Reserve.Items.Add(s.Start);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }



    }
}
