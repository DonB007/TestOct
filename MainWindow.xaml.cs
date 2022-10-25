using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Data;
using System.Data.SQLite;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace TestOct
{

    public class SelectOptions
    {
        public Options EnumProperty { get; set; }
        public bool BooleanProperty { get; set; }
    }
    public enum Options
    {
        BillNo,
        BillDt,
        Amt
    }

    public class RadioButtonCheckedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture)
        {
            return value.Equals(parameter);
        }
        public object ConvertBack(object value, Type targetType, object parameter,
                                  System.Globalization.CultureInfo culture)
        {
            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }

    public class SelectedItemToItemsSource : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return null;
            return new List<object>() { value };
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IndexToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((int)value >= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class TextBoxNotEmptyValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            string str = value as string;
            bool f = !string.IsNullOrEmpty(str);


            return new ValidationResult(f, Message);
        }
        public String Message { get; set; }
    }

    public class NumericValidationRule : ValidationRule
    {

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            decimal result = 1.0M;
            bool canConvert = decimal.TryParse(value as string, out result);
            return new ValidationResult(canConvert, Message);
        }
        public String Message { get; set; }
    }

    public class StringToDecimal : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal ret = 0;
            return decimal.TryParse((string)value, out ret) ? ret : 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new BillsViewModel();
        }

        public void DoSelectedRow(object sender, MouseButtonEventArgs e)
        {
            DataGridCell cell = sender as DataGridCell;
            if (cell != null && !cell.IsEditing)
            {
                DataGridRow row = FindVisualParent<DataGridRow>(cell);
                if (row != null)
                {
                    row.IsSelected = !row.IsSelected;

                    e.Handled = true;
                }
            }
        }

        public static Parent FindVisualParent<Parent>(DependencyObject child) where Parent : DependencyObject
        {
            DependencyObject parentObject = child;

            while (!((parentObject is System.Windows.Media.Visual)
                     || (parentObject is System.Windows.Media.Media3D.Visual3D)))
            {
                if (parentObject is Parent || parentObject == null)
                {
                    return parentObject as Parent;
                }
                else
                {
                    parentObject = (parentObject as FrameworkContentElement).Parent;
                }
            }
            parentObject = VisualTreeHelper.GetParent(parentObject);
            if (parentObject is Parent || parentObject == null)
            {
                return parentObject as Parent;
            }
            else
            {
                return FindVisualParent<Parent>(parentObject);
            }
        }

        void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        void Minimize_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        void Maximize_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
                WindowState = WindowState.Maximized;
        }
        void Quit_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }

    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        protected virtual void SetValue<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            field = value;
            OnPropertyChanged(propertyName);
        }
    }

    public class Bills : ViewModelBase
    {

        private int _id;
        public int Id
        {
            get
            {
                return _id;
            }
            set
            {
                SetValue(ref _id, value);
            }
        }

        private string _party;
        public string Party
        {
            get
            {
                return _party;
            }
            set
            {
                SetValue(ref _party, value);
            }
        }

        private string _billNo;
        public string BillNo
        {
            get
            {
                return _billNo;
            }
            set
            {
                SetValue(ref _billNo, value);
            }
        }

        public string BillDt2
        {
            get
            {
                DateTime dt;
                var b = DateTime.TryParse(_billDt, out dt);
                return b ? dt.ToString("dd-MM-yyyy") : _billDt;
            }

        }

        private string _billDt;
        public string BillDt
        {
            get
            {
                DateTime dt;
                var b = DateTime.TryParse(_billDt, out dt);
                return b ? dt.ToString("yyyy-MM-dd") : _billDt;
            }

            set
            {
                SetValue(ref _billDt, value);
            }
        }

        private string _amt;
        public string Amt
        {
            get
            {
                return _amt;
            }
            set
            {
                SetValue(ref _amt, value);
            }
        }

        public string DueDt2
        {
            get
            {
                DateTime dt;
                var b = DateTime.TryParse(_dueDt, out dt);
                return b ? dt.ToString("dd-MM-yyyy") : _dueDt;
            }

        }

        private string _dueDt;
        public string DueDt
        {
            get
            {
                DateTime dt;
                var b = DateTime.TryParse(_dueDt, out dt);
                return b ? dt.ToString("yyyy-MM-dd") : _dueDt;
            }
            set
            {
                SetValue(ref _dueDt, value);
            }
        }

        private string _remarks;
        public string Remarks
        {
            get
            {
                return _remarks;
            }
            set
            {
                SetValue(ref _remarks, value);
            }
        }

        public string PaidOn2
        {
            get
            {
                DateTime dt;
                var b = DateTime.TryParse(_paidOn, out dt);
                return b ? dt.ToString("dd-MM-yyyy") : _paidOn;
            }

        }

        private string _paidOn;
        public string PaidOn
        {
            get
            {
                DateTime dt;
                var b = DateTime.TryParse(_paidOn, out dt);
                return b ? dt.ToString("yyyy-MM-dd") : _paidOn;
            }
            set
            {
                SetValue(ref _paidOn, value);
            }
        }


        //cndn table
        private int _aid;
        public int AId
        {
            get
            {
                return _aid;
            }
            set
            {
                SetValue(ref _aid, value);
            }
        }

        private string _vendor;
        public string Vendor
        {
            get
            {
                return _vendor;
            }
            set
            {
                SetValue(ref _vendor, value);
            }
        }

        private string _cndnNo;
        public string CndnNo
        {
            get
            {
                return _cndnNo;
            }
            set
            {
                SetValue(ref _cndnNo, value);
            }
        }


        private string _date;
        public string Date
        {
            get
            {
                return _date;
            }
            set
            {
                SetValue(ref _date, value);
            }
        }

        private string _value;
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                SetValue(ref _value, value);
            }
        }

        private string _usedOn;
        public string UsedOn
        {
            get
            {
                return _usedOn;
            }
            set
            {
                SetValue(ref _usedOn, value);
            }
        }

    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }

    public class BillsViewModel : ViewModelBase
    {
        public IList SelectedItems { get; set; }


        public int Cnt
        {
            get
            {
                return AllBills.OfType<Bills>().ToList().Count(x => String.IsNullOrEmpty(x.PaidOn));
            }

        }

        public int Cnt2
        {
            get
            {
                return AllBills.OfType<Bills>().ToList().Where(a => !String.IsNullOrEmpty(a.Remarks)).Count(x => String.IsNullOrEmpty(x.PaidOn));
            }

        }

        private ObservableCollection<Bills> _allBills;
        public ObservableCollection<Bills> AllBillss
        {
            get { return _allBills; }
            set
            {

                _allBills = value;
                SetValue(ref _allBills, value);
            }

        }

        private ObservableCollection<Bills> _adjBillss;
        public ObservableCollection<Bills> AdjBillss
        {
            get { return _adjBillss; }
            set
            {

                _adjBillss = value;
                SetValue(ref _adjBillss, value);
            }

        }

        private ICollectionView pendingBills;
        public ICollectionView PendingBills
        {
            get { return pendingBills; }
            set
            {
                pendingBills = value;
                SetValue(ref pendingBills, value);
            }
        }

        private ICollectionView _allBillsCollection;
        public ICollectionView AllBills
        {
            get { return _allBillsCollection; }
            set
            {

                _allBillsCollection = value;
                OnPropertyChanged("AllBills");
            }
        }

        private ObservableCollection<string> comboItems;
        public ObservableCollection<string> ComboItems
        {
            get { return comboItems; }
            set
            {
                comboItems = value;
                OnPropertyChanged("ComboItems");
            }
        }

        private ObservableCollection<string> comboItems2;
        public ObservableCollection<string> ComboItems2
        {
            get { return comboItems2; }
            set
            {
                comboItems2 = value;
                OnPropertyChanged("ComboItems2");
            }
        }


        private string _SelectedCBItem;
        public string SelectedCBItem
        {
            get { return _SelectedCBItem; }
            set
            {
                SetValue(ref _SelectedCBItem, value);
            }
        }

        private string _SelectedCBItem2;
        public string SelectedCBItem2
        {
            get { return _SelectedCBItem2; }
            set
            {
                FilterString = null;
                SetValue(ref _SelectedCBItem2, value);
                AllBills.Refresh();
                OnPropertyChanged("Cnt");
                OnPropertyChanged("Cnt2");
                PendingBills.Refresh();
            }
        }

        private string _filterString;
        public string FilterString
        {
            get { return _filterString; }
            set
            {
                _filterString = value;
                SetValue(ref _filterString, value);
            }
        }

        BillsBusinessObject bills;
        private ObservableCollection<Bills> _Bill;
        public ObservableCollection<Bills> Bill
        {
            get
            {
                _Bill = new ObservableCollection<Bills>(bills.GetBills());
                return _Bill;

            }
        }

        private ObservableCollection<Bills> _AdjustmentBill;
        public ObservableCollection<Bills> AdjustmentBill
        {
            get
            {
                _AdjustmentBill = new ObservableCollection<Bills>(bills.GetAdjustmentBills());
                return _AdjustmentBill;

            }
        }

        public int SelectedIndex { get; set; }
        object _SelectedInv;
        public object SelectedInv
        {
            get
            {
                return _SelectedInv;
            }
            set
            {
                if (_SelectedInv != value)
                {
                    _SelectedInv = value;
                    OnPropertyChanged("SelectedInv");
                }
            }
        }

        private BindingGroup _UpdateBindingGroup;
        public BindingGroup UpdateBindingGroup
        {
            get
            {
                return _UpdateBindingGroup;
            }
            set
            {
                if (_UpdateBindingGroup != value)
                {
                    _UpdateBindingGroup = value;
                    OnPropertyChanged("UpdateBindingGroup");
                }
            }
        }

        private string _myInfo;
        public string myInfo
        {
            get { return _myInfo; }
            set
            {
                SetValue(ref _myInfo, value);
            }
        }

        private string _CityName;
        public string CityName
        {
            get { return _CityName; }
            set
            {
                SetValue(ref _CityName, value);
            }
        }

        public RelayCommand GoButtonClicked { get; set; }

        public RelayCommand SearchButtonClicked { get; set; }

        public RelayCommand DocxButtonClicked { get; set; }

        public ICollectionView FilteredBills { get; private set; }
        public ICollectionView AdjBills { get; private set; }

        public SelectOptions SOptions { get; set; }


        public BillsViewModel()
        {
            CultureInfo culture = new CultureInfo("en-IN");

            SOptions = new SelectOptions();

            AllBillss = DatabaseLayer.GetAllBillsFromDB();

            AdjBillss = DatabaseLayer.GetAllAdjBillsFromDB();

            comboItems = new ObservableCollection<string>(AllBillss.Select(b => b.Party).Distinct().OrderBy(b => b).ToList());
            comboItems2 = new ObservableCollection<string>(AllBillss.Select(b => b.Party).Distinct().OrderBy(b => b).ToList());

            PendingBills = new ListCollectionView(AllBillss)
            {
                Filter = o => String.IsNullOrEmpty(((Bills)o).PaidOn)
                    && (DateTime.ParseExact(((Bills)o).DueDt, "yyyy-MM-dd", culture) >= DateTime.Today.AddDays(-10)
                        && DateTime.ParseExact(((Bills)o).DueDt, "yyyy-MM-dd", culture) < DateTime.Today.AddDays(30)
                       )
            };


            AllBills = new ListCollectionView(AllBillss)
            {
                Filter = o => ((Bills)o).Party == SelectedCBItem2
            };
            GoButtonClicked = new RelayCommand(GoFilterData);

            bills = new BillsBusinessObject();
            bills.BillChanged += new EventHandler(bills_BillChanged);


            UpdateBindingGroup = new BindingGroup { Name = "Group1" };
            CancelCommand = new RelayCommand(DoCancel);
            SaveCommand = new RelayCommand(DoSave);
            AddCommand = new RelayCommand(AddUser);
            DeleteUserCommand = new RelayCommand(DeleteUser);

            SearchButtonClicked = new RelayCommand(ComboFilterData);

            DocxButtonClicked =new RelayCommand(CreateDoc);

            FilteredBills = new ListCollectionView(AllBillss)
            {
                Filter = o => ((Bills)o).Party == SelectedCBItem && String.IsNullOrEmpty(((Bills)o).PaidOn)
            };

            AdjBills = new ListCollectionView(AdjBillss)
            {
                Filter = o => ((Bills)o).Vendor == SelectedCBItem && String.IsNullOrEmpty(((Bills)o).UsedOn)
            };
        }

        public void ComboFilterData(object param)
        {
            FilteredBills.Refresh();
            AdjBills.Refresh();

            Dictionary<string, string> dict = new Dictionary<string, string>();


            using (var connection = new SQLiteConnection("Data Source=testDB.db"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM supps";


                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var Name = reader.GetString(0);
                        var City = reader.GetString(1);

                        if (!dict.ContainsKey(Name))
                        {
                            dict.Add(Name, City);
                        }
                    }
                }
            }

            bool keyExists = dict.ContainsKey(SelectedCBItem);
            if (keyExists)
            {
                CityName=dict[SelectedCBItem];
            }
            else
            {
                CityName="";
            }

            myInfo = SelectedCBItem;
            SelectedCBItem=null;
        }

        public void GoFilterData(object param)
        {
            AllBills.Filter=FilterTask;
            OnPropertyChanged("Cnt");
            OnPropertyChanged("Cnt2");
        }


        public void CreateDoc(object param)
        {
            var window2 = Application.Current.Windows
                .Cast<Window>()
                .FirstOrDefault(window => window is MainWindow) as MainWindow;

            var billsGrid = window2.billsGrid;
            var adjGrid = window2.adjGrid;

            if (adjGrid.SelectedItems.Count > 0)
            {
                if (adjGrid.SelectedItems.Count > 3)
                {
                    MessageBox.Show("Please select 3 or less Credit/Debit notes");
                    return;
                }


                if (billsGrid.SelectedItems.Count > 0 && billsGrid.SelectedItems.Count > 8)
                {
                    MessageBox.Show("Please select atleast 1 bill and/or maximum 8 bills");
                }

                else
                {

                    if (adjGrid.SumSelectedAdjustments() > billsGrid.SumSelectedBills())
                    {
                        MessageBox.Show("Credit/Debit note total value cannot be higher than total bill value");
                        return;
                    }

                    var selectedBills = billsGrid.SelectedItems.OfType<Bills>().Select(a => a.Id);

                    string query = "UPDATE billdata SET PaidOn=@PaidOn, Remarks=@Remarks WHERE Id = @Id";

                    using (var con = new SQLiteConnection("Data Source=testDB.db"))
                    {
                        con.Open();
                        using (var cmd = new SQLiteCommand(query, con))
                        {
                            foreach (var pair in selectedBills)
                            {
                                cmd.Parameters.AddWithValue("@Id", pair);
                                cmd.Parameters.AddWithValue("@PaidOn", DateTime.Today.ToString("dd-MM-yyyy"));
                                cmd.Parameters.AddWithValue("@Remarks", "Procssed");
                            }
                            cmd.ExecuteNonQuery();
                        }

                    }

                    var selectedAdjs = adjGrid.SelectedItems.OfType<Bills>().Select(a => a.Id);

                    string _query = "UPDATE cndndata SET UsedOn=@UsedOn WHERE AId = @AId";

                    using (var con = new SQLiteConnection("Data Source=testDB.db"))
                    {
                        con.Open();
                        using (var cmd = new SQLiteCommand(_query, con))
                        {
                            foreach (var pair in selectedAdjs)
                            {
                                cmd.Parameters.AddWithValue("@AId", pair);
                                cmd.Parameters.AddWithValue("@UsedOn", DateTime.Today.ToString("dd-MM-yyyy"));
                            }
                            cmd.ExecuteNonQuery();
                        }

                    }

                    MessageBox.Show("Done");

                    AllBills.Refresh();
                    PendingBills.Refresh();
                    FilteredBills.Refresh();


                    myInfo="";
                }
            }
            else
            {
                if (billsGrid.SelectedItems.Count < 1 || billsGrid.SelectedItems.Count > 15)
                {
                    MessageBox.Show("Please select minimum 1 item and/or maximum 15 items");
                }
                else
                {

                    var selectedBills = billsGrid.SelectedItems.OfType<Bills>().Select(a => a.Id);

                    string query = "UPDATE billdata SET PaidOn=@PaidOn, Remarks=@Remarks WHERE Id = @Id";

                    using (var con = new SQLiteConnection("Data Source=testDB.db"))
                    {
                        con.Open();
                        using (var cmd = new SQLiteCommand(query, con))
                        {
                            foreach (var pair in selectedBills)
                            {
                                cmd.Parameters.AddWithValue("@Id", pair);
                                cmd.Parameters.AddWithValue("@PaidOn", DateTime.Today.ToString("dd-MM-yyyy"));
                                cmd.Parameters.AddWithValue("@Remarks", "Procssed");
                            }
                            cmd.ExecuteNonQuery();
                        }

                    }
                    MessageBox.Show("Done");

                    AllBills.Refresh();
                    PendingBills.Refresh();
                    FilteredBills.Refresh();

                    myInfo="";
                }
            }

        }


        public bool FilterTask(object value)
        {
            bool f;
            var entry = value as Bills;

            if (entry != null)
            {
                if (!string.IsNullOrEmpty(FilterString))
                {

                    switch (SOptions.EnumProperty)
                    {
                        case Options.BillNo:
                            f = entry.Party == SelectedCBItem2 && entry.BillNo.Contains(FilterString);
                            break;
                        case Options.BillDt:
                            f = entry.Party == SelectedCBItem2 && entry.BillDt == FilterString;
                            break;
                        case Options.Amt:
                            f = entry.Party == SelectedCBItem2 && entry.Amt == FilterString;
                            break;
                        default:
                            f = entry.Party == SelectedCBItem2;

                            break;
                    }


                    return f;
                }
                else
                {
                    f = entry.Party == SelectedCBItem2;
                    return f;
                }

            }

            return false;
        }

        void bills_BillChanged(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                OnPropertyChanged("Bill");
                OnPropertyChanged("AllBillss");
            }));
        }


        public RelayCommand CancelCommand { get; set; }
        public RelayCommand SaveCommand { get; set; }
        public RelayCommand AddCommand { get; set; }
        public RelayCommand DeleteUserCommand { get; set; }


        void DoCancel(object param)
        {
            UpdateBindingGroup.CancelEdit();
            if (SelectedIndex == -1)    //This only closes if new
                SelectedInv = null;
        }

        void DoSave(object param)
        {
            UpdateBindingGroup.CommitEdit();
            var bill = SelectedInv as Bills;

            if (SelectedIndex == -1)
            {
                bills.AddBill(bill);
                AllBillss.Add(bill);
                if (!ComboItems2.Select(a => a).Distinct().ToList().Contains(bill.Party))
                {
                    ComboItems.Add(bill.Party.ToString());

                    ComboItems = new ObservableCollection<string>(ComboItems.OrderBy(i => i));

                    ComboItems2.Add(bill.Party.ToString());

                    ComboItems2 = new ObservableCollection<string>(ComboItems2.OrderBy(i => i));

                    OnPropertyChanged("ComboItems");
                    OnPropertyChanged("ComboItems2");

                }

                MessageBox.Show("Data saved!");
            }
            else
            {
                bills.UpdateBill(bill);

                MessageBox.Show("Data edited!");
            }

            SelectedInv = null;
            OnPropertyChanged("Bill");
            OnPropertyChanged("AllBillss");
            OnPropertyChanged("Cnt");
            OnPropertyChanged("Cnt2");

            AllBills.Refresh();
            PendingBills.Refresh();
            FilteredBills.Refresh();
        }


        void AddUser(object param)
        {
            SelectedInv = null;
            var bill = new Bills();

            bill.BillDt=DateTime.Today.ToString("dd-MM-yyyy");
            bill.DueDt=DateTime.Today.ToString("dd-MM-yyyy");

            SelectedInv = bill;

        }
        void DeleteUser(object parameter)
        {
            var bill = SelectedInv as Bills;
            if (SelectedIndex != -1)
            {
                bills.DeleteBill(bill);

                AllBillss.Remove(bill);

                if (!AllBillss.Select(a => a.Party).Distinct().ToList().Contains(bill.Party))
                {
                    ComboItems.Remove(bill.Party.ToString());
                    ComboItems2.Remove(bill.Party.ToString());
                    OnPropertyChanged("ComboItems");
                    OnPropertyChanged("ComboItems2");
                    OnPropertyChanged("myInfo");
                }

                MessageBox.Show("Data deleted successfully!");
            }
            else
            {
                SelectedInv = null;
            }
            OnPropertyChanged("Bill");
            OnPropertyChanged("AllBillss");

            AllBills.Refresh();
            PendingBills.Refresh();
            FilteredBills.Refresh();
        }
    }

    public static class DatabaseLayer
    {

        public static ObservableCollection<Bills> GetAllBillsFromDB()
        {
            try
            {
                SQLiteConnection m_dbConnection = new SQLiteConnection("Data Source=testDB.db");
                SQLiteCommand sqlCom = new SQLiteCommand("Select * From billdata", m_dbConnection);
                SQLiteDataAdapter sda = new SQLiteDataAdapter(sqlCom);
                DataTable dt = new DataTable();
                sda.Fill(dt);
                var Bill = new ObservableCollection<Bills>();

                foreach (DataRow row in dt.Rows)
                {

                    var p = (row["PaidOn"] == DBNull.Value) ? String.Empty : (string)(row["PaidOn"]);
                    var q = (row["Remarks"] == DBNull.Value) ? String.Empty : (string)(row["Remarks"]);

                    var obj = new Bills()
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Party = (string)row["Party"],
                        BillNo = (string)row["BillNo"],
                        BillDt = (string)(row["BillDt"]),
                        Amt = (string)(row["Amt"]),
                        DueDt = (string)(row["DueDt"]),
                        PaidOn = p,
                        Remarks =q

                    };
                    Bill.Add(obj);
                    m_dbConnection.Close();

                }

                return Bill;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static ObservableCollection<Bills> GetAllAdjBillsFromDB()
        {
            try
            {
                SQLiteConnection m_dbConnection = new SQLiteConnection("Data Source=testDB.db");
                SQLiteCommand sqlCom = new SQLiteCommand("Select * From cndndata", m_dbConnection);
                SQLiteDataAdapter sda = new SQLiteDataAdapter(sqlCom);
                DataTable dt = new DataTable();
                sda.Fill(dt);
                var AdjustmentBill = new ObservableCollection<Bills>();

                foreach (DataRow row in dt.Rows)
                {

                    var p = (row["UsedOn"] == DBNull.Value) ? String.Empty : (string)(row["UsedOn"]);

                    var obj = new Bills()
                    {
                        Vendor = (string)row["Vendor"],
                        CndnNo = (string)row["CndnNo"],
                        Date = (string)(row["Date"]),
                        Value = (string)(row["Value"]),
                        UsedOn = p

                    };
                    AdjustmentBill.Add(obj);
                    m_dbConnection.Close();

                }

                return AdjustmentBill;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static int InsertBill(Bills bill)
        {
            try
            {
                const string query = "INSERT INTO billdata(Party,BillNo, BillDt,Amt,DueDt,PaidOn,Remarks) VALUES(@Party, @BillNo,@BillDt,@Amt,@DueDt,@PaidOn,@Remarks)";
                var args = new Dictionary<string, object>
                {
                    {"@Party", bill.Party},
                    {"@BillNo", bill.BillNo},
                    {"@BillDt", bill.BillDt},
                    {"@Amt", bill.Amt},
                    {"@DueDt", bill.DueDt},
                    {"@PaidOn", bill.PaidOn},
                    {"@Remarks", bill.Remarks},
                };
                return ExecuteWrite(query, args);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        internal static int UpdateBill(Bills bill)
        {

            try
            {
                const string query = "UPDATE billdata SET Party = @Party, BillNo = @BillNo, BillDt=@BillDt, Amt=@Amt, DueDt=@DueDt , PaidOn=@PaidOn, Remarks=@Remarks WHERE Id = @Id";

                var args = new Dictionary<string, object>
                {
                    {"@Id", bill.Id},
                    {"@Party", bill.Party},
                    {"@BillNo", bill.BillNo},
                    {"@BillDt", bill.BillDt},
                    {"@Amt", bill.Amt},
                    {"@DueDt", bill.DueDt},
                    {"@PaidOn", bill.PaidOn},
                    {"@Remarks", bill.Remarks},
                };

                return ExecuteWrite(query, args);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        internal static int DeleteBill(Bills bill)
        {
            try
            {
                const string query = "Delete from billdata WHERE Id = @id";
                var args = new Dictionary<string, object>
                {
                    {"@id", bill.Id}
                };
                return ExecuteWrite(query, args);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        private static int ExecuteWrite(string query, Dictionary<string, object> args)
        {
            int numberOfRowsAffected;

            using (var con = new SQLiteConnection("Data Source=testDB.db"))
            {
                con.Open();
                using (var cmd = new SQLiteCommand(query, con))
                {
                    foreach (var pair in args)
                    {
                        cmd.Parameters.AddWithValue(pair.Key, pair.Value);
                    }
                    numberOfRowsAffected = cmd.ExecuteNonQuery();
                }
                return numberOfRowsAffected;
            }
        }
    }

    public class BillsBusinessObject
    {
        internal EventHandler BillChanged;

        ObservableCollection<Bills> Bill { get; set; }
        ObservableCollection<Bills> Adj { get; set; }

        public BillsBusinessObject()
        {
            Bill = DatabaseLayer.GetAllBillsFromDB();
            Adj = DatabaseLayer.GetAllAdjBillsFromDB();
        }

        public ObservableCollection<Bills> GetBills()
        {
            return Bill = DatabaseLayer.GetAllBillsFromDB();
        }

        public ObservableCollection<Bills> GetAdjustmentBills()
        {
            return Adj = DatabaseLayer.GetAllAdjBillsFromDB();
        }

        public void AddBill(Bills bill)
        {
            DatabaseLayer.InsertBill(bill);
            OnBillChanged();
        }

        public void UpdateBill(Bills bill)
        {
            DatabaseLayer.UpdateBill(bill);
            OnBillChanged();
        }

        public void DeleteBill(Bills bill)
        {
            DatabaseLayer.DeleteBill(bill);
            OnBillChanged();
        }

        void OnBillChanged()
        {
            if (BillChanged != null)
                BillChanged(this, null);
        }

    }

    public static class DataGridExtensions
    {
        public static decimal SumSelectedBills(this DataGrid gridView)
        {
            return gridView.SelectedItems.OfType<Bills>().Sum(t => Convert.ToDecimal(t.Amt.ToString()));
        }

        public static decimal SumSelectedAdjustments(this DataGrid gridView)
        {
            return gridView.SelectedItems.OfType<Bills>().Sum(t => Convert.ToDecimal(t.Value.ToString()));
        }
    }
}
