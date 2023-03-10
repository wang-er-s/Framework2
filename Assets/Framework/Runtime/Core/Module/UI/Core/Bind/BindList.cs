using System;
using System.Collections.Specialized;
using UnityEngine;

namespace Framework
{
    public class BindList<TComponent,TVm> : BaseBind where TComponent : UnityEngine.Object
    {
        private TComponent _component;
        private ObservableList<TVm> _list;
        private IBindList<TVm> _bindList;

        public void Reset(TComponent component, ObservableList<TVm> list, Action<TComponent, TVm> onCreate,
            Action<TComponent, TVm> onDestroy)
        {
            _component = component;
            this._list = list;
            InitEvent(onCreate, onDestroy);
            InitCpntValue();
        }

        private void InitCpntValue()
        {
            _bindList.GetBindListFunc()(NotifyCollectionChangedAction.Reset, default, -1);
            for (var i = 0; i < _list.Count; i++)
            {
                _bindList.GetBindListFunc()(NotifyCollectionChangedAction.Add, _list[i], i);
            }
        }

        private void InitEvent(Action<TComponent, TVm> onCreate, Action<TComponent, TVm> onDestroy)
        {
            _bindList = _bindList ?? _component as IBindList<TVm> ??
                DefaultBindList<Component, TVm>.Create(_component, onCreate, onDestroy);
            Log.Assert(_bindList != null, $"can not find IBindList of {_component}");
            _list.AddListener(_bindList.GetBindListFunc());
        }

        protected override void OnReset()
        {
            _list.RemoveListener(_bindList.GetBindListFunc());
        }

        protected override void OnClear()
        {
            _component = default;
            _list = default;
            _bindList = default;
        }
    }
}