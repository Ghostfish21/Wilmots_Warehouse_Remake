using System.Collections.Generic;
using UnityEngine;

namespace ModuleManager {
    public abstract class Module : MonoBehaviour {

        // #################### Module API ���󷽷� ####################
        public abstract string getName(); // ��ȡģ������, Get the name of the module
        public abstract List<string> getDependencies(); // ��ȡģ������, Get the dependencies of the module
        public abstract void construct(); // ����ģ�飬�÷����� Start �����б�����, Construct the module, this method is called in the Start method

        // #################### Unity �������ڷ��� ####################
        // Start ������Ӧ����������κεط������¶���
        protected void Start() {
            ModuleManager.inst().tryConstructModule(this);
        }

    }
}

