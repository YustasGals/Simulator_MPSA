using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Simulator_MPSA.CL;
using Simulator_MPSA.CL.Signal;

namespace Simulator_MPSA
{
    public abstract class AddPlantToWatch : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
       
        }

        public abstract void Execute(object parameter);
        //{

         //   throw new NotImplementedException();
       // }
    }

    public class AddZDToWatch : AddPlantToWatch
    {
        public override void Execute(object parameter)
        {
 
                System.Collections.IList list = (System.Collections.IList)parameter;
                foreach (ZDStruct zd in list.Cast<ZDStruct>())
                {
                    if (zd.OKC!=null)
                    WatchItem.Items.Add(new WatchItem(zd.OKC));

                    if (zd.CKC!=null)
                    WatchItem.Items.Add(new WatchItem(zd.CKC));

                    if (zd.ODC!=null)
                    WatchItem.Items.Add(new WatchItem(zd.ODC));

                    if (zd.CDC != null)
                    WatchItem.Items.Add(new WatchItem(zd.CDC));

                    if (zd.DC != null)
                    WatchItem.Items.Add(new WatchItem(zd.DC));

                    if (zd.BS != null)
                    WatchItem.Items.Add(new WatchItem(zd.BS));

                    if (zd.DOB != null)
                    WatchItem.Items.Add(new WatchItem(zd.DOB));

                    if (zd.DKB !=null)
                    WatchItem.Items.Add(new WatchItem(zd.DKB));

                    if (zd.DCB != null)
                    WatchItem.Items.Add(new WatchItem(zd.DCB));

                    if (zd.DCBZ != null)
                    WatchItem.Items.Add(new WatchItem(zd.DCBZ));

                    if (zd.MC != null)
                    WatchItem.Items.Add(new WatchItem(zd.MC));

                    
                    foreach (DIItem item in zd.CustomDIs)
                     if (item.DI!=null)
                        WatchItem.Items.Add(new WatchItem(item.DI));

                if (zd.ZD_position_ai != null)
                    WatchItem.Items.Add(new WatchItem(zd.ZD_position_ai));
                }
        }
    }

    public class AddVSToWatch : AddPlantToWatch
    {
        public override void Execute(object parameter)
        {
            System.Collections.IList list = (System.Collections.IList)parameter;
            foreach (VSStruct vs in list.Cast<VSStruct>())
            {
                if (vs.EC != null)
                    WatchItem.Items.Add(new WatchItem(vs.EC));

                if (vs.PC != null)
                    WatchItem.Items.Add(new WatchItem(vs.PC));


                if (vs.BS != null)
                    WatchItem.Items.Add(new WatchItem(vs.BS));


                if (vs.MPC != null)
                    WatchItem.Items.Add(new WatchItem(vs.MPC));

                if (vs.ABB != null)
                    WatchItem.Items.Add(new WatchItem(vs.ABB));

                if (vs.ABO != null)
                    WatchItem.Items.Add(new WatchItem(vs.ABO));

                if (vs.controledAIs != null && vs.controledAIs.Length > 0)
                    foreach (AnalogIOItem a in vs.controledAIs)
                        WatchItem.Items.Add(new WatchItem(a.AI));

                if (vs.AnalogCommand != null)
                    WatchItem.Items.Add(new WatchItem(vs.AnalogCommand));

            }
        }
    }

    public class AddKLToWatch : AddPlantToWatch
    {
        public override void Execute(object parameter)
        {
            System.Collections.IList list = (System.Collections.IList)parameter;
            foreach (KLStruct kl in list.Cast<KLStruct>())
            {
                if (kl.OKC != null)
                    WatchItem.Items.Add(new WatchItem(kl.OKC));

                if (kl.CKC != null)
                    WatchItem.Items.Add(new WatchItem(kl.CKC));

                if (kl.DOB != null)
                    WatchItem.Items.Add(new WatchItem(kl.DOB));

                if (kl.DKB != null)
                    WatchItem.Items.Add(new WatchItem(kl.DKB));
            }
        }
    }

    public class AddMPNAToWatch : AddPlantToWatch
    {
        public override void Execute(object parameter)
        {
            //throw new NotImplementedException();
            System.Collections.IList list = (System.Collections.IList)parameter;
            foreach (MPNAStruct agr in list.Cast<MPNAStruct>())
            {
                foreach (DIStruct di in agr.GetDIs())
                {
                    WatchItem.Items.Add(new WatchItem(di));
                }

                foreach (DOStruct d in agr.GetDOs())
                {
                    WatchItem.Items.Add(new WatchItem(d));
                }

                if (agr.controledAIs != null && agr.controledAIs.Length > 0)
                {
                    foreach (AnalogIOItem ai in agr.controledAIs)
                        WatchItem.Items.Add(new WatchItem(ai.AI));
                }

            }
        }
    }
}
