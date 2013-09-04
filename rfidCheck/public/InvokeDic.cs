using System;
using System.Collections.Generic;
using System.Text;

namespace LogisTechBase
{
    public class InvokeDic
    {

        Dictionary<string, bool> _FormUpdatedList = new Dictionary<string, bool>();
        public bool CheckItem(string itemName)
        {
            if (!this._FormUpdatedList.ContainsKey(itemName))
            {
                this._FormUpdatedList.Add(itemName, false);
                return true;//第一次使用该项，之前肯定不会有
            }
            return this._FormUpdatedList[itemName];
        }
        public void SetItem(string itemName,bool value)
        {
            if (!_FormUpdatedList.ContainsKey(itemName))
            {
                return;
            }
            _FormUpdatedList[itemName] = value;
        }
        public bool ChekcAllItem()
        {
            bool bOk = true;
            Dictionary<string, bool>.ValueCollection valueColl = this._FormUpdatedList.Values;

            foreach (bool b in valueColl)
            {
                if (b == false)
                {
                    bOk = false;
                    break;
                }
                bOk = true;
            }
            return bOk;
        }
    }
}
