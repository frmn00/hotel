using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Net;
using System.Windows.Threading;
using Itenso.TimePeriod;
using MWord = Microsoft.Office.Interop.Word;
using MExcel = Microsoft.Office.Interop.Excel;
using System.Reflection;

namespace hotel
{

    public class RoomReserved : ApplicationException
    {

    }
    public class RoomInUse : ApplicationException
    {

    }
    public class WrondPillow : ApplicationException
    { }
    public class WrongData : ApplicationException 
    { 
    }
    public class InterTime : ApplicationException
    {

    }

    public static class Report
    {

        private static void replace(string from, string to, ref MWord.Application wrd)
        {
            MWord.Find tf = wrd.Selection.Find;
            tf.Text = from;
            tf.ClearFormatting();
            tf.Replacement.Text = to;
            tf.Replacement.ClearFormatting();
            object ro = MWord.WdReplace.wdReplaceOne;
            tf.Execute(Replace: ref ro, Wrap: MWord.WdFindWrap.wdFindContinue);
            return;
        }

        public static void HistoryReport(List<List<RoomInformation>> info)
        {
            //TODO
        }

        public static void HistoryReport(List<RoomInformation> info)
        {
            var exl = new MExcel.Application();
            exl.Workbooks.Open(System.AppDomain.CurrentDomain.BaseDirectory + "templates\\history");
            var sh = exl.Worksheets.get_Item(1);
            sh.Cells[1, 1] = "Дата:                   ";
            sh.Cells[1, 2] = "Статус:                 ";
            sh.Cells[1, 3] = "Долг:                   ";
            sh.Cells[1, 4] = "ФИО:                    ";
            sh.Cells[1, 5] = "Бельё:                  ";
            for (int i = 2; i < info.Count; i++)
            {
                sh.Cells[i, 1] = info[i - 2].Date.ToString();
            }
            for (int i = 2; i < info.Count; i++)
            {
                sh.Cells[i, 2] = info[i - 2].Status;
            }
            for (int i = 2; i < info.Count; i++)
            {
                sh.Cells[i, 3] = info[i - 2].Debt.ToString();
            }
            for (int i = 2; i < info.Count; i++)
            {
                sh.Cells[i, 4] = info[i - 2].Name;
            }
            for (int i = 2; i < info.Count; i++)
            {
                sh.Cells[i, 5] = info[i - 2].Pillows.ToString();
            }
            Random rnd = new Random((int)DateTime.Now.ToFileTime());
            int num = rnd.Next(100);
            exl.Workbooks[1].SaveAs(System.AppDomain.CurrentDomain.BaseDirectory + "history" + DateTime.Now.ToShortDateString() + "_" + num.ToString() + ".xls");
            exl.Workbooks[1].Close(); 
            exl.Quit();
            MessageBox.Show("Отчет " + "history" + DateTime.Now.ToShortDateString() + "_" + num.ToString() + ".xls" + " создан.");
        }

        public static void ArrivalBlank(Room room){
            Object miss = System.Reflection.Missing.Value;
            Object t_obj = true;
            Object f_obj = false;
            var wrd = new MWord.Application();
            wrd.Visible = true;
            Random rnd = new Random((int)DateTime.Now.ToFileTime());
            int num = rnd.Next(100);
            string pt = System.AppDomain.CurrentDomain.BaseDirectory + "anketa" + room.Person.Name + room.Person.Soname + DateTime.Now.ToShortDateString() + "_" + num.ToString() + ".doc";         
            FileInfo fn = new FileInfo(System.AppDomain.CurrentDomain.BaseDirectory + "\\templates\\anketa");
            fn.CopyTo(pt);       

            MWord.Document doc;
            try
            {
               doc = wrd.Documents.Add(pt, miss, miss, miss);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            replace("<name>", room.Person.Name, ref wrd);
            replace("<soname>", room.Person.Soname, ref wrd);
            replace("<lname>", room.Person.ThName, ref wrd);
            replace("<nroom>", room.Id.ToString(), ref wrd);
            replace("<arrivetime>", room.UsedAt.Start.ToShortDateString(), ref wrd);
            replace("<bdate>", room.Person.Birthday.ToShortDateString(), ref wrd);
            replace("<home>", room.Person.Pasport.home, ref wrd);
            replace("<pnum>", room.Person.Pasport.id, ref wrd);
            replace("<pwhen>", room.Person.Pasport.when.ToShortDateString(), ref wrd);
            replace("<pby>", room.Person.Pasport.place, ref wrd);
            replace("<bmonth>", room.Person.Birthday.Month.ToString(), ref wrd);
            replace("<bbtime>", room.UsedAt.End.ToShortDateString(), ref wrd);
            wrd.ActiveDocument.SaveAs(pt);
           // wrd.ActiveDocument.Close();
            //wrd.Quit();
            MessageBox.Show("Отчет " + room.Person.Name + room.Person.Soname + DateTime.Now.ToShortDateString()  + "_" + num.ToString() +".doc" + " создан.");
        }
    }

