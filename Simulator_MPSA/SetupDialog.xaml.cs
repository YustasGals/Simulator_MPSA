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
using System.Windows.Shapes;
using Simulator_MPSA.CL;
namespace Simulator_MPSA
{
    /// <summary>
    /// Логика взаимодействия для SetupDialog.xaml
    /// </summary>
    public partial class SetupDialog : Window
    {
        SetupTableModel model;
        object _object;
        Type objectType;

        public SetupDialog(VSStruct agr)
        {
            InitializeComponent();
            _object = agr;
            model = new SetupTableModel(agr);
           // textBox_name.DataContext = model;
           // textBox_name.DataContext = model;
           // dataGrid.DataContext = model;
            this.DataContext = model;
            objectType = typeof(VSStruct);
            Title = "Настройка вспомсистемы";
        }
        public SetupDialog(KLStruct klapan)
        {
            _object = klapan;
            InitializeComponent();
            model = new SetupTableModel(klapan);
            this.DataContext = model;
            objectType = typeof(KLStruct);
            Title = "Настройка клапана (заслонки)";
        }
        public SetupDialog(ZDStruct zd)
        {
            _object = zd;
            InitializeComponent();
            model = new SetupTableModel(zd);
            this.DataContext = model;
            objectType = typeof(ZDStruct);
            Title = "Настройка задвижки";

        }
        public SetupDialog(MPNAStruct agr)
        {
            _object = agr;
            InitializeComponent();
            model = new SetupTableModel(agr);
            this.DataContext = model;
            objectType = typeof(MPNAStruct);
            Title = "Настройка агрегата МНА/ПНА";
        }
        private void button_OK_Click(object sender, RoutedEventArgs e)
        {

            // this.Close();
            //чтобы обновить табличку заново вызовем конструктор модели
            /* model = new SetupTableModel(_agr);
             this.DataContext = model;*/
             
            //запись настроек в структуру объекта из view model 
            model.ApplyChanges();

            //обновить отображение на экране
            if (objectType == typeof(KLStruct))
                model = new SetupTableModel(_object as KLStruct);
            if (objectType == typeof(VSStruct))
                model = new SetupTableModel(_object as VSStruct);
            if (objectType == typeof(ZDStruct))
                model = new SetupTableModel(_object as ZDStruct);
            if (objectType == typeof(MPNAStruct))
                model = new SetupTableModel(_object as MPNAStruct);

            this.DataContext = model;
         //   model = new SetupTableModel(_object as model.)
        }

        private void button_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
