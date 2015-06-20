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
    /// Логика взаимодействия для GivePillows.xaml
    /// </summary>
    public partial class GivePillows : Window
    {
        private Room room;
        int mode;
        public GivePillows(Room room, int mode)
        {
            this.mode = mode;
            if (mode == 0)
            {
                this.Title = "Вернуть бельё";
            }
            this.room = room;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (mode == 0)
            {
                room.RetPillow(System.Convert.ToInt32(Box.Text));
                this.Close();
                return;
            }
            room.GivePillow(System.Convert.ToInt32(Box.Text));
            this.Close();
            return;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
