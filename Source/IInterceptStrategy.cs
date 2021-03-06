﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq.Proxy;
using System.Reflection;

namespace Moq
{
	internal enum InterceptionAction
	{
 		Continue,
        Stop
	}

	internal interface IInterceptStrategy
	{
		/// <summary>
		/// Handle interception
		/// </summary>
		/// <param name="invocation">the current invocation context</param>
		/// <param name="ctx">shared data among the strategies during an interception</param>
		/// <returns>true if further interception has to be processed, otherwise false</returns>
		InterceptionAction HandleIntercept(ICallContext invocation, InterceptStrategyContext ctx);
		
	}

	internal class InterceptStrategyContext
	{
		private Dictionary<string, List<Delegate>> invocationLists = new Dictionary<string, List<Delegate>>();
		private List<ICallContext> actualInvocations = new List<ICallContext>();
		private List<IProxyCall> orderedCalls = new List<IProxyCall>();

		public InterceptStrategyContext(Mock Mock, Type targetType, MockBehavior behavior)
		{
			this.Behavior = behavior;
			this.Mock = Mock;
			this.TargetType = targetType;
		}
		public Mock Mock { get; private set; }
		public Type TargetType { get; private set; }
		public MockBehavior Behavior { get; private set; }
		public IProxyCall CurrentCall { get; set; }
		
		#region InvocationLists
		internal IEnumerable<Delegate> GetInvocationList(EventInfo ev)
		{
			lock (invocationLists)
			{
				List<Delegate> handlers;
				if (!invocationLists.TryGetValue(ev.Name, out handlers))
				{
					return new Delegate[0];
				}

				return handlers.ToList();
			}
		}

		internal void AddEventHandler(EventInfo ev, Delegate handler)
		{
			lock (invocationLists)
			{
				List<Delegate> handlers;
				if (!invocationLists.TryGetValue(ev.Name, out handlers))
				{
					handlers = new List<Delegate>();
					invocationLists.Add(ev.Name, handlers);
				}

				handlers.Add(handler);
			}
		}
		internal void RemoveEventHandler(EventInfo ev, Delegate handler)
		{
			lock (invocationLists)
			{
				List<Delegate> handlers;
				if (invocationLists.TryGetValue(ev.Name, out handlers))
				{
					handlers.Remove(handler);
				}
			}
		}
		#endregion
		#region ActualInvocations
		internal void AddInvocation(ICallContext invocation)
		{
			lock (actualInvocations)
			{
				actualInvocations.Add(invocation);
			}
		}
		internal IEnumerable<ICallContext> ActualInvocations
		{
			get
			{
				lock (actualInvocations)
				{
					return actualInvocations.ToList();
				}
			}
		}
		internal void ClearInvocations()
		{
			lock (actualInvocations)
			{
				actualInvocations.Clear();
			}
		}
		#endregion
		#region OrderedCalls
		internal void AddOrderedCall(IProxyCall call)
		{
			lock (orderedCalls)
			{
				orderedCalls.Add(call);
			}
		}
		internal void RemoveOrderedCall(IProxyCall call)
		{
			lock (orderedCalls)
			{
				orderedCalls.Remove(call);
			}
		}
		internal IEnumerable<IProxyCall> OrderedCalls
		{
			get
			{
				lock (orderedCalls)
				{
					return orderedCalls.ToList();
				}
			}
		}
		#endregion

	}

}
