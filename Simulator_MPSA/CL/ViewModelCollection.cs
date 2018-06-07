using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Data;
using Simulator_MPSA.ViewModel;

namespace Simulator_MPSA.CL
{
   /// <summary>
   /// Универсальный шаблон ViewModel для отображения коллекций в DataGrid
   /// класс реализует синхронизацию между коллекцией ViewModel и коллекцией Model
   /// </summary>
   /// <typeparam name="TViewModel">Тип ViewModel элемента коллекции </typeparam>
   /// <typeparam name="TModel">Тип Model элемента коллекции </typeparam>
    public class ViewModelCollection<TViewModel, TModel>
        where TViewModel:class, IViewModel<TModel>, new()
        where TModel:class, new()
    {
        public ObservableCollection<TViewModel> VMCollection
        { set; get; }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        protected virtual void OnCollectionChanged(NotifyCollectionChangedAction action)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action));
        }
        /// <summary>
        /// Создает ViewModel коллекции и связывает ее с Model
        /// </summary>
        /// <param name="models">коллекция моделей</param>
        public ViewModelCollection(ObservableCollection<TModel> models)
        {
            VMCollection = new ObservableCollection<TViewModel>();
            VMCollection.CollectionChanged += ViewModelCollection_CollectionChanged;


            modelCollection = models;
            if (modelCollection != null)
            {
                modelCollection.CollectionChanged += ModelCollectionChanged;
                FetchFromModel();
            }
            viewSource.Source = VMCollection;
           // viewSource.Filter += new FilterEventHandler(Name_Filter);
        }

        public ObservableCollection<TModel> modelCollection;
       // TModel modelItem;
        /// <summary>
        /// Забираем все элементы из коллекции в модели
        /// </summary>
        void FetchFromModel()
        {
            VMCollection.CollectionChanged -= ViewModelCollection_CollectionChanged;

            VMCollection.Clear();
            if (modelCollection != null)
            foreach (TModel c in modelCollection)
            {
                TViewModel vm = new TViewModel();
                vm.SetModel(c);
                VMCollection.Add(vm);
            }
            VMCollection.CollectionChanged += ViewModelCollection_CollectionChanged;
        }
        /// <summary>
        /// Обработка события при изменении коллекции модели представления
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewModelCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            TModel item;
            modelCollection.CollectionChanged -= ModelCollectionChanged;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    item = (e.NewItems[0] as TViewModel).GetModel();

                    modelCollection.Add(item);

                    break;
                case NotifyCollectionChangedAction.Remove:
                    item = (e.OldItems[0] as TViewModel).GetModel();

                    modelCollection.Remove(item);

                    break;
            }
            modelCollection.CollectionChanged += ModelCollectionChanged;
        }

        /// <summary>
        /// Обработка события при изменении коллекции модели
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ModelCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            VMCollection.CollectionChanged -= ViewModelCollection_CollectionChanged;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    /*item = (e.NewItems[0] as TModel);
                    TViewModel viewItem = new TViewModel();
                    viewItem.SetModel(item);
                    VMCollection.Add(viewItem);*/
                    FetchFromModel();

                    break;
                case NotifyCollectionChangedAction.Remove:
                    FetchFromModel();

                    break;
            }

            VMCollection.CollectionChanged += ViewModelCollection_CollectionChanged;
        }

        private CollectionViewSource viewSource = new CollectionViewSource();
        public CollectionViewSource ViewSource
        { get { return viewSource; } }

        private string tagFilter = "";
        public string TagFilter
        {
            get { return tagFilter; }
            set { tagFilter = value; ApplyFilter(); }
        }
        private bool _hideEmpty = false;

        public bool HideEmpty
        {
            get {
                return _hideEmpty;
            }
            set {
                _hideEmpty = value;
                ApplyFilter();
            }
        }
      
        private string _nameFilter = "";
        public string NameFilter
        {
            get { return _nameFilter; }
            set
            {
                _nameFilter = value;
                ApplyFilter();
                /*viewSource.LiveFilteringProperties.Clear();
                viewSource.LiveFilteringProperties.Add(_nameFilter);*/
            }
        }
        /*private void Name_Filter(object sender, FilterEventArgs e)
        {
            IViewModel<TViewModel> item = e.Item as IViewModel<TViewModel>;

            if (item != null)
                if (item.GetName() != null)
                {
                    {
                        //if (item.NameAI.Contains(_nameFilter)) 
                        if (item.GetName().ToLower().Contains(NameFilter.ToLower()))
                            e.Accepted = true;
                        else e.Accepted = false;
                    }
                }
                else
                    e.Accepted = false;

        }*/
        public void ApplyFilter()
        {
 
            viewSource.Filter += new FilterEventHandler(Filter_Func);
        }
        private void Filter_Func(object sender, FilterEventArgs e)
        {
            IViewModel<TModel> item = e.Item as IViewModel<TModel>;

            if (item != null)
            {
                if (item.GetName() == "")
                    e.Accepted = !_hideEmpty;
                else
                {
                    if ((item.GetName().ToLower().Contains(_nameFilter.ToLower())) && (item.GetTag().ToLower().Contains(tagFilter.ToLower())))
                        e.Accepted = true;
                    else
                        e.Accepted = false;
                }
            }
        }



    }
}
