using System;

namespace DomainEventServices
{
	public class DomainEvent
	{
	}

	public delegate void DomainEventHandler(DomainEvent e);

	public interface IDomainEventDispatcher
	{
		public void RegisterEventHandler(Type eventType, DomainEventHandler handler);
		public void Raise(DomainEvent e);
	}
}
