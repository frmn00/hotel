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
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace hotel
{
    /// <summary>
    /// Логика взаимодействия для AddClient.xaml
    /// </summary>
    public partial class AddClient : Window
    {
        Room room;
        List<Person> clients;
        Person mypers;
        public AddClient(Room room, List<Person> clients)
        {
            this.room = room;
            this.clients = clients;
            InitializeComponent();
        }



        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Person newpers = new Person(Name.Text, Soname.Text, ThName.Text);
                if (!(WherePass.Text.Length == 0 || WhenPass.Text.Length == 0 || IdPass.Text.Length == 0 || WhoPass.Text.Length == 0 || Birthday.Text.Length == 0))
                {
                    newpers.Pasport.home = WherePass.Text;
                    newpers.Pasport.id = IdPass.Text;
                    newpers.Pasport.when = DateTime.Parse(WhenPass.Text);
                    newpers.Birthday = DateTime.Parse(Birthday.Text);
                    newpers.Pasport.who = WhoPass.Text;
                    newpers.Pasport.place = WherePass.Text;
                    clients.Add(newpers);
                    this.mypers = newpers;
                    this.DialogResult = true;
                    this.Close();
                }
                else throw new ArgumentException();                
            }
            catch (WrongData)
            {
                MessageBox.Show("Не введены ФИО.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch(ArgumentException)
            {
                MessageBox.Show("Не введены данные.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (FormatException)
            {
                MessageBox.Show("Неправильно введена дата.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch
            {
                MessageBox.Show("Неправильно введена дата.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            saveBaseClients(clients, "clients.dat");
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void saveBaseClients(List<Person> clients, string filename)
        {
            BinaryFormatter format = new BinaryFormatter();

            using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
            {
                format.Serialize(fs, clients);
            }
        }

        public Person returnPers()
        {
            return mypers;
        }
    }
}