    public struct RoomInformation
    {
        private DateTime date;
        public DateTime Date
        {
            get { return date; }
            set { date = value; }
        }

        private string status;

        public string Status
        {
            get { return status; }
            set { status = value; }
        }
        private string FIO;

        public string Name
        {
            get { return FIO; }
            set { FIO = value; }
        }
        private int debt;

        public int Debt
        {
            get { return debt; }
            set { debt = value; }
        }
        private int pillows;

        public int Pillows
        {
            get { return pillows; }
            set { pillows = value; }
        }
        public RoomInformation(DateTime date, string status, int debt, string name, int pill)
            : this()
        {
            Date = date;
            Status = status;
            Name = name;
            Debt = debt;
            Pillows = pill;
        }
    }

    public static class DataBase
    {
        private static SqlConnection connection;
        private static bool isconnected = false;
        public static bool Connect()
        {
            string connstring = @"Data Source=(localdb)\Projects;Initial Catalog=base;
                                Integrated Security=True;Connect Timeout=30;Encrypt=False;
                                TrustServerCertificate=False;";
            connection = new SqlConnection(connstring);
            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                isconnected = false;
                return false;
            }
            isconnected = true;
            return true;
        }

        public static bool UpdateRoom(Room room)
        {
            if (connection != null)
            {
                string q = "CREATE TABLE ROOM" + room.Id.ToString() +
                    " (Date datetime not null, Status nvarchar(20) not null, Debt int not null, " +
                    "  FIO nvarchar(100), Pillows int not null, id int IDENTITY, primary key (id))";
                SqlCommand comm = new SqlCommand(q, connection);
                try
                {
                    comm.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    if (ex.Number != 2714)
                    {
                        MessageBox.Show(ex.Message);
                        return false;
                    }
                }
                q = "Insert into ROOM" + room.Id.ToString() + "(Date, Status, Debt, FIO, Pillows)" +
                    "Values (@Date, @Status, @Debt, @FIO, @Pillows)";
                comm = new SqlCommand(q, connection);
                SqlParameter param = new SqlParameter();
                param.ParameterName = "@Date";
                param.Value = DateTime.Now;
                param.SqlDbType = SqlDbType.DateTime;
                comm.Parameters.Add(param);
                param = new SqlParameter();
                param.ParameterName = "@Status";
                param.Value = room.Status == Room.stat.FREE ? "Свободен" : room.Status == Room.stat.RESERVED ? "Забронирован" : "Занят";
                param.SqlDbType = SqlDbType.NVarChar;
                comm.Parameters.Add(param);
                param = new SqlParameter();
                param.ParameterName = "@Debt";
                param.Value = room.Debt;
                param.SqlDbType = SqlDbType.Int;
                comm.Parameters.Add(param);
                param = new SqlParameter();
                param.ParameterName = "@FIO";
                if (room.Person != null)
                    param.Value = room.Person.Soname + " " + room.Person.Name + " " + room.Person.ThName;
                else
                    param.Value = "-";
                param.SqlDbType = SqlDbType.NVarChar;
                comm.Parameters.Add(param);
                param = new SqlParameter();
                param.ParameterName = "@Pillows";
                param.Value = room.Pillows;
                param.SqlDbType = SqlDbType.Int;
                comm.Parameters.Add(param);
                try
                {
                    comm.ExecuteNonQuery();
                    //MessageBox.Show("CALL");
                }
                catch (Exception ex2)
                {
                    MessageBox.Show(ex2.Message);
                    return false;
                }
                return true;
            }
            else return false;
        }

