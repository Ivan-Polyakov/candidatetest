using System;
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
using Newtonsoft.Json;
using System.IO;
using System.ComponentModel;
using Microsoft.Win32;

namespace CSV_Reader
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        // Десериализованный файл Json
        public Json_ItemColl JsonColl;

        // Словарь типов
        public Dictionary<string, int> TypeSizeDict = new Dictionary<string, int>
        {
            { "double", 8 },
            { "bool",1},
            { "int",4}
        };

        #region Свойства для View

        //Текущая строчка CSV
        int _CSV_CurrentLine = 0;
        public int CSV_CurrentLine
        {
            get { return _CSV_CurrentLine; }
            set
            {
                _CSV_CurrentLine = value;
                CSV_TagOffesetList = Create_TagOffsetList_FromCSV_List_Line(CSV_List, CSV_CurrentLine);
                OnPropertyChanged(nameof(CSV_CurrentLine));
            }
        }

        //Общее число строк в файле CSV
        private int _CSVLineCount;
        public int CSVLineCount
        {
            get { return _CSVLineCount; }
            set
            {
                _CSVLineCount = value;
                OnPropertyChanged(nameof(CSVLineCount));
            }
        }

        //Коллекция элементов Tag + Offset
        private List<CSV_TagOffeset> _CSV_TagOffesetList;
        public List<CSV_TagOffeset> CSV_TagOffesetList
        {
            get { return _CSV_TagOffesetList; }
            set
            {
                _CSV_TagOffesetList = value;
                OnPropertyChanged(nameof(CSV_TagOffesetList));
            }
        }
        #endregion

        List<CSV_item> _CSV_List;
        public List<CSV_item> CSV_List
        {
            get { return _CSV_List; }
            set
            {
                _CSV_List = value;
                OnPropertyChanged(nameof(CSV_List));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            try
            {
                JsonColl = JsonConvert.DeserializeObject<Json_ItemColl>(File.ReadAllText(@"..\..\candidatetest-master\TypeInfos.json"));
            }
            catch (Exception) { Console.Beep(); this.Close();};

        }
        List<CSV_item> Read_CSV_File()
        {
            string Path;
            string Line;
            string[] Split;
            string[] SplitRule = { ";" };
            List<CSV_item> CSV_List = new List<CSV_item>();

            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.DefaultExt = ".csv"; // Default file extension
            openFileDialog.Filter = "CSV file (.CSV)|*.CSV"; // Filter files by extension
            if (openFileDialog.ShowDialog() == true)
            {
                Path = openFileDialog.FileName;
                using (System.IO.TextReader tr = new StreamReader(Path))
                {
                    tr.ReadLine(); // Пропускаем первую строчку , дальше цикл
                    while ((Line = tr.ReadLine()) != null)
                    {
                        Split = Line.Split(SplitRule, System.StringSplitOptions.RemoveEmptyEntries);
                        CSV_List.Add(new CSV_item { Tag = Split[0], Type = Split[1] });
                    }
                    tr.Close();
                }
            }
                return CSV_List;
        }
 
        bool CSV_Json_Link(List<CSV_item> CSV_List, Json_ItemColl JsonColl)
        {
            foreach (CSV_item CSV_Item in CSV_List)
            {
                foreach (Json_ItemType Json_Item in JsonColl.TypeInfos)
                {
                    if (Json_Item.TypeName == CSV_Item.Type)
                    {
                        CSV_Item.Propertys = Json_Item.Propertys;
                        break;
                    }
                }
            }
            return true;
        }

        List<CSV_TagOffeset> Create_TagOffsetList_FromCSV_List_Line(List<CSV_item> CSV_List,int CSV_CurrentLine)
        {
            List<CSV_TagOffeset> TagOffsetList = new List<CSV_TagOffeset>();

            //Первый элемент
            TagOffsetList.Add(new CSV_TagOffeset {
                Tag = CSV_List[CSV_CurrentLine].Tag + "." + CSV_List[CSV_CurrentLine].Propertys.ElementAt(0).Key,
                Offset = 0});
            //Остальные элементы
            for (int i = 1; i < CSV_List[CSV_CurrentLine].Propertys.Count; i++)
            {
                TagOffsetList.Add(new CSV_TagOffeset
                {
                    Tag = CSV_List[CSV_CurrentLine].Tag + "." + CSV_List[CSV_CurrentLine].Propertys.ElementAt(i).Key,
                    Offset = TypeSizeDict[CSV_List[CSV_CurrentLine].Propertys.ElementAt(i - 1).Value.Replace("}", "")] + 
                    TagOffsetList[i - 1].Offset
                });
            }


            return TagOffsetList;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CSV_List = Read_CSV_File();

            CSV_Json_Link(CSV_List, JsonColl);

            //При открытии файла отображаем по умолчанию 1 строчку
            CSV_TagOffesetList = Create_TagOffsetList_FromCSV_List_Line(CSV_List,0);

            CSVLineCount = CSV_List.Count;
            //Активируем TextBox для ввода строки
            bt_XML.IsEnabled = true;


        }

        private void bt_XML_Click(object sender, RoutedEventArgs e)
        {
            string FilePath;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = "XML_Log"; // Default file name
            saveFileDialog.DefaultExt = ".XML"; // Default file extension
            saveFileDialog.Filter = "XML Log file (.XML)|*.XML"; // Filter files by extension
            if (saveFileDialog.ShowDialog() == true)
            {
                FilePath = saveFileDialog.FileName;
                using (TextWriter file = new StreamWriter(FilePath))
                {
                    file.WriteLine("<root>");
                    foreach (CSV_TagOffeset item in CSV_TagOffesetList)
                    {
                        file.WriteLine("\t<item Binding=\"Introduced\">");
                        file.WriteLine("\t\t<node-path>" + item.Tag+ "</node-path>");
                        file.WriteLine("\t\t<address>" + item.Offset.ToString() + "</address>");
                        file.WriteLine("\t</item>");
                    }
                    file.WriteLine("</root>");
                    file.Close();
                }
            }

        }

        public class CSV_item
        {
            public string Tag { get; set; }
            public string Type { get; set; }
            public Dictionary<string, string> Propertys { get; set; } = new Dictionary<string, string> { };
        }

        public class CSV_TagOffeset
        {
            public string Tag { get; set; }
            public int Offset { get; set; }
        }



            #region Json help types
        public class Json_ItemType
        {
            public string TypeName { get; set; }
            public Dictionary<string, string> Propertys { get; set; }
        }

        public class Json_ItemColl
        {
            public Json_ItemType[] TypeInfos;
        }
        #endregion

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion


    }
}
