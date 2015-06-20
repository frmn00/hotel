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
using SQLite;
using SQLite3 = System.Data.SQLite;

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
            object ro = MWord.WdReplace.wdReplaceAll;
            tf.Execute(Replace: ref ro, Wrap: MWord.WdFindWrap.wdFindContinue);
            return;
        }

        public static void HistoryReport(List<List<RoomInformation>> info)
        {
            var exl = new MExcel.Application();
            exl.Workbooks.Open(System.AppDomain.CurrentDomain.BaseDirectory + "templates\\history");
            var sh = (MExcel.Worksheet)exl.Worksheets.get_Item(1);
            sh.Cells[1, 1] = "Номер:";
            sh.Cells[1, 2] = "Дата:";
            sh.Cells[1, 3] = "Статус:";
            sh.Cells[1, 4] = "Долг:";
            sh.Cells[1, 5] = "ФИО:";
            sh.Cells[1, 6] = "Бельё:";
            int pos = 2;
            for (int i = 0; i < info.Count; i++)
            {
                if (info[i].Count != 0)
                {
                    sh.Cells[pos, 1] = info[i][0].Num;
                    pos++;
                }
                else continue;
                int tmp = pos;
                for (int j = 0; j < info[i].Count; j++)
                {
                    sh.Cells[tmp, 2] = info[i][j].Date.ToString();
                    tmp++;
                }
                tmp = pos;
                for (int j = 0; j < info[i].Count; j++)
                {
                    sh.Cells[tmp, 3] = info[i][j].Status;
                    tmp++;
                }
                tmp = pos;
                for (int j = 0; j < info[i].Count; j++)
                {
                    sh.Cells[tmp, 4] = info[i][j].Debt;
                    tmp++;
                }
                tmp = pos;
                for (int j = 0; j < info[i].Count; j++)
                {
                    sh.Cells[tmp, 5] = info[i][j].Name;
                    tmp++;
                }
                tmp = pos;
                for (int j = 0; j < info[i].Count; j++)
                {
                    sh.Cells[tmp, 6] = info[i][j].Pillows;
                    tmp++;
                }
                pos += info[i].Count;
            }
            sh.Columns.AutoFit();
            MExcel.Range r = sh.get_Range("A1", "F" + (pos-1).ToString());
            r.Borders.ColorIndex = 0;
            r.Borders.LineStyle = MExcel.XlLineStyle.xlContinuous;
            r.Borders.Weight = MExcel.XlBorderWeight.xlThin;
            Random rnd = new Random((int)DateTime.Now.ToFileTime());
            int num = rnd.Next(100);
            exl.Workbooks[1].SaveAs(System.AppDomain.CurrentDomain.BaseDirectory + "history" + DateTime.Now.ToShortDateString() + "_" + num.ToString() + ".xls");
            MessageBox.Show("Отчет " + "history" + DateTime.Now.ToShortDateString() + "_" + num.ToString() + ".xls" + " создан.");
            exl.Visible = true;
        }

        public static void ArrivalBlank(Room room){
            var wrd = new MWord.Application();  
            MWord.Document doc;
            try
            {
                doc = wrd.Documents.Open(System.AppDomain.CurrentDomain.BaseDirectory + "\\templates\\anketa");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            replace("<end>", DateTime.Now.ToString(), ref wrd);
            replace("<name>", room.Person.Name, ref wrd);
            replace("<soname>", room.Person.Soname, ref wrd);
            replace("<lname>", room.Person.ThName, ref wrd);
            replace("<nroom>", room.Id.ToString(), ref wrd);
            replace("<arrivetime>", room.UsedAt.Start.ToString(), ref wrd);
            replace("<bdate>", room.Person.Birthday.ToShortDateString(), ref wrd);
            replace("<home>", room.Person.PHome, ref wrd);
            replace("<pnum>", room.Person.PId, ref wrd);
            replace("<pwhen>", room.Person.PWhen.ToShortDateString(), ref wrd);
            replace("<pby>", room.Person.PPlace, ref wrd);
            replace("<bbtime>", room.UsedAt.End.ToString(), ref wrd);

            if (wrd.Dialogs[MWord.WdWordDialog.wdDialogFilePrint].Show() == 0)
            {
                wrd.ActiveDocument.Close(MWord.WdSaveOptions.wdDoNotSaveChanges);
                wrd.Quit();
            }
            wrd.Quit();
            wrd = null;
        }

        public static void InBlank(Room room)
        {
            int n = System.Convert.ToInt32(File.ReadAllLines(System.AppDomain.CurrentDomain.BaseDirectory + "\\templates\\num")[0]);
            var wrd = new MWord.Application();
            MWord.Document doc;
            try
            {
                doc = wrd.Documents.Open(System.AppDomain.CurrentDomain.BaseDirectory + "\\templates\\in");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            replace("<snum>", n.ToString(), ref wrd);
            replace("<fullname>", room.Person.ToString(), ref wrd);
            replace("<rnum>", room.Id.ToString(), ref wrd);
            replace("<datein>", room.UsedAt.Start.ToString(), ref wrd);
            replace("<dateout>", room.UsedAt.End.ToString(), ref wrd);
            replace("<sum>", room.Debt.ToString(), ref wrd); 
            if (wrd.Dialogs[MWord.WdWordDialog.wdDialogFilePrint].Show() == 0)
            {
                wrd.ActiveDocument.Close(MWord.WdSaveOptions.wdDoNotSaveChanges);
                wrd.Quit();
            }
            wrd.Quit();
            wrd = null;
            n++;
            File.WriteAllText(System.AppDomain.CurrentDomain.BaseDirectory + "\\templates\\num", n.ToString(), Encoding.UTF8);
            return;
        }
    }

    public struct RoomInformation
    {
        private DateTime date;
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public DateTime Date
        {
            get { return date; }
            set { date = value; }
        }

        private string status;
        [MaxLength(20)]
        public string Status
        {
            get { return status; }
            set { status = value; }
        }
        private string FIO;
        [MaxLength(150)]
        public string Name
        {
            get { return FIO; }
            set { FIO = value; }
        }
        private int debt;
        [Indexed]
        public int Debt
        {
            get { return debt; }
            set { debt = value; }
        }
        private int pillows;
        [Indexed]
        public int Pillows
        {
            get { return pillows; }
            set { pillows = value; }
        }
        private int num;
        [Indexed]
        public int Num
        {
            get { return num; }
            set { num = value; }
        }
        public RoomInformation(DateTime date, string status, int debt, string name, int pill, int num)
            : this()
        {
            Date = date;
            Status = status;
            Name = name;
            Debt = debt;
            Pillows = pill;
            Num = num;
        }
        public RoomInformation(bool f):this() {      
        }
    }



    public static class DataBase
    {
        public static void UpdateRoom(Room room)
        {
            if (!File.Exists("database.db"))
            {
                SQLite3.SQLiteConnection.CreateFile("database.db");
            }
            try
            {
                using (var db = new SQLite.SQLiteConnection("database.db"))
                {
                    db.CreateTable<RoomInformation>();
                    db.Insert(new RoomInformation(true)
                    {
                        Date = DateTime.Now,
                        Debt = room.Debt,
                        Num = room.Id,
                        Name = room.Person.ToString(),
                        Pillows = room.Pillows,
                        Status = room.Status == Room.stat.FREE ? "Свободен" : "Занят"
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        public static List<RoomInformation> Information(Room room, TimeRange range)
        {
            List<RoomInformation> ans = new List<RoomInformation>();
            if (!File.Exists("database.db"))
            {
                SQLite3.SQLiteConnection.CreateFile("database.db");
            }
            try
            {
                using (var db = new SQLite.SQLiteConnection("database.db"))
                {                
                    var qw = db.Query<RoomInformation>(string.Format("select * from RoomInformation where Num = {0} and Date between \"{1}\" and \"{2}\"", 
                        room.Id, range.Start.Date.ToString("yyyy-MM-dd"), range.End.Date.ToString("yyyy-MM-dd")));
                    ans = qw;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            return ans;
        }

        public static void CurrentSave(List<Room> rooms)
        {
            if (!File.Exists("database.db"))
            {
                SQLite3.SQLiteConnection.CreateFile("database.db");
            }
            try
            {
                using (var db = new SQLite.SQLiteConnection("database.db"))
                {                
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        public static List<Room> CurrentLoad()
        {
            if (!File.Exists("database.db"))
            {
                SQLite3.SQLiteConnection.CreateFile("database.db");
            }
            try
            {
                using (var db = new SQLite.SQLiteConnection("database.db"))
                {

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            return null;
        }

        public static void ClientSave(List<Person> clients)
        {
            if (!File.Exists("database.db"))
            {
                SQLite3.SQLiteConnection.CreateFile("database.db");
            }
            try
            {
                using (var db = new SQLite.SQLiteConnection("database.db"))
                {
                    db.DeleteAll<Person>();
                    db.CreateTable<Person>();
                    db.InsertAll(clients);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        public static List<Person> CilentsLoad()
        {
            List<Person> ans = new List<Person>();
            if (!File.Exists("database.db"))
            {
                SQLite3.SQLiteConnection.CreateFile("database.db");
            }
            try
            {
                using (var db = new SQLite.SQLiteConnection("database.db"))
                {
                    db.CreateTable<Person>();
                    var qw = db.Query<Person>("select * from Person");
                    ans = qw;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            return ans;
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
        public Room()
        {

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
            serial(this);
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
            if (this.num_res != -1)
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
                Report.InBlank(this);
                PayRoom(pay);
            }
            serial(this);
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
                Report.ArrivalBlank(this);
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
            this.num_res--;
            if (_date.Count == 0 && res.Count == 0 && st_room != stat.USE)
            {
                this.st_room = stat.FREE;
                Color();
            }
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
            serial(this);
        }

        public void PayRoom(int sum)
        {
            if (sum <= 0)
            {
                MessageBox.Show("Некорректная сумма.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.pay_room >= sum)
            {
                this.pay_room -= sum;
            }
            else
            {
                MessageBox.Show("Сумма превышает долг!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            serial(this);
        }

        public int DaysOfUse()
        {
            return (this.out_room - this.in_room).Days;
        }

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
        [MaxLength(50)]
        public string ThName
        {
            get { return thname; }
            set { thname = value; }
        }
        [MaxLength(50)]
        public string Soname
        {
            get { return soname; }
            set { soname = value; }
        }
        [MaxLength(50)]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }


        DateTime birthday;
        [Indexed]
        public DateTime Birthday
        {
            get { return birthday; }
            set { birthday = value; }
        }
        private string id, place, who, home;
        [MaxLength(120)]
        public string PHome
        {
            get { return home; }
            set { home = value; }
        }
        [MaxLength(150)]
        public string PWho
        {
            get { return who; }
            set { who = value; }
        }
        [MaxLength(150)]
        public string PPlace
        {
            get { return place; }
            set { place = value; }
        }
        [MaxLength(30)]
        public string PId
        {
            get { return id; }
            set { id = value; }
        }
        private DateTime when;
        [Indexed]
        public DateTime PWhen
        {
            get { return when; }
            set { when = value; }
        }


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
        public Person()
        {

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
         }

        public void serial(Room room)
        {
            saveBase(rooms, BASE);
            updateColor(rooms);
            DataBase.UpdateRoom(room);
            //PostBase("http://basehotel.16mb.com/upload.php?id=2", "base.dat");
        }
        //DEPRECATED
        //public string PostBase(string url, string filename)
        //{
        //    WebClient client = new WebClient();
        //    byte[] resp = client.UploadFile(url, filename);
        //    return System.Text.Encoding.ASCII.GetString(resp);
        //}

        //public void DownloadBase(string url, string filename)
        //{
        //    WebClient client = new WebClient();
        //    client.DownloadFile(url, filename);
        //}

        //public bool NeedReload(string url)
        //{
        //    WebClient client = new WebClient();
        //    return client.DownloadString(url) == "1";
        //}
              
        public void saveBase(Room[] rooms, string filename)
        {
            BinaryFormatter format = new BinaryFormatter();

            using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
            {
                format.Serialize(fs, rooms);
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
            if (clients.Count > 0)
                DataBase.ClientSave(clients);
        }

        public void updateColor(Room[] roomz)
        {
            for (int i = 1; i < roomz.Count(); i++ )
            {
                rooms[i].Color();
            }
        }

        //public void doMagic(object sender, EventArgs e)
        //{
        //    if(NeedReload("http://basehotel.16mb.com/check.php?id=1")){
        //        DownloadBase("http://basehotel.16mb.com/xyzzy/base.dat", "base.dat");
        //        rooms = loadBaseRoom(); 
        //        InitCls();
        //        updateColor(rooms);
        //    }
        //    else{
        //        MessageBox.Show("Нет изменений.");
        //    }
        //}

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
            this.clients = DataBase.CilentsLoad();

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

        private void GivePillows(object sender, RoutedEventArgs e)
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
            GivePillows g = new GivePillows(myroom, 1);
            g.ShowDialog();
        }

        private void RetPillows(object sender, RoutedEventArgs e)
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
            GivePillows g = new GivePillows(myroom, 0);
            g.ShowDialog();
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

        private void PayRoom(object sender, RoutedEventArgs e)
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
            if (myroom.Debt != 0)
            {
                Payment form = new Payment(myroom);
                form.ShowDialog();
            }
        }





    }
}