        public static List<RoomInformation> Information(Room room, TimeRange range)
        {
            if (!isconnected) Connect();
            List<RoomInformation> result = new List<RoomInformation>();
            SqlCommand ex = new SqlCommand("Select * From ROOM" + room.Id.ToString() + " Where Date Between '" + range.Start.Date.ToString("MM.dd.yyyy") + "' AND '" +
                range.End.Date.ToString("MM.dd.yyyy") + "'", connection);
            using (SqlDataReader rd = ex.ExecuteReader(CommandBehavior.CloseConnection))
            {
                if (!rd.HasRows) return result;
                while (rd.Read())
                {
                    result.Add(new RoomInformation((DateTime)rd.GetSqlDateTime(0), (string)rd.GetValue(1), (int)rd.GetValue(2), (string)rd.GetValue(3), (int)rd.GetValue(4)));
                }
            }
            return result;
        }

    }

    [Serializable]
    public class Room
    {
        
        public enum stat { FREE, USE, RESERVED };
        private stat st_room;
        private DateTime in_room, out_room;
        private int pay_room, iPillow;
        [field: NonSerialized()]
        private List<TimeRange> res = new List<TimeRange>();
        private bool is_dirty;
        private int id, mest, num_res;
        private static fuc serial;
        private Dictionary<DateTime, DateTime> _date = new Dictionary<DateTime, DateTime>();
        [field: NonSerialized()]
        private TextBlock cls;
        private Person person;

        public Person Person
        {
            get { return person; }
            set { person = value; }
        }


        public delegate void fuc(Room room);

        public static fuc Callback
        {
            set
            {
                serial = value;
            }
        }

        public stat Status
        {
            get
            {
                return st_room;
            }
            set
            {
                st_room = value;
                serial(this);
            }
        }

        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        public int Places
        {
            get
            {
                return mest;
            }
            set
            {
                mest = value;
            }
        }

        public List<TimeRange> ReservedList
        {
            get
            {
                return res;
            }
            set
            {
                res = value;
            }
        }

        public TimeRange LastReserved
        {
            get
            {
                return res.Last();
            }
            set
            {
                res.Add(value);
                _date.Add(value.Start, value.End);
            }
        }

        public bool IsDirty
        {
            get
            {
                return is_dirty;
            }
            set
            {
                is_dirty = value;
            }
        }

        public int Pillows
        {
            get
            {
                return iPillow;
            }
            set
            {
                iPillow = value;
            }
        }


        public TimeRange UsedAt
        {
            get
            {
                return new TimeRange(in_room, out_room);
            }
            set
            {
                in_room = value.Start;
                out_room = value.End;
            }
        }

        public int Debt
        {
            get
            {
                return pay_room;
            }
            set
            {
                pay_room = value;
            }
        }


        public Room(int id, int mest)
        {
            this.id = id;
            this.mest = mest;
            this.st_room = stat.FREE;
            this.is_dirty = false;
            this.num_res = 0;
        }

        public bool IsReservedToday
        {
            get
            {
                foreach (TimeRange s in this.res)
                {
                    if (s.Start == DateTime.Today)
                        return true;
                }
                return false;
            }
        }


        public void SetObj(TextBlock tx)
        {
            this.cls = tx;
            res = new List<TimeRange>();
            foreach (KeyValuePair<DateTime, DateTime> s in _date)
            {
                this.res.Add(new TimeRange(s.Key, s.Value));
            }
        }

        public void GivePillow(int i)
        {
            this.iPillow += i;
            serial(this);
        }

        public void RetPillow(int i)
        {
            if (this.iPillow >= i)
                iPillow -= i;
            else
                throw new WrondPillow();
        }

        public bool InTime(DateTime time)
        {
            foreach(TimeRange s in this.res){
                if (s.HasInside(time))
                    return true;
            }
            return false;
        }

