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
    /// Логика взаимодействия для DateRes.xaml
    /// </summary>
    public partial class DateRes : Window
    {
        Room room;
        public DateRes(Room room)
        {
            this.room = room;
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                room.ReserveRoom((DateTime)iDate.SelectedDate, (DateTime)oDate.SelectedDate);
                this.Close();
            }
            catch (InterTime)
            {
                MessageBox.Show("Номер забронирован/занят в этот период.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (WrongData)
            {
                MessageBox.Show("Неправильно введена дата.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("Введите дату.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
