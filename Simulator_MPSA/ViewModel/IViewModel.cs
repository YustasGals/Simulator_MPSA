using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator_MPSA.CL
{
    /// <summary>
    /// Универсальный интерфейс реализуемый ViewModel сотрудника и компании
    /// Необходим для двух целей:
    /// 1. связывание ViewModel с Model (связывание происходит в классе ViewModelCollection
    /// 2. Получение экземпляра Model через ViewModel
    /// </summary>
    /// <typeparam name="T"></typeparam>
    interface IViewModel<T>
    {
        T GetModel();
        void SetModel(T model);

        //для фильтра
        string GetTag();
        string GetName();
    }
}
