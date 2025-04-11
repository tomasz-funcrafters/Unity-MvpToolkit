using System;
using System.Collections.Generic;
using Behc.Mvp.Presenters;
using Behc.Mvp.Presenters.Factories;
using UnityEngine;

namespace Behc.Utils
{
	internal interface IPoolManager
	{
		IPresenter SpawnPresenter(object model);
		void DespawnPresenter(object model, IPresenter presenter);
		void ClearPool();
	}
	
	internal class PoolManager : IPoolManager
	{
		private readonly IPresenterMap _presenterMap;
		private readonly RectTransform _rootTransform;
		private readonly RectTransform _poolTransform;
		
		private Dictionary<IPresenterFactory, Stack<IPresenter>> _presentersPool;

		public PoolManager(IPresenterMap presenterMap, RectTransform rootTransform)
		{
			_presenterMap = presenterMap;
			_rootTransform = rootTransform;

			GameObject poolGameObject = new("PoolContainer");
			_poolTransform = poolGameObject.AddComponent<RectTransform>();
			_poolTransform.gameObject.AddComponent<Canvas>().enabled = false;
			_poolTransform.SetParent(_rootTransform.parent, false);
		}

		public IPresenter SpawnPresenter(object model)
		{
			IPresenterFactory factory = _presenterMap.TryGetPresenterFactory(model);
			
			if (factory == null)
				throw new Exception($"No PresenterFactory found for '{model.GetType().Name}' model!");

			IPresenter presenter = null;
			_presentersPool ??= new Dictionary<IPresenterFactory, Stack<IPresenter>>();
			if (_presentersPool.TryGetValue(factory, out Stack<IPresenter> pool) && pool.Count > 0)
			{
				presenter = pool.Pop();
				presenter.RectTransform.SetParent(_rootTransform, true);
			}

			presenter ??= factory.CreatePresenter(_rootTransform);
			presenter.SetUnbindPolicy(UnbindPolicy.KEEP_UNCHANGED);
			return presenter;
		}
		
		public void DespawnPresenter(object model, IPresenter presenter)
		{
			_presentersPool ??= new Dictionary<IPresenterFactory, Stack<IPresenter>>();
			IPresenterFactory factory = _presenterMap.TryGetPresenterFactory(model);

			if (!_presentersPool.TryGetValue(factory, out Stack<IPresenter> pool))
			{
				pool = new Stack<IPresenter>();
				_presentersPool.Add(factory, pool);
			}

			pool.Push(presenter);
			presenter.RectTransform.SetParent(_poolTransform, true);
		}

		public void ClearPool()
		{
			if (_presentersPool != null)
			{
				foreach (var kv in _presentersPool) 
				{
					foreach (IPresenter presenter in kv.Value)
					{
						kv.Key.DestroyPresenter(presenter);
					}
				}
                
				_presentersPool.Clear();
			}
		}
	}
}
