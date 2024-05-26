using System.Collections.Generic;
using UnityEngine;

namespace ModuleManager {
    public abstract class Module : MonoBehaviour {

        // #################### Module API 抽象方法 ####################
        public abstract string getName(); // 获取模块名称, Get the name of the module
        public abstract List<string> getDependencies(); // 获取模块依赖, Get the dependencies of the module
        public abstract void construct(); // 构造模块，该方法在 Start 方法中被调用, Construct the module, this method is called in the Start method

        // #################### Unity 生命周期方法 ####################
        // Start 方法不应该在子类的任何地方被重新定义
        protected void Start() {
            ModuleManager.inst().tryConstructModule(this);
        }

    }
}