        public void Color()
        {
            if (this.num_res != 0)
            {
                var tmp = "\n     "+this.id.ToString();
                for (int i = 0; i < this.num_res; i++)
                    tmp = tmp.Insert(i, "*");
                this.cls.Text = tmp;
            }
            switch (this.st_room)
            {
                case stat.FREE:
                    if(!this.is_dirty)
                        this.cls.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(170, 0, 0, 0));
                    else
                        this.cls.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 238, 132, 44));
                    break;
                case stat.RESERVED:
                    if (IsReservedToday)
                        this.cls.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(170, 0, 0, 0));
                    else
                        this.cls.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(170, 0, 0, 0));
                    break;
                case stat.USE:
                    this.cls.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 100, 100, 100));
                    break;
            }
        }

        public void UseRoom(DateTime date_in, DateTime date_out, int sum, int pay)
        {
            if (this.st_room == stat.USE) throw new RoomInUse();
            if (!MayRes(new TimeRange(date_in, date_out))) throw new InterTime();
            if ((date_out - date_in).Days < 0) throw new WrongData();
            if (this.st_room == stat.FREE || this.st_room == stat.RESERVED)
            {
                this.st_room = stat.USE;
                this.in_room = date_in;
                this.out_room = date_out;
                CashRoom(sum);
                PayRoom(pay);
            }
            serial(this);
            Report.ArrivalBlank(this);
            //gittest
        }

        public void FreeRoom()
        {
            if (this.st_room == stat.USE)
            {
                this.st_room = stat.FREE;
                this.is_dirty = true;
                if (this._date.Count != 0)
                    this.st_room = stat.RESERVED;
                serial(this);
            }

        }

        public void DeleteRes(DateTime time)
        {
            TimeRange todel = res.Find(
                delegate(TimeRange r)
                {
                    return r.Start == time;
                });
            this.res.Remove(todel);
            this._date.Remove(time);
            if (_date.Count == 0 && res.Count == 0 && st_room != stat.USE)
            {
                this.st_room = stat.FREE;
                Color();
            }
            this.num_res--;
            serial(this);
        }

        public bool HasDebt()
        {
            return pay_room != 0;
        }


        private bool MayRes(TimeRange r)
        {
            if (st_room == stat.USE)
            {
                var tmp = new TimeRange(in_room, out_room);
                if (tmp.IntersectsWith(r)) return false;
                foreach (TimeRange s in this.res)
                {
                    if (s.IntersectsWith(r))
                    {
                        return false;
                    }
                }
            }
            else if (st_room == stat.RESERVED)
            {
                foreach (TimeRange s in this.res)
                {
                    if (s.IntersectsWith(r))
                    {
                        return false;
                    }
                }
            }
            return true;
        }




        public void ReserveRoom(DateTime date_in, DateTime date_out)
        {
            if ((date_out - date_in).Days < 0 || (DateTime.Today-date_in).Days > 0) throw new WrongData();
            if (!MayRes(new TimeRange(date_in, date_out))) throw new InterTime();
                //if (DateTime.Today == date_in)
                    //this.cls.Background = new SolidColorBrush(Colors.Orange);
                
                this.res.Add(new TimeRange(date_in, date_out));
                this._date.Add(date_in, date_out);
            if(this.st_room == stat.FREE)
                this.st_room = stat.RESERVED;
            this.num_res++;
            serial(this);
        }

        public void CashRoom(int sum)
        {
            this.pay_room = sum;
            //serial(this);
        }

        public void PayRoom(int sum)
        {
            if (this.pay_room >= sum)
            {
                this.pay_room -= sum;
            }
            else
            {
                //throw some exeption
            }
            //serial(this);
        }

        public int DaysOfUse()
        {
            return (this.out_room - this.in_room).Days;
        }

        //public int daysOfRes()
      //  {
            
      //  }

        public void CleanRoom(){
            if (this.is_dirty)
                this.is_dirty = false;
            serial(this);
        }

        public override string ToString()
        {
            return "Номер " + this.Id.ToString();
        }
        
    }

    [Serializable]
    public class Person
    {
        string name, soname, thname;

        public string ThName
        {
            get { return thname; }
            set { thname = value; }
        }

        public string Soname
        {
            get { return soname; }
            set { soname = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }


        DateTime birthday;

        public DateTime Birthday
        {
            get { return birthday; }
            set { birthday = value; }
        }
        [Serializable]
        public struct Passport
        {
            public string id, place, who, home;
            public DateTime when;
            public Passport(string id, string place, string who, string home, DateTime when)
            {
                this.home = home;
                this.id = id;
                this.when = when;
                this.who = who;
                this.place = place;
            }
        }
        public Passport Pasport = new Passport();


        public Person(string name, string soname, string thname)
        {
            if (!(name.Length == 0 || soname.Length == 0 || thname.Length == 0))
            {
                this.name = name;
                this.soname = soname;
                this.thname = thname;
            }
            else throw new WrongData();
        }

        public override string ToString()
        {
            return String.Format("{0} {1} {2}", this.soname, this.name, this.thname);
        }

    }



    public partial class MainWindow : Window
    {

        const string BASE = "base.dat";
        const string CLBASE = "clients.dat";
        public Room[] rooms = new Room[9];
        public int st = 0;
        public List<Person> clients = new List<Person>();



        public MainWindow()
        {
            InitializeComponent();
        }

        public void InitCls()
        {
            rooms[1].SetObj(this.Room1);
            rooms[2].SetObj(this.Room2);
            rooms[3].SetObj(this.Room3);
            rooms[4].SetObj(this.Room4);
            rooms[5].SetObj(this.Room5);
            rooms[6].SetObj(this.Room6);
            rooms[7].SetObj(this.Room7);
            rooms[8].SetObj(this.Room8);
            DataBase.Connect();
         }



        public void serial(Room room)
        {
            saveBase(rooms, BASE);
            updateColor(rooms);
            DataBase.UpdateRoom(room);
            //PostBase("http://basehotel.16mb.com/upload.php?id=2", "base.dat");
        }

        public string PostBase(string url, string filename)
        {
            WebClient client = new WebClient();
            byte[] resp = client.UploadFile(url, filename);
            return System.Text.Encoding.ASCII.GetString(resp);
        }

        public void DownloadBase(string url, string filename)
        {
            WebClient client = new WebClient();
            client.DownloadFile(url, filename);
        }

        public bool NeedReload(string url)
        {
            WebClient client = new WebClient();
            return client.DownloadString(url) == "1";
        }

        
        

        public void saveBase(Room[] rooms, string filename)
        {
            BinaryFormatter format = new BinaryFormatter();

            using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
            {
                format.Serialize(fs, rooms);
            }
        }

        public void saveBaseClients(List<Person> clients, string filename)
        {
            BinaryFormatter format = new BinaryFormatter();

            using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
            {
                format.Serialize(fs, clients);
            }
        }

        public Room[] loadBaseRoom()
        {
            BinaryFormatter format = new BinaryFormatter();

            using (FileStream fs = new FileStream(BASE, FileMode.OpenOrCreate))
            {
                return (Room[])format.Deserialize(fs);
            }
        }

        public List<Person> loadBaseClient()
        {
            BinaryFormatter format2 = new BinaryFormatter();
            using (FileStream f2s = new FileStream(CLBASE, FileMode.OpenOrCreate))
            {
                return (List<Person>)format2.Deserialize(f2s);
            }
        }

        private void click(object sender, MouseButtonEventArgs e)
        {
            st = System.Convert.ToInt32((sender as TextBlock).Uid);
            RoomInfo form = new RoomInfo(rooms[st]);
            form.Owner = this;
            form.ShowDialog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            saveBase(rooms, BASE);
            if(clients.Count > 0)
            saveBaseClients(clients, "clients.dat");
        }

        public void updateColor(Room[] roomz)
        {
            for (int i = 1; i < roomz.Count(); i++ )
            {
                rooms[i].Color();
            }
        }

        public void doMagic(object sender, EventArgs e)
        {
            if(NeedReload("http://basehotel.16mb.com/check.php?id=1")){
                DownloadBase("http://basehotel.16mb.com/xyzzy/base.dat", "base.dat");
                rooms = loadBaseRoom(); 
                InitCls();
                updateColor(rooms);
            }
            else{
                MessageBox.Show("Нет изменений.");
            }
        }

        public void HistoryReports(object sender, EventArgs e)
        {
            HistoryReport rep = new HistoryReport(rooms);
            rep.Owner = this;
            rep.ShowDialog();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            Room.Callback = serial;
            if (!System.IO.File.Exists("base.dat"))
            {
                rooms[1] = new Room(203, 5);
                rooms[1].SetObj(this.Room1);
                rooms[2] = new Room(204, 4);
                rooms[2].SetObj(this.Room2);
                rooms[3] = new Room(205, 4);
                rooms[3].SetObj(this.Room3);
                rooms[4] = new Room(206, 2);
                rooms[4].SetObj(this.Room4);
                rooms[5] = new Room(207, 1);
                rooms[5].SetObj(this.Room5);
                rooms[6] = new Room(208, 1);
                rooms[6].SetObj(this.Room6);
                rooms[7] = new Room(209, 1);
                rooms[7].SetObj(this.Room7);
                rooms[8] = new Room(210, 1);
                rooms[8].SetObj(this.Room8);
                updateColor(rooms);
                
            }
            else
            {
                rooms = loadBaseRoom();
                InitCls();
                updateColor(rooms);                
            }
            if (File.Exists("clients.dat"))
            {
                this.clients = loadBaseClient();
            }
            //DateTime bg = new DateTime(2015, 4, 23);
            //DateTime ed = new DateTime(2015, 5, 15);
            //Report.HistoryReport(DataBase.Information(rooms[1], new TimeRange(bg, ed)));
        }

        private void cleanRoom(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ContextMenu menu = (ContextMenu)item.Parent;
            TextBlock block = (TextBlock)menu.PlacementTarget;
            var myroom = rooms[System.Convert.ToInt32(block.Uid)];
            if (myroom.IsDirty)
            {
                myroom.CleanRoom();
            }
            else
                MessageBox.Show("Номер чист.");
        }

        private void useRoom(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ContextMenu menu = (ContextMenu)item.Parent;
            TextBlock block = (TextBlock)menu.PlacementTarget;
            var myroom = rooms[System.Convert.ToInt32(block.Uid)];
            if (myroom.Status != Room.stat.USE)
            {
                DatePick date = new DatePick(myroom, clients);
                date.ShowDialog();
            }
            else
                MessageBox.Show("Комната занята.");
        }

        private void resRoom(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ContextMenu menu = (ContextMenu)item.Parent;
            TextBlock block = (TextBlock)menu.PlacementTarget;
            var myroom = rooms[System.Convert.ToInt32(block.Uid)];
            DateRes date = new DateRes(myroom);
            date.ShowDialog();
        }

        private void freeRoom(object sender, RoutedEventArgs e)
        {
                MenuItem item = (MenuItem)sender;
                ContextMenu menu = (ContextMenu)item.Parent;
                TextBlock block = (TextBlock)menu.PlacementTarget;
                var myroom = rooms[System.Convert.ToInt32(block.Uid)];
                if (myroom.Status == Room.stat.RESERVED)
                {
                    MessageBox.Show("Комната свободна.");
                    return;
                }
                if (myroom.Status == Room.stat.FREE)
                {
                    MessageBox.Show("Комната свободна.");
                    return;
                }
                if (MessageBox.Show("Освободить номер?", "Подтвердите действие.", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                {
                    if (myroom.Status == Room.stat.USE)
                        myroom.FreeRoom();
                }
        }

        private void dResRoom(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ContextMenu menu = (ContextMenu)item.Parent;
            TextBlock block = (TextBlock)menu.PlacementTarget;
            var myroom = rooms[System.Convert.ToInt32(block.Uid)];
            if (myroom.Status == Room.stat.RESERVED || myroom.ReservedList.Count != 0)
            {
                DeleteReserve form = new DeleteReserve(myroom);
                form.ShowDialog();
            }
        }

        private void exit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }





    }
}
