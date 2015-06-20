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
    /// Логика взаимодействия для RoomInfo.xaml
    /// </summary>
    public partial class RoomInfo : Window
    {
        MainWindow main;
        Room room;
        public RoomInfo(Room room)
        {
            if (main != null) this.main = (MainWindow)this.Owner;
            if (room != null) this.room = room;
            InitializeComponent();
        }


        public void inUse()
        {
            Stat.Foreground = new SolidColorBrush(Colors.Red);
            Stat.Content = "Занят";
            if (room.HasDebt())
            {
                tDate.Text += "Долг " + room.Debt.ToString() + "p\n";
            }
            tDate.Text += "Занят до: " + System.Convert.ToString(room.UsedAt.End.ToString()+"\n");
            tDate.Text += "ФИО: " + room.Person.ToString()+"\n";
            foreach (TimeRange s in room.ReservedList)
            {
                tDate.Text += String.Format("Забронирован с {0}, по {1}" + System.Environment.NewLine, s.Start.ToString().Substring(0, 10), s.End.ToString().Substring(0, 10));
            }
            tDate.Text += "Бельё: " + room.Pillows.ToString() + "\n";
        }

        public void inRes()
        {
            Stat.Foreground = new SolidColorBrush(Colors.Orange);
            Stat.Content = "Забронирован";
            foreach (TimeRange s in room.ReservedList)
            {
                tDate.Text += String.Format("Забронирован с {0}, по {1}"+System.Environment.NewLine, s.Start.ToString().Substring(0, 10), s.End.ToString().Substring(0, 10));
            }
        }

        public void inFree()
        {
            Stat.Foreground = new SolidColorBrush(Colors.Green);
            Stat.Content = "Свободен";
            if (room.IsDirty)
            {
                tDate.Text += "Необходимо помыть номер.\n";
            }
        }

        public void statRoom()
        {
            switch (room.Status)
            {
                case Room.stat.FREE:
                    {
                        inFree();
                        break;
                    }
                case Room.stat.RESERVED:
                    {
                        inRes();
                        break;
                    }
                case Room.stat.USE:
                    {
                        inUse();
                        break;
                    }
            }

        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            statRoom();
            RoomBox.Header = "Номер " + System.Convert.ToString(room.Id);
        }




    }


}
