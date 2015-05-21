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

    public class NoPerson : Exception { };
    /// <summary>
    /// Логика взаимодействия для DatePick.xaml
    /// </summary>
    public partial class DatePick : Window
    {
        Room room;
        List<Person> clients;
        public DatePick(Room room, List<Person> clients)
        {
            this.room = room;
            this.clients = clients;
            InitializeComponent();

        }

        private void bOk_Click(object sender, RoutedEventArgs e)
        {
            if (date.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            DateTime dout = ((DateTime)date.SelectedDate);
            dout = dout.AddHours(DateTime.Now.Hour);
            dout = dout.AddMinutes(DateTime.Now.Minute);
            dout = dout.AddSeconds(DateTime.Now.Second);
            try
            {
                if (Client.SelectedItem == null) throw new NoPerson();
                room.Person = (Person)Client.SelectedItem;
                room.UseRoom(DateTime.Now, dout, System.Convert.ToInt32(Sum.Text), System.Convert.ToInt32(Pay.Text));
                this.Close();
            }
            catch (InterTime)
            {
                MessageBox.Show("Номер забронирован.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch(RoomReserved)
            {
                MessageBox.Show("Номер забронирован.", "Ошибка", MessageBoxButton.OK ,MessageBoxImage.Error);
            }
            catch (RoomInUse)
            {
                MessageBox.Show("Номер занят.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch(InvalidOperationException)
            {
                MessageBox.Show("Введите дату.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (NoPerson)
            {
                MessageBox.Show("Выберите клиента.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (WrongData)
            {
                MessageBox.Show("Неверно введена дата.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void isdight(object sender, TextCompositionEventArgs e)
        {
            e.Handled = "0123456789".IndexOf(e.Text) < 0;
        }

        private void Pay_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = "0123456789".IndexOf(e.Text) < 0;
        }

        private void NewClient_Click(object sender, RoutedEventArgs e)
        {
            AddClient cls = new AddClient(this.room, this.clients);
            cls.ShowDialog();
            if (cls.DialogResult == true)
            {
                Client.Items.Add(cls.returnPers());
                
            }
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            foreach (Person s in clients)
            {
                Client.Items.Add(s);
            }
        }
    }
}
